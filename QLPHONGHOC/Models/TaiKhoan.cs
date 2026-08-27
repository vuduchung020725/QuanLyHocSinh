using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLPhongHoc.Models
{
    [Table("TAIKHOAN")]
    public class TaiKhoan
    {
        [Key]
        public int MaTaiKhoan { get; set; }

        [StringLength(50)]
        public string TenDangNhap { get; set; } = string.Empty;

        [StringLength(255)]
        public string MatKhau { get; set; } = string.Empty;

        [StringLength(100)]
        public string HoTen { get; set; } = string.Empty;

        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [StringLength(15)]
        public string? SoDienThoai { get; set; }

        public int MaVaiTro { get; set; }
        [ForeignKey("MaVaiTro")]
        public VaiTro VaiTro { get; set; } = null!;

        public bool TrangThai { get; set; } = true;

        public DateTime NgayTao { get; set; } = DateTime.Now;
    }
}
