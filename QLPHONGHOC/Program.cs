using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using QLPhongHoc.Data;
using QLPhongHoc.Models;
using QLPhongHoc.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<QLPhongHocContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddHttpContextAccessor();

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings.GetValue<string>("SecretKey") ?? throw new InvalidOperationException("JwtSettings:SecretKey is not configured.");
var issuer = jwtSettings.GetValue<string>("Issuer") ?? "QLHocSinhAPI";
var audience = jwtSettings.GetValue<string>("Audience") ?? "QLHocSinhClients";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultForbidScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ValidateIssuer = true,
        ValidIssuer = issuer,
        ValidateAudience = true,
        ValidAudience = audience,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero,
        RoleClaimType = "VaiTro"
    };
    options.Events = new JwtBearerEvents
    {
        OnChallenge = context =>
        {
            context.HandleResponse();
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";
            var isApi = context.HttpContext.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase);
            var payload = isApi
                ? System.Text.Json.JsonSerializer.Serialize(new
                {
                    success = false,
                    errorCode = "UNAUTHORIZED",
                    message = "Bạn chưa đăng nhập hoặc phiên đăng nhập đã hết hạn."
                })
                : System.Text.Json.JsonSerializer.Serialize(new
                {
                    success = false,
                    errorCode = "UNAUTHORIZED",
                    message = "Vui lòng đăng nhập để truy cập.",
                    redirectUrl = "/Account/Login"
                });
            return context.Response.WriteAsync(payload);
        },
        OnForbidden = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/json";
            var payload = System.Text.Json.JsonSerializer.Serialize(new
            {
                success = false,
                errorCode = "FORBIDDEN",
                message = "Bạn không có quyền truy cập vào tài nguyên này."
            });
            return context.Response.WriteAsync(payload);
        }
    };
});

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IHocSinhService, HocSinhService>();
builder.Services.AddScoped<IGiaoVienService, GiaoVienService>();
builder.Services.AddScoped<IDiemService, DiemService>();
builder.Services.AddScoped<ILopService, LopService>();
builder.Services.AddScoped<IMonHocService, MonHocService>();
builder.Services.AddScoped<INamHocService, NamHocService>();
builder.Services.AddScoped<IPhanCongService, PhanCongService>();
builder.Services.AddScoped<IExcelExportService, ExcelExportService>();
builder.Services.AddScoped<IZaloNotificationService, ZaloNotificationService>();

var app = builder.Build();

// ===== Self-healing DB: Migrate (optional) + hash plaintext passwords + ensure seed =====
try
{
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<QLPhongHocContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        await DbSeeder.EnsureSeedDataAsync(db, logger);
    }
}
catch (Exception ex)
{
    var bootstrapLogger = LoggerFactory.Create(b => b.AddConsole()).CreateLogger("Bootstrap");
    bootstrapLogger.LogWarning(ex, "DB seed/bootstrap skipped (database may be unavailable). Startup will continue.");
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStatusCodePages(context =>
{
    var ctx = context.HttpContext;
    var request = ctx.Request;
    var response = ctx.Response;
    var isApi = request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase)
                || string.Equals(request.Query["format"].FirstOrDefault(), "json", StringComparison.OrdinalIgnoreCase)
                || (request.Headers.Accept.Count > 0 && request.Headers.Accept.Any(a => a != null && a.Contains("application/json", StringComparison.OrdinalIgnoreCase)));

    if (response.StatusCode == StatusCodes.Status401Unauthorized)
    {
        if (isApi)
        {
            response.ContentType = "application/json";
            var body = System.Text.Json.JsonSerializer.Serialize(new
            {
                success = false,
                errorCode = "UNAUTHORIZED",
                message = "Bạn chưa đăng nhập hoặc phiên đăng nhập đã hết hạn."
            });
            return response.WriteAsync(body);
        }
        response.Redirect("/Account/Login?returnUrl=" + Uri.EscapeDataString(request.Path + request.QueryString));
        return Task.CompletedTask;
    }
    if (response.StatusCode == StatusCodes.Status403Forbidden)
    {
        if (isApi)
        {
            response.ContentType = "application/json";
            var body = System.Text.Json.JsonSerializer.Serialize(new
            {
                success = false,
                errorCode = "FORBIDDEN",
                message = "Bạn không có quyền truy cập vào tài nguyên này."
            });
            return response.WriteAsync(body);
        }
        response.Redirect("/Account/AccessDenied");
        return Task.CompletedTask;
    }
    return Task.CompletedTask;
});

app.UseStaticFiles();
app.UseSession();

