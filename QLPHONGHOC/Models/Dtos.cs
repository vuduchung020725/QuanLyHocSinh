using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace QLPhongHoc.Models
{
    // ==========================================
    // PAGINATION HELPER (GIỮ NGUYÊN)
    // ==========================================
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new List<T>();
        public int TotalItems { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
    }

    // ==========================================
    // MODULE 1: ACCOUNT DTOS (GIỮ NGUYÊN)
    // ==========================================
    public class AccountDto
    {
        public int MaTaiKhoan { get; set; }
        public string TenDangNhap { get; set; } = string.Empty;
        public string HoTen { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? SoDienThoai { get; set; }
        public int MaVaiTro { get; set; }
        public string TenVaiTro { get; set; } = string.Empty;
        public bool TrangThai { get; set; }
    }

    public class UpdateAccountRequest
    {
        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        [StringLength(100, ErrorMessage = "Họ tên tối đa 100 ký tự")]
        public string HoTen { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [StringLength(100, ErrorMessage = "Email tối đa 100 ký tự")]
        public string Email { get; set; } = string.Empty;

        [StringLength(15, ErrorMessage = "Số điện thoại tối đa 15 ký tự")]
        public string? SoDienThoai { get; set; }

        [Required(ErrorMessage = "Vai trò là bắt buộc")]
        public int MaVaiTro { get; set; }

        public bool TrangThai { get; set; } = true;
    }

    public class ApproveAccountRequest
    {
        [Required(ErrorMessage = "Trạng thái duyệt là bắt buộc")]
        public bool TrangThai { get; set; }
    }

    // ==========================================
    // MODULE 2: GIAOVIEN DTOS
    // ==========================================
    public class GiaoVienDto
    {
        public int MaGiaoVien { get; set; }
        public int? MaTaiKhoan { get; set; }
        public string MaGV { get; set; } = string.Empty;
        public string HoTen { get; set; } = string.Empty;
        public DateOnly? NgaySinh { get; set; }
        public string? GioiTinh { get; set; }
        public string? SoDienThoai { get; set; }
        public string? Email { get; set; }
        public string? DiaChi { get; set; }
        public string ChuyenMon { get; set; } = string.Empty;
        public bool TrangThai { get; set; }
    }

    public class CreateGiaoVienRequest
    {
        [Required(ErrorMessage = "Mã giáo viên là bắt buộc")]
        [StringLength(20, ErrorMessage = "Mã giáo viên tối đa 20 ký tự")]
        public string MaGV { get; set; } = string.Empty;

        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        [StringLength(100, ErrorMessage = "Họ tên tối đa 100 ký tự")]
        public string HoTen { get; set; } = string.Empty;

        public DateOnly? NgaySinh { get; set; }

        [StringLength(10)]
        public string? GioiTinh { get; set; }

        [StringLength(15, ErrorMessage = "Số điện thoại tối đa 15 ký tự")]
        [RegularExpression(@"^[0-9\+\-\s]{7,15}$", ErrorMessage = "Số điện thoại không hợp lệ")]
        public string? SoDienThoai { get; set; }

        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(255)]
        public string? DiaChi { get; set; }

        [Required(ErrorMessage = "Chuyên môn là bắt buộc")]
        [StringLength(100)]
        public string ChuyenMon { get; set; } = string.Empty;

        public bool TrangThai { get; set; } = true;

        public int? MaTaiKhoan { get; set; }
    }

    public class UpdateGiaoVienRequest : CreateGiaoVienRequest { }

    // ==========================================
    // MODULE 3: HOCSINH DTOS
    // ==========================================
    public class HocSinhDto
    {
        public int MaHocSinh { get; set; }
        public int? MaTaiKhoan { get; set; }
        public string MaHS { get; set; } = string.Empty;
        public string HoTen { get; set; } = string.Empty;
        public DateOnly NgaySinh { get; set; }
        public string GioiTinh { get; set; } = string.Empty;
        public string? DiaChi { get; set; }
        public string? SoDienThoai { get; set; }
        public string? Email { get; set; }
        public int MaLop { get; set; }
        public string TenLop { get; set; } = string.Empty;
        public bool TrangThai { get; set; }
    }

    public class CreateHocSinhRequest
    {
        [Required(ErrorMessage = "Mã học sinh là bắt buộc")]
        [StringLength(20, ErrorMessage = "Mã học sinh tối đa 20 ký tự")]
        public string MaHS { get; set; } = string.Empty;

        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        [StringLength(100, ErrorMessage = "Họ tên tối đa 100 ký tự")]
        public string HoTen { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ngày sinh là bắt buộc")]
        public DateOnly NgaySinh { get; set; }

        [Required(ErrorMessage = "Giới tính là bắt buộc")]
        [StringLength(10)]
        public string GioiTinh { get; set; } = string.Empty;

        [StringLength(255)]
        public string? DiaChi { get; set; }

        [StringLength(15)]
        [RegularExpression(@"^[0-9\+\-\s]{7,15}$", ErrorMessage = "Số điện thoại không hợp lệ")]
        public string? SoDienThoai { get; set; }

        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [StringLength(100)]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Lớp là bắt buộc")]
        public int MaLop { get; set; }

        public bool TrangThai { get; set; } = true;

        public int? MaTaiKhoan { get; set; }
    }

    public class UpdateHocSinhRequest : CreateHocSinhRequest { }

    // ==========================================
    // MODULE 4: LOP DTOS
    // ==========================================
    public class LopDto
    {
        public int MaLop { get; set; }
        public string MaLopCode { get; set; } = string.Empty;
        public string TenLop { get; set; } = string.Empty;
        public int Khoi { get; set; }
        public int MaNamHoc { get; set; }
        public string TenNamHoc { get; set; } = string.Empty;
        public int? MaGVCN { get; set; }
        public string? TenGVCN { get; set; }
        public int SiSo { get; set; }
        public bool TrangThai { get; set; }
    }

    public class CreateLopRequest
    {
        [Required(ErrorMessage = "Mã lớp là bắt buộc")]
        [StringLength(20, ErrorMessage = "Mã lớp tối đa 20 ký tự")]
        public string MaLopCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên lớp là bắt buộc")]
        [StringLength(50)]
        public string TenLop { get; set; } = string.Empty;

        [Required(ErrorMessage = "Khối là bắt buộc")]
        public int Khoi { get; set; }

        [Required(ErrorMessage = "Năm học là bắt buộc")]
        public int MaNamHoc { get; set; }

        public int? MaGVCN { get; set; }

        public bool TrangThai { get; set; } = true;
    }

    public class UpdateLopRequest : CreateLopRequest { }

    // ==========================================
    // MODULE 5: MONHOC DTOS
    // ==========================================
    public class MonHocDto
    {
        public int MaMonHoc { get; set; }
        public string MaMon { get; set; } = string.Empty;
        public string TenMon { get; set; } = string.Empty;
        public int SoTiet { get; set; }
        public string? GhiChu { get; set; }
    }

    public class CreateMonHocRequest
    {
        [Required(ErrorMessage = "Mã môn học là bắt buộc")]
        [StringLength(20)]
        public string MaMon { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên môn học là bắt buộc")]
        [StringLength(100)]
        public string TenMon { get; set; } = string.Empty;

        [Range(0, 500, ErrorMessage = "Số tiết phải từ 0 đến 500")]
        public int SoTiet { get; set; } = 0;

        [StringLength(255)]
        public string? GhiChu { get; set; }
    }

    public class UpdateMonHocRequest : CreateMonHocRequest { }

    // ==========================================
    // MODULE 6: NAMHOC DTOS
    // ==========================================
    public class NamHocDto
    {
        public int MaNamHoc { get; set; }
        public string TenNamHoc { get; set; } = string.Empty;
        public bool TrangThai { get; set; }
    }

    // ==========================================
    // MODULE 7: HOCKY DTOS
    // ==========================================
    public class HocKyDto
    {
        public int MaHocKy { get; set; }
        public string TenHocKy { get; set; } = string.Empty;
        public int MaNamHoc { get; set; }
        public string TenNamHoc { get; set; } = string.Empty;
        public DateOnly TuNgay { get; set; }
        public DateOnly DenNgay { get; set; }
    }

    // ==========================================
    // MODULE 8: DIEM DTOS
    // ==========================================
    public class DiemDto
    {
        public int MaDiem { get; set; }
        public int MaHocSinh { get; set; }
        public string MaHS { get; set; } = string.Empty;
        public string TenHocSinh { get; set; } = string.Empty;
        public string TenLop { get; set; } = string.Empty;
        public int MaMonHoc { get; set; }
        public string TenMon { get; set; } = string.Empty;
        public int MaHocKy { get; set; }
        public string TenHocKy { get; set; } = string.Empty;
        public decimal? DiemMieng { get; set; }
        public decimal? Diem15Phut { get; set; }
        public decimal? DiemGiuaKy { get; set; }
        public decimal? DiemCuoiKy { get; set; }
        public decimal? DiemTrungBinh { get; set; }
        public string? XepLoai { get; set; }
        public DateTime NgayCapNhat { get; set; }
        public string? TenGiaoVienNhap { get; set; }
    }

    public class NhapDiemRequest
    {
        [Required(ErrorMessage = "Học sinh là bắt buộc")]
        public int MaHocSinh { get; set; }

        [Required(ErrorMessage = "Môn học là bắt buộc")]
        public int MaMonHoc { get; set; }

        [Required(ErrorMessage = "Học kỳ là bắt buộc")]
        public int MaHocKy { get; set; }

        [Range(0, 10, ErrorMessage = "Điểm miệng phải từ 0 đến 10")]
        public decimal? DiemMieng { get; set; }

        [Range(0, 10, ErrorMessage = "Điểm 15 phút phải từ 0 đến 10")]
        public decimal? Diem15Phut { get; set; }

        [Range(0, 10, ErrorMessage = "Điểm giữa kỳ phải từ 0 đến 10")]
        public decimal? DiemGiuaKy { get; set; }

        [Range(0, 10, ErrorMessage = "Điểm cuối kỳ phải từ 0 đến 10")]
        public decimal? DiemCuoiKy { get; set; }
    }

    // Dùng khi nhập điểm hàng loạt cho một lớp
    public class NhapDiemHangLoatRequest
    {
        [Required]
        public int MaMonHoc { get; set; }
        [Required]
        public int MaHocKy { get; set; }
        [Required]
        public int MaLop { get; set; }
        public List<NhapDiemRequest> DanhSachDiem { get; set; } = new();
    }

    // ==========================================
    // MODULE 9: BAOCAO (Dashboard + Thống kê)
    // ==========================================
    public class DashboardSummaryDto
    {
        public int TongSoHocSinh { get; set; }
        public int TongSoGiaoVien { get; set; }
        public int TongSoLop { get; set; }
        public int TongSoMonHoc { get; set; }
        public string? TenNamHocHienTai { get; set; }
        public string? TenHocKyHienTai { get; set; }
        public DateTime ThoiGianCapNhat { get; set; }
    }

    public class HocLucThongKeDto
    {
        public string XepLoai { get; set; } = string.Empty;
        public int SoLuong { get; set; }
        public double TyLe { get; set; }
    }

    public class HocSinhKetQuaDto
    {
        public int MaHocSinh { get; set; }
        public string MaHS { get; set; } = string.Empty;
        public string HoTen { get; set; } = string.Empty;
        public string TenLop { get; set; } = string.Empty;
        public decimal? DiemTrungBinhChung { get; set; }   // TBChung = Trung bình các môn
        public string? XepLoaiChung { get; set; }
        public int SoMonHoc { get; set; }
        public int SoMonDaNhap { get; set; }
    }

    public class PhanCongDto
    {
        public int MaPhanCong { get; set; }
        public int MaGiaoVien { get; set; }
        public string TenGiaoVien { get; set; } = string.Empty;
        public int MaMonHoc { get; set; }
        public string TenMon { get; set; } = string.Empty;
        public int MaLop { get; set; }
        public string TenLop { get; set; } = string.Empty;
        public int MaNamHoc { get; set; }
        public string TenNamHoc { get; set; } = string.Empty;
        public int HocKy { get; set; } // 1, 2, 3
    }

    public class CreatePhanCongRequest
    {
        [Required] public int MaGiaoVien { get; set; }
        [Required] public int MaMonHoc { get; set; }
        [Required] public int MaLop { get; set; }
        [Required] public int MaNamHoc { get; set; }
        [Required] public int HocKy { get; set; } // 1, 2, 3
    }
}
