using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLPhongHoc.Models
{
    [Table("LOP")]
    public class Lop
    {
        [Key]
        public int MaLop { get; set; }

        [Required]
        [StringLength(20)]
        public string MaLopCode { get; set; } = string.Empty; // '10A1', '11B2'

        [Required]
        [StringLength(50)]
        public string TenLop { get; set; } = string.Empty;

        [Required]
        public int Khoi { get; set; }

        public int MaNamHoc { get; set; }
        [ForeignKey("MaNamHoc")]
        public NamHoc NamHoc { get; set; } = null!;

        [Column("MaGiaoVienChuNhiem")]
        public int? MaGVCN { get; set; }
        [ForeignKey("MaGVCN")]
        public GiaoVien? GiaoVienChuNhiem { get; set; }

        public int SiSo { get; set; } = 0;

        public bool TrangThai { get; set; } = true;

        // Navigation
        public ICollection<HocSinh> HocSinhs { get; set; } = new List<HocSinh>();
        public ICollection<PhanCong> PhanCongs { get; set; } = new List<PhanCong>();
    }
}
