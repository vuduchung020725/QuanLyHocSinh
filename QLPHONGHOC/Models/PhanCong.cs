using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLPhongHoc.Models
{
    [Table("PHANCONG")]
    public class PhanCong
    {
        [Key]
        public int MaPhanCong { get; set; }

        public int MaGiaoVien { get; set; }
        [ForeignKey("MaGiaoVien")]
        public GiaoVien GiaoVien { get; set; } = null!;

        public int MaMonHoc { get; set; }
        [ForeignKey("MaMonHoc")]
        public MonHoc MonHoc { get; set; } = null!;

        public int MaLop { get; set; }
        [ForeignKey("MaLop")]
        public Lop Lop { get; set; } = null!;

        public int MaNamHoc { get; set; }
        [ForeignKey("MaNamHoc")]
        public NamHoc NamHoc { get; set; } = null!;

        public int HocKy { get; set; } // 1, 2, 3

        public DateOnly NgayPhanCong { get; set; } = DateOnly.FromDateTime(DateTime.Now);

        public bool TrangThai { get; set; } = true;
    }
}