// Tự động gắn JWT Token từ Session vào Header để [Authorize] hoạt động với MVC Views
// Fallback: nếu Session mất nhưng Request có JWT cookie hợp lệ → khôi phục lại Session (fix restart-login bug)
app.Use(async (context, next) =>
{
    var token = context.Session.GetString("JwtToken");
    if (string.IsNullOrEmpty(token))
    {
        if (context.Request.Cookies.TryGetValue("QLHS_JWT", out var cookieToken) && !string.IsNullOrEmpty(cookieToken))
        {
            token = cookieToken;
            context.Session.SetString("JwtToken", token);
            if (context.Request.Cookies.TryGetValue("QLHS_MaTK", out var maTkStr) && int.TryParse(maTkStr, out var maTk))
                context.Session.SetInt32("MaTaiKhoan", maTk);
            if (context.Request.Cookies.TryGetValue("QLHS_TenDN", out var tenDn))
                context.Session.SetString("TenDangNhap", tenDn);
            if (context.Request.Cookies.TryGetValue("QLHS_VaiTro", out var vaiTro))
                context.Session.SetString("VaiTro", vaiTro);
            if (context.Request.Cookies.TryGetValue("QLHS_HoTen", out var hoTen))
                context.Session.SetString("HoTen", hoTen);
        }
    }
    if (!string.IsNullOrEmpty(token) && !context.Request.Headers.ContainsKey("Authorization"))
    {
        context.Request.Headers.Append("Authorization", "Bearer " + token);
    }
    await next();
});

app.UseRouting();

// ====== Health check endpoint (anonymous) ======
app.MapGet("/api/health", (
    [FromServices] IConfiguration configuration,
    [FromServices] IWebHostEnvironment env,
    HttpContext ctx) =>
{
    var connStr = configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
    var masked = connStr.Length > 0 ? (connStr.Contains("Password=") ? "[REDACTED]" : connStr) : "(empty)";
    return new
    {
        status = "UP",
        app = "QLHocSinh",
        version = "1.0.0",
        environment = env.EnvironmentName,
        timeUtc = DateTime.UtcNow,
        database = new { configured = connStr.Length > 0, connectionString = masked }
    };
});

