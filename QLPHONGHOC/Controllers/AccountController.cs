using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLPhongHoc.Data;
using QLPhongHoc.Helpers;
using QLPhongHoc.Models;
using QLPhongHoc.Models.Auth;
using QLPhongHoc.Services;

namespace QLPhongHoc.Controllers
{
    public class AccountController : Controller
    {
        private readonly QLPhongHocContext _context;
        private readonly IAuthService _authService;
        private readonly IAccountService _accountService;
        private readonly ILogger<AccountController> _logger;

        private const string COOKIE_JWT = "QLHS_JWT";
        private const string COOKIE_MATK = "QLHS_MaTK";
        private const string COOKIE_TENDN = "QLHS_TenDN";
        private const string COOKIE_VAITRO = "QLHS_VaiTro";
        private const string COOKIE_HOTEN = "QLHS_HoTen";
        private static readonly TimeSpan CookieLifetime = TimeSpan.FromHours(24);

        public AccountController(
            QLPhongHocContext context,
            IAuthService authService,
            IAccountService accountService,
            ILogger<AccountController> logger)
        {
            _context = context;
            _authService = authService;
            _accountService = accountService;
            _logger = logger;
        }

        private void PersistAuthCookies(LoginResponse response, bool ghiNho)
        {
            var opts = new CookieOptions
            {
                HttpOnly = true,
                IsEssential = true,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                Expires = ghiNho ? DateTimeOffset.Now.Add(CookieLifetime) : null
            };
            if (response.UserInfo != null)
            {
                Response.Cookies.Append(COOKIE_JWT, response.Token ?? string.Empty, opts);
                Response.Cookies.Append(COOKIE_MATK, response.UserInfo.MaTaiKhoan.ToString(), opts);
                Response.Cookies.Append(COOKIE_TENDN, response.UserInfo.TenDangNhap ?? string.Empty, opts);
                Response.Cookies.Append(COOKIE_VAITRO, response.UserInfo.TenVaiTro ?? string.Empty, opts);
                Response.Cookies.Append(COOKIE_HOTEN, response.UserInfo.HoTen ?? string.Empty, opts);
            }
        }

        private void SaveSessionForLogin(LoginResponse response)
        {
            if (response.UserInfo != null)
            {
                Microsoft.AspNetCore.Http.SessionExtensions.SetString(
                    HttpContext.Session,
                    SessionHelper.SessionUserId,
                    response.UserInfo.MaTaiKhoan.ToString());

                Microsoft.AspNetCore.Http.SessionExtensions.SetString(
                    HttpContext.Session,
                    SessionHelper.SessionHoTen,
                    response.UserInfo.HoTen ?? string.Empty);

                Microsoft.AspNetCore.Http.SessionExtensions.SetString(
                    HttpContext.Session,
                    SessionHelper.SessionTenVaiTro,
                    response.UserInfo.TenVaiTro ?? string.Empty);

                Microsoft.AspNetCore.Http.SessionExtensions.SetString(
                    HttpContext.Session,
                    SessionHelper.SessionMaVaiTro,
                    response.UserInfo.MaVaiTro.ToString());

                Microsoft.AspNetCore.Http.SessionExtensions.SetString(
                    HttpContext.Session,
                    "JwtToken",
                    response.Token ?? string.Empty);
            }
        }

        private void ClearAuthCookies()
        {
            foreach (var key in new[] { COOKIE_JWT, COOKIE_MATK, COOKIE_TENDN, COOKIE_VAITRO, COOKIE_HOTEN })
            {
                if (Request.Cookies.ContainsKey(key))
                    Response.Cookies.Delete(key, new CookieOptions { Secure = Request.IsHttps, SameSite = SameSiteMode.Lax });
            }
        }

        /// <summary>
        /// GET: /Account/Login - Hiển thị trang đăng nhập
        /// </summary>
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        /// <summary>
        /// POST: /Account/Login - Xử lý đăng nhập
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string TenDangNhap, string MatKhau, bool GhiNho = false)
        {
            var loginRequest = new LoginRequest
            {
                TenDangNhap = TenDangNhap,
                MatKhau = MatKhau,
                GhiNho = GhiNho
            };

            var response = await _authService.LoginAsync(loginRequest);

            if (!response.Success)
            {
                ViewBag.Error = response.Message;
                return View();
            }

            SaveSessionForLogin(response);
            PersistAuthCookies(response, GhiNho);

            _logger.LogInformation("User {TenDangNhap} logged in successfully", response.UserInfo?.TenDangNhap ?? "unknown");
            TempData["Success"] = "Đăng nhập thành công.";

            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// POST: /api/Account/Login - API endpoint cho đăng nhập (hỗ trợ JSON)
        /// </summary>
        [HttpPost]
        [Route("api/Account/Login")]
        public async Task<IActionResult> ApiLogin([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiErrorResponse
                {
                    ErrorCode = "INVALID_INPUT",
                    Message = "Dữ liệu đầu vào không hợp lệ",
                    Details = ModelState
                });
            }

            var response = await _authService.LoginAsync(request);

            if (!response.Success)
            {
                return Unauthorized(response);
            }

            SaveSessionForLogin(response);
            PersistAuthCookies(response, request.GhiNho);

            return Ok(response);
        }

