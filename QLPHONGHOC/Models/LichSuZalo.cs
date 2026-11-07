using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLPhongHoc.Models
{
    [Table("LICHSUZALO")]
    public class LichSuZalo
    {
        [Key]
        public int MaLichSuZalo { get; set; }

        public int MaHocSinh { get; set; }
        [ForeignKey("MaHocSinh")]
        public HocSinh HocSinh { get; set; } = null!;

        [Required]
        [StringLength(15)]
        public string SoDienThoaiNhan { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string LoaiTinNhan { get; set; } = string.Empty; // "Bảng điểm", "Điểm danh vắng mặt", "Nhận xét"

        [Required]
        public string NoiDungGui { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string TrangThai { get; set; } = "Thành công"; // "Thành công", "Thất bại", "Đang gửi"

        [StringLength(100)]
        public string? MessageIdZalo { get; set; }

        public DateTime ThoiGianGui { get; set; } = DateTime.Now;
    }
}

