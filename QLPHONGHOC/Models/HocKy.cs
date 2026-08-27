using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLPhongHoc.Models
{
    [Table("HOCKY")]
    public class HocKy
    {
        [Key]
        public int MaHocKy { get; set; }

        [Required]
        [StringLength(30)]
        public string TenHocKy { get; set; } = string.Empty; // 'Học kỳ 1', 'Học kỳ 2'

        public int MaNamHoc { get; set; }
        [ForeignKey("MaNamHoc")]
        public NamHoc NamHoc { get; set; } = null!;

        public int SoHocKy { get; set; }

        [NotMapped]
        public DateOnly TuNgay { get; set; }

        [NotMapped]
        public DateOnly DenNgay { get; set; }

        public bool TrangThai { get; set; } = true;

        // Navigation
        public ICollection<PhanCong> PhanCongs { get; set; } = new List<PhanCong>();
        public ICollection<Diem> Diems { get; set; } = new List<Diem>();
    }
}
