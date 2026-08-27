using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLPhongHoc.Models
{
    [Table("VAITRO")]
    public class VaiTro
    {
        [Key]
        public int MaVaiTro { get; set; }

        [StringLength(50)]
        public string TenVaiTro { get; set; } = string.Empty;

        [StringLength(255)]
        public string? MoTa { get; set; }
    }
}