        /// <summary>
        /// GET: /Account/Logout - Đăng xuất
        /// </summary>
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            ClearAuthCookies();
            _logger.LogInformation("User logged out");
            return RedirectToAction("Login", "Account");
        }

        /// <summary>
        /// GET: /Account/Register - Hiển thị trang đăng ký
        /// </summary>
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        /// <summary>
        /// POST: /Account/Register - Xử lý đăng ký
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            var response = await _authService.RegisterAsync(request);

            if (!response.Success)
            {
                ViewBag.Error = response.Message;
                return View();
            }

            _logger.LogInformation($"User {request.TenDangNhap} registered successfully");
            TempData["Success"] = response.Message;

            return RedirectToAction("Login", "Account");
        }

        /// <summary>
        /// POST: /api/Account/Register - API endpoint cho đăng ký
        /// </summary>
        [HttpPost]
        [Route("api/Account/Register")]
        public async Task<IActionResult> ApiRegister([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ApiErrorResponse
                {
                    ErrorCode = "INVALID_INPUT",
                    Message = "Dữ liệu đầu vào không hợp lệ",
                    Details = ModelState
                });
            }

            var response = await _authService.RegisterAsync(request);

            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        /// <summary>
        /// GET: /Account/ManageUsers - Giao diện quản lý tài khoản & phê duyệt (Admin)
        /// </summary>
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> ManageUsers(string? keyword, bool? trangThai, int? maVaiTro, int page = 1, int size = 15)
        {
            ViewData["ActivePage"] = "TaiKhoan";
            ViewData["Title"] = "Quản lý tài khoản & Phê duyệt";
            ViewData["Keyword"] = keyword;
            ViewData["TrangThai"] = trangThai;
            ViewData["MaVaiTro"] = maVaiTro;
            ViewData["VaiTros"] = await _context.VAITRO.AsNoTracking().ToListAsync();

            var accounts = await _accountService.GetAccountsAsync(keyword, trangThai, maVaiTro, page, size);
            return View(accounts);
        }

        /// <summary>
        /// POST: /Account/ApproveUser - Phê duyệt tài khoản (Pending -> Active)
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> ApproveUser(int id)
        {
            try
            {
                var success = await _accountService.ApproveAccountAsync(id, new ApproveAccountRequest { TrangThai = true });
                if (success)
                {
                    TempData["Success"] = "Phê duyệt tài khoản thành công! Người dùng đã có thể đăng nhập.";
                }
                else
                {
                    TempData["Error"] = "Không tìm thấy tài khoản để phê duyệt.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction(nameof(ManageUsers));
        }

        /// <summary>
        /// POST: /Account/ToggleUserStatus - Khóa hoặc mở khóa tài khoản
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> ToggleUserStatus(int id)
        {
            try
            {
                var user = await _context.TAIKHOAN.FindAsync(id);
                if (user == null)
                {
                    TempData["Error"] = "Không tìm thấy tài khoản.";
                }
                else
                {
                    user.TrangThai = !user.TrangThai;
                    await _context.SaveChangesAsync();
                    TempData["Success"] = user.TrangThai ? "Đã kích hoạt/mở khóa tài khoản thành công!" : "Đã khóa tài khoản thành công!";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction(nameof(ManageUsers));
        }

        /// <summary>
        /// POST: /Account/DeleteUser - Xóa / Vô hiệu hóa tài khoản
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var success = await _accountService.DeleteAccountAsync(id);
                if (success)
                {
                    TempData["Success"] = "Xóa/vô hiệu hóa tài khoản thành công!";
                }
                else
                {
                    TempData["Error"] = "Không tìm thấy tài khoản.";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
            }
            return RedirectToAction(nameof(ManageUsers));
        }
    }
}