app.MapGet("/api/health/db", async ([FromServices] QLPhongHocContext db) =>
{
    try
    {
        var canConnect = await db.Database.CanConnectAsync();
        var taiKhoanCount = canConnect ? await db.TAIKHOAN.AsNoTracking().CountAsync() : 0;
        var hocSinhCount = canConnect ? await db.HOCSINH.AsNoTracking().CountAsync() : 0;
        var lopCount = canConnect ? await db.LOP.AsNoTracking().CountAsync() : 0;
        return Results.Ok(new
        {
            status = "UP",
            database = "OK",
            canConnect,
            counts = new { taiKhoan = taiKhoanCount, hocSinh = hocSinhCount, lop = lopCount }
        });
    }
    catch (Exception ex)
    {
        return Results.Problem(detail: ex.Message, title: "Database unhealthy", statusCode: 503);
    }
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();

// =============================================================================
// DbSeeder: self-healing helper (hash lại password plaintext, seed vai trò mẫu)
// =============================================================================
file static class DbSeeder
{
    public static async Task EnsureSeedDataAsync(QLPhongHocContext db, ILogger logger)
    {
        // 1. Đảm bảo DB có thể kết nối – KHÔNG tự Migrate (không phá DB production)
        bool canConnect;
        try { canConnect = await db.Database.CanConnectAsync(); }
        catch { canConnect = false; }
        if (!canConnect)
        {
            logger.LogWarning("Cannot connect to database on bootstrap. Skipping seed.");
            return;
        }

        // 2. Ensure VAITRO defaults (upsert-style)
        if (!await db.VAITRO.AnyAsync())
        {
            logger.LogInformation("Seeding VAITRO defaults...");
            db.VAITRO.AddRange(new List<VaiTro>
            {
                new() { TenVaiTro = "ADMIN", MoTa = "Quản trị hệ thống" },
                new() { TenVaiTro = "TEACHER", MoTa = "Giáo viên" },
                new() { TenVaiTro = "STUDENT", MoTa = "Học sinh" }
            });
            await db.SaveChangesAsync();
        }

        // 2.5. Ensure 3 demo accounts: admin / teacher / student (password 123456)
        const string DEMO_PASSWORD = "123456";
        var adminVt = await db.VAITRO.FirstOrDefaultAsync(v => v.TenVaiTro == "ADMIN");
        var teacherVt = await db.VAITRO.FirstOrDefaultAsync(v => v.TenVaiTro == "TEACHER");
        var studentVt = await db.VAITRO.FirstOrDefaultAsync(v => v.TenVaiTro == "STUDENT");

        async Task EnsureDemoAccountAsync(string username, VaiTro? vaiTro, string hoTen)
        {
            if (vaiTro == null) return;
            var existing = await db.TAIKHOAN.FirstOrDefaultAsync(t => t.TenDangNhap == username);
            bool needSave = false;
                if (existing == null)
            {
                existing = new TaiKhoan
                {
                    TenDangNhap = username,
                    MatKhau = DEMO_PASSWORD,
                    HoTen = hoTen,
                    Email = $"{username}@qlhocsinh.edu.vn",
                    NgayTao = DateTime.Now,
                    MaVaiTro = vaiTro.MaVaiTro,
                    TrangThai = true
                };
                db.TAIKHOAN.Add(existing);
                needSave = true;
            }
            else
            {
                if (existing.MaVaiTro != vaiTro.MaVaiTro) { existing.MaVaiTro = vaiTro.MaVaiTro; needSave = true; }
                if (!existing.TrangThai) { existing.TrangThai = true; needSave = true; }
                if (string.IsNullOrWhiteSpace(existing.HoTen)) { existing.HoTen = hoTen; needSave = true; }
            }
            // Verify password → nếu không khớp DEMO_PASSWORD thì reset
            bool pwOk = false;
            try
            {
                if (!string.IsNullOrWhiteSpace(existing.MatKhau) &&
                    (existing.MatKhau.StartsWith("$2a$") || existing.MatKhau.StartsWith("$2b$")))
                    pwOk = BCrypt.Net.BCrypt.Verify(DEMO_PASSWORD, existing.MatKhau);
            }
            catch { pwOk = false; }
            if (!pwOk)
            {
                existing.MatKhau = DEMO_PASSWORD; // plaintext → step 3 sẽ auto-hash
                needSave = true;
            }
            if (needSave)
                await db.SaveChangesAsync();

            // Link account student ↔ HOCSINH (HS đầu tiên chưa có MaTaiKhoan)
            if (username == "student")
            {
                var hsLinked = await db.HOCSINH.FirstOrDefaultAsync(h => h.MaTaiKhoan == existing.MaTaiKhoan);
                if (hsLinked == null)
                {
                    var firstFreeHs = await db.HOCSINH.OrderBy(h => h.MaHocSinh)
                        .FirstOrDefaultAsync(h => h.MaTaiKhoan == null || h.MaTaiKhoan == 0);
                    if (firstFreeHs != null) { firstFreeHs.MaTaiKhoan = existing.MaTaiKhoan; await db.SaveChangesAsync(); }
                }
            }
            // Link account teacher ↔ GIAOVIEN (GV đầu tiên chưa có MaTaiKhoan)
            if (username == "teacher")
            {
                var gvLinked = await db.GIAOVIEN.FirstOrDefaultAsync(g => g.MaTaiKhoan == existing.MaTaiKhoan);
                if (gvLinked == null)
                {
                    var firstFreeGv = await db.GIAOVIEN.OrderBy(g => g.MaGiaoVien)
                        .FirstOrDefaultAsync(g => g.MaTaiKhoan == null || g.MaTaiKhoan == 0);
                    if (firstFreeGv != null) { firstFreeGv.MaTaiKhoan = existing.MaTaiKhoan; await db.SaveChangesAsync(); }
                }
            }
        }

        await EnsureDemoAccountAsync("admin", adminVt, "Quản Trị Viên");
        await EnsureDemoAccountAsync("teacher", teacherVt, "Giáo Viên Mẫu");
        await EnsureDemoAccountAsync("student", studentVt, "Học Sinh Mẫu");

        // 3. Self-healing: hash lại passwords dạng plaintext (chưa hash)
        //    BCrypt hash luôn dài ~60 chars, version "$2a$" / "$2b$"
        var allAccounts = await db.TAIKHOAN.ToListAsync();
        int fixedCount = 0;
        foreach (var acc in allAccounts)
        {
            if (string.IsNullOrWhiteSpace(acc.MatKhau)) continue;
            if (acc.MatKhau.StartsWith("$2a$") || acc.MatKhau.StartsWith("$2b$") || acc.MatKhau.Length >= 55) continue;
            try
            {
                acc.MatKhau = BCrypt.Net.BCrypt.HashPassword(acc.MatKhau, workFactor: 12);
                fixedCount++;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to hash MatKhau for account {MaTK}", acc.MaTaiKhoan);
            }
        }
        if (fixedCount > 0)
        {
            logger.LogInformation("Hashed {N} plaintext password(s) in TAIKHOAN table.", fixedCount);
            await db.SaveChangesAsync();
        }
    }
}
