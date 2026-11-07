namespace QLPhongHoc.Models.Auth
{
    /// <summary>
    /// Model nhận dữ liệu đăng nhập từ client
    /// </summary>
    public class LoginRequest
    {
        /// <summary>Tên đăng nhập (Username)</summary>
        public string TenDangNhap { get; set; } = string.Empty;

        /// <summary>Mật khẩu</summary>
        public string MatKhau { get; set; } = string.Empty;

        /// <summary>Ghi nhớ đăng nhập (optional)</summary>
        public bool GhiNho { get; set; }
    }

    /// <summary>
    /// Model trả về kết quả đăng nhập
    /// </summary>
    public class LoginResponse
    {
        /// <summary>Trạng thái thành công/thất bại</summary>
        public bool Success { get; set; }

        /// <summary>Thông báo (lỗi hoặc thành công)</summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>Mã lỗi (nếu có)</summary>
        public string? ErrorCode { get; set; }

        /// <summary>JWT Token (nếu đăng nhập thành công)</summary>
        public string? Token { get; set; }

        /// <summary>Thời gian hết hạn token (tính bằng giây)</summary>
        public int ExpiresIn { get; set; }

        /// <summary>Thông tin người dùng đã đăng nhập</summary>
        public UserInfoDto? UserInfo { get; set; }
    }

    /// <summary>
    /// Model chứa thông tin người dùng sau khi đăng nhập
    /// </summary>
    public class UserInfoDto
    {
        /// <summary>Mã tài khoản</summary>
        public int MaTaiKhoan { get; set; }

        /// <summary>Tên đăng nhập</summary>
        public string TenDangNhap { get; set; } = string.Empty;

        /// <summary>Họ và tên</summary>
        public string HoTen { get; set; } = string.Empty;

        /// <summary>Email</summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>Số điện thoại</summary>
        public string? SoDienThoai { get; set; }

        /// <summary>Mã vai trò</summary>
        public int MaVaiTro { get; set; }

        /// <summary>Tên vai trò</summary>
        public string TenVaiTro { get; set; } = string.Empty;

        /// <summary>Trạng thái tài khoản</summary>
        public string TrangThai { get; set; } = string.Empty;
    }

    /// <summary>
    /// Model đăng ký tài khoản
    /// </summary>
    public class RegisterRequest
    {
        /// <summary>Tên đăng nhập</summary>
        public string TenDangNhap { get; set; } = string.Empty;

        /// <summary>Mật khẩu</summary>
        public string MatKhau { get; set; } = string.Empty;

        /// <summary>Xác nhận mật khẩu</summary>
        public string XacNhanMatKhau { get; set; } = string.Empty;

        /// <summary>Họ và tên</summary>
        public string HoTen { get; set; } = string.Empty;

        /// <summary>Email</summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>Số điện thoại</summary>
        public string? SoDienThoai { get; set; }
    }

    /// <summary>
    /// Model trả về kết quả đăng ký
    /// </summary>
    public class RegisterResponse
    {
        /// <summary>Trạng thái thành công/thất bại</summary>
        public bool Success { get; set; }

        /// <summary>Thông báo</summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>Mã lỗi (nếu có)</summary>
        public string? ErrorCode { get; set; }

        /// <summary>Mã tài khoản (nếu tạo thành công)</summary>
        public int? MaTaiKhoan { get; set; }
    }

    /// <summary>
    /// Model lỗi chung cho API
    /// </summary>
    public class ApiErrorResponse
    {
        /// <summary>Mã lỗi</summary>
        public string ErrorCode { get; set; } = string.Empty;

        /// <summary>Thông báo lỗi</summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>Chi tiết lỗi (nếu có)</summary>
        public object? Details { get; set; }
    }
}
