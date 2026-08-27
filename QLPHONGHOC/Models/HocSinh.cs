using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLPhongHoc.Models
{
    [Table("HOCSINH")]
    public class HocSinh
    {
        [Key]
        public int MaHocSinh { get; set; }

        public int? MaTaiKhoan { get; set; }
        [ForeignKey("MaTaiKhoan")]
        public TaiKhoan? TaiKhoan { get; set; }

        [Required]
        [StringLength(20)]
        [Column("MaHocSinhCode")]
        public string MaHS { get; set; } = string.Empty; // 'HS001'

        [Required]
        [StringLength(100)]
        public string HoTen { get; set; } = string.Empty;

        [Required]
        public DateOnly NgaySinh { get; set; }

        [Required]
        [StringLength(10)]
        public string GioiTinh { get; set; } = string.Empty; // 'Nam', 'Nữ', 'Khác'

        [StringLength(255)]
        public string? DiaChi { get; set; }

        [StringLength(15)]
        public string? SoDienThoai { get; set; }

        [StringLength(100)]
        public string? Email { get; set; }

        public int MaLop { get; set; }
        [ForeignKey("MaLop")]
        public Lop Lop { get; set; } = null!;

        public DateOnly? NgayNhapHoc { get; set; }

        public bool TrangThai { get; set; } = true;

        // Navigation
        public ICollection<Diem> Diems { get; set; } = new List<Diem>();
    }
}
