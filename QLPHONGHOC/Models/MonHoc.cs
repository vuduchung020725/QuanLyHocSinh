using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLPhongHoc.Models
{
    [Table("MONHOC")]
    public class MonHoc
    {
        [Key]
        public int MaMonHoc { get; set; }

        [Required]
        [StringLength(20)]
        [Column("MaMonHocCode")]
        public string MaMon { get; set; } = string.Empty; // 'TOAN', 'VAN', 'ANH'

        [Required]
        [StringLength(100)]
        [Column("TenMonHoc")]
        public string TenMon { get; set; } = string.Empty;

        public int? SoTiet { get; set; } = 0;

        [StringLength(255)]
        [Column("MoTa")]
        public string? GhiChu { get; set; }

        public bool TrangThai { get; set; } = true;

        // Navigation
        public ICollection<PhanCong> PhanCongs { get; set; } = new List<PhanCong>();
        public ICollection<Diem> Diems { get; set; } = new List<Diem>();
    }
}
