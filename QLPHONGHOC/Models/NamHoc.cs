using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLPhongHoc.Models
{
    [Table("NAMHOC")]
    public class NamHoc
    {
        [Key]
        public int MaNamHoc { get; set; }

        [Required]
        [StringLength(20)]
        public string TenNamHoc { get; set; } = string.Empty; // VD: '2024-2025'

        public DateOnly? NgayBatDau { get; set; }

        public DateOnly? NgayKetThuc { get; set; }

        public bool TrangThai { get; set; } = true;

        // Navigation
        public ICollection<HocKy> HocKys { get; set; } = new List<HocKy>();
        public ICollection<Lop> Lops { get; set; } = new List<Lop>();
    }
}
