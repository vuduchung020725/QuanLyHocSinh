using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLPhongHoc.Models
{
    [Table("GIAOVIEN")]
    public class GiaoVien
    {
        [Key]
        public int MaGiaoVien { get; set; }

        public int? MaTaiKhoan { get; set; }
        [ForeignKey("MaTaiKhoan")]
        public TaiKhoan? TaiKhoan { get; set; }

        [Required]
        [StringLength(20)]
        [Column("MaGiaoVienCode")]
        public string MaGV { get; set; } = string.Empty; // 'GV001'

        [Required]
        [StringLength(100)]
        public string HoTen { get; set; } = string.Empty;

        public DateOnly? NgaySinh { get; set; }

        [StringLength(10)]
        public string? GioiTinh { get; set; } // 'Nam', 'Nữ', 'Khác'

        [StringLength(15)]
        public string? SoDienThoai { get; set; }

        [StringLength(100)]
        public string? Email { get; set; }

        [StringLength(255)]
        public string? DiaChi { get; set; }

        [Required]
        [StringLength(100)]
        public string ChuyenMon { get; set; } = string.Empty;

        public bool TrangThai { get; set; } = true;

        // Navigation
        public ICollection<Lop> LopChuNhiem { get; set; } = new List<Lop>();
        public ICollection<PhanCong> PhanCongs { get; set; } = new List<PhanCong>();
    }
}
