using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using QLPhongHoc.Data;
using QLPhongHoc.Models;
using QLPhongHoc.Models.Auth;

namespace QLPhongHoc.Services
{
    /// <summary>
    /// Service quản lý xác thực (Authentication)
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Đăng nhập người dùng
        /// </summary>
        Task<LoginResponse> LoginAsync(LoginRequest request);

        /// <summary>
        /// Đăng ký người dùng mới
        /// </summary>
        Task<RegisterResponse> RegisterAsync(RegisterRequest request);

        /// <summary>
        /// Hash mật khẩu
        /// </summary>
        string HashPassword(string password);

        /// <summary>
        /// Verify mật khẩu
        /// </summary>
        bool VerifyPassword(string password, string hash);
    }

    public class AuthService : IAuthService
    {
        private readonly QLPhongHocContext _context;
        private readonly ITokenService _tokenService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            QLPhongHocContext context,
            ITokenService tokenService,
            ILogger<AuthService> logger)
        {
            _context = context;
            _tokenService = tokenService;
            _logger = logger;
        }

        /// <summary>
        /// Đăng nhập người dùng
        /// </summary>
        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(request.TenDangNhap) || string.IsNullOrWhiteSpace(request.MatKhau))
                {
                    _logger.LogWarning("Login attempt with empty credentials");
                    return new LoginResponse
                    {
                        Success = false,
                        Message = "Vui lòng nhập tên đăng nhập và mật khẩu.",
                        ErrorCode = "EMPTY_CREDENTIALS"
                    };
                }

                // Tìm tài khoản theo tên đăng nhập
                var user = await _context.TAIKHOAN
                    .Include(t => t.VaiTro)
                    .FirstOrDefaultAsync(u => u.TenDangNhap == request.TenDangNhap);

                // Kiểm tra tài khoản tồn tại
                if (user == null)
                {
                    _logger.LogWarning($"Login attempt with non-existent username: {request.TenDangNhap}");
                    return new LoginResponse
                    {
                        Success = false,
                        Message = "Tên đăng nhập hoặc mật khẩu không đúng.",
                        ErrorCode = "INVALID_CREDENTIALS"
                    };
                }

                // Kiểm tra mật khẩu
                if (!VerifyPassword(request.MatKhau, user.MatKhau))
                {
                    _logger.LogWarning($"Login attempt with wrong password for user: {request.TenDangNhap}");
                    return new LoginResponse
                    {
                        Success = false,
                        Message = "Tên đăng nhập hoặc mật khẩu không đúng.",
                        ErrorCode = "INVALID_CREDENTIALS"
                    };
                }

                // Kiểm tra trạng thái tài khoản
                var statusResponse = ValidateAccountStatus(user);
                if (!statusResponse.Success)
                {
                    return statusResponse;
                }

                // Tạo JWT Token
                var token = _tokenService.GenerateToken(user);
                var jwtSettings = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json")
                    .Build()
                    .GetSection("JwtSettings");
                var expiryMinutes = jwtSettings.GetValue<int>("ExpiryMinutes", 60);

                _logger.LogInformation($"User logged in successfully: {user.TenDangNhap}");

                return new LoginResponse
                {
                    Success = true,
                    Message = "Đăng nhập thành công.",
                    Token = token,
                    ExpiresIn = expiryMinutes * 60,
                    UserInfo = new UserInfoDto
                    {
                        MaTaiKhoan = user.MaTaiKhoan,
                        TenDangNhap = user.TenDangNhap,
                        HoTen = user.HoTen,
                        Email = user.Email,
                        SoDienThoai = user.SoDienThoai,
                        MaVaiTro = user.MaVaiTro,
                        TenVaiTro = user.VaiTro?.TenVaiTro,
                        TrangThai = user.TrangThai ? "Hoạt động" : "Khóa"
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during login: {ex.Message}");
                return new LoginResponse
                {
                    Success = false,
                    Message = "Đã xảy ra lỗi trong quá trình đăng nhập.",
                    ErrorCode = "LOGIN_ERROR"
                };
            }
        }

        /// <summary>
        /// Đăng ký người dùng mới
        /// </summary>
        public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(request.TenDangNhap) ||
                    string.IsNullOrWhiteSpace(request.MatKhau) ||
                    string.IsNullOrWhiteSpace(request.HoTen) ||
                    string.IsNullOrWhiteSpace(request.Email))
                {
                    return new RegisterResponse
                    {
                        Success = false,
                        Message = "Vui lòng điền đầy đủ thông tin.",
                        ErrorCode = "INCOMPLETE_DATA"
                    };
                }

                // Kiểm tra mật khẩu khớp
                if (request.MatKhau != request.XacNhanMatKhau)
                {
                    return new RegisterResponse
                    {
                        Success = false,
                        Message = "Mật khẩu xác nhận không khớp.",
                        ErrorCode = "PASSWORD_MISMATCH"
                    };
                }

                // Kiểm tra mật khẩu đủ mạnh (tối thiểu 6 ký tự)
                if (request.MatKhau.Length < 6)
                {
                    return new RegisterResponse
                    {
                        Success = false,
                        Message = "Mật khẩu phải có ít nhất 6 ký tự.",
                        ErrorCode = "WEAK_PASSWORD"
                    };
                }

                // Kiểm tra username tồn tại
                var existingUsername = await _context.TAIKHOAN
                    .FirstOrDefaultAsync(u => u.TenDangNhap == request.TenDangNhap);
                if (existingUsername != null)
                {
                    return new RegisterResponse
                    {
                        Success = false,
                        Message = "Tên đăng nhập đã tồn tại.",
                        ErrorCode = "USERNAME_EXISTS"
                    };
                }

                // Kiểm tra email tồn tại
                var existingEmail = await _context.TAIKHOAN
                    .FirstOrDefaultAsync(u => u.Email == request.Email);
                if (existingEmail != null)
                {
                    return new RegisterResponse
                    {
                        Success = false,
                        Message = "Email đã được sử dụng.",
                        ErrorCode = "EMAIL_EXISTS"
                    };
                }

                // Tạo tài khoản mới
                var newUser = new TaiKhoan
                {
                    TenDangNhap = request.TenDangNhap,
                    MatKhau = HashPassword(request.MatKhau),
                    HoTen = request.HoTen,
                    Email = request.Email,
                    SoDienThoai = request.SoDienThoai,
                    MaVaiTro = 2, // Vai trò mặc định (User)
                    TrangThai = false // Trạng thái mặc định (Chờ duyệt)
                };

                _context.TAIKHOAN.Add(newUser);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"User registered successfully: {request.TenDangNhap}");

                return new RegisterResponse
                {
                    Success = true,
                    Message = "Đăng ký thành công! Vui lòng chờ phê duyệt từ quản trị viên.",
                    MaTaiKhoan = newUser.MaTaiKhoan
                };
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError($"Database error during registration: {ex.Message}");
                return new RegisterResponse
                {
                    Success = false,
                    Message = "Đã xảy ra lỗi cơ sở dữ liệu.",
                    ErrorCode = "DATABASE_ERROR"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during registration: {ex.Message}");
                return new RegisterResponse
                {
                    Success = false,
                    Message = "Đã xảy ra lỗi trong quá trình đăng ký.",
                    ErrorCode = "REGISTER_ERROR"
                };
            }
        }

        /// <summary>
        /// Hash mật khẩu sử dụng BCrypt
        /// </summary>
        public string HashPassword(string password)
        {
            try
            {
                return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error hashing password: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Verify mật khẩu với hash
        /// </summary>
        public bool VerifyPassword(string password, string hash)
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(password, hash);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error verifying password: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Validate trạng thái tài khoản
        /// </summary>
        private LoginResponse ValidateAccountStatus(TaiKhoan user)
        {
            if (!user.TrangThai)
            {
                _logger.LogWarning($"Login attempt with pending or locked account: {user.TenDangNhap}");
                return new LoginResponse
                {
                    Success = false,
                    Message = "Tài khoản của bạn đang chờ phê duyệt hoặc đã bị khóa.",
                    ErrorCode = "ACCOUNT_DISABLED"
                };
            }
            return new LoginResponse { Success = true };
        }
    }
}
