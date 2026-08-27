using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLPhongHoc.Models
{
    [Table("DIEM")]
    public class Diem
    {
        [Key]
        public int MaDiem { get; set; }

        public int MaHocSinh { get; set; }
        [ForeignKey("MaHocSinh")]
        public HocSinh HocSinh { get; set; } = null!;

        public int MaMonHoc { get; set; }
        [ForeignKey("MaMonHoc")]
        public MonHoc MonHoc { get; set; } = null!;

        public int MaHocKy { get; set; }
        [ForeignKey("MaHocKy")]
        public HocKy HocKy { get; set; } = null!;

        [Column(TypeName = "decimal(4,2)")]
        public decimal? DiemMieng { get; set; }

        [Column(TypeName = "decimal(4,2)")]
        public decimal? Diem15Phut { get; set; }

        [Column(TypeName = "decimal(4,2)")]
        public decimal? DiemGiuaKy { get; set; }

        [Column(TypeName = "decimal(4,2)")]
        public decimal? DiemCuoiKy { get; set; }

        [Column(TypeName = "decimal(4,2)")]
        public decimal? DiemTrungBinh { get; set; }

        [NotMapped]
        public string? XepLoai { get; set; }

        [StringLength(255)]
        public string? GhiChu { get; set; }

        public DateTime NgayCapNhat { get; set; } = DateTime.Now;
    }
}
