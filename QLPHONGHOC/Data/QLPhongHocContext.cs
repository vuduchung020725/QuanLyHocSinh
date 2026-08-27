using Microsoft.EntityFrameworkCore;
using QLPhongHoc.Models;

namespace QLPhongHoc.Data
{
    /// <summary>
    /// DbContext cho Hệ thống Quản lý Học sinh
    /// Tái sử dụng từ QLPHONGHOC, xóa bảng cũ, thêm bảng mới
    /// </summary>
    public class QLPhongHocContext : DbContext
    {
        public QLPhongHocContext(DbContextOptions<QLPhongHocContext> options) : base(options)
        {
        }

        // ===== GIỮ NGUYÊN: Tài khoản & Phân quyền =====
        public DbSet<VaiTro>   VAITRO   { get; set; }
        public DbSet<TaiKhoan> TAIKHOAN { get; set; }

        // ===== MỚI: Hệ thống Quản lý Học sinh =====
        public DbSet<NamHoc>   NAMHOC   { get; set; }
        public DbSet<HocKy>    HOCKY    { get; set; }
        public DbSet<GiaoVien> GIAOVIEN { get; set; }
        public DbSet<Lop>      LOP      { get; set; }
        public DbSet<HocSinh>  HOCSINH  { get; set; }
        public DbSet<MonHoc>   MONHOC   { get; set; }
        public DbSet<PhanCong> PHANCONG { get; set; }
        public DbSet<Diem>     DIEM     { get; set; }
        public DbSet<ThongBao> THONGBAO { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ==========================================
            // VAITRO
            // ==========================================
            modelBuilder.Entity<VaiTro>()
                .ToTable("VAITRO");

            // ==========================================
            // TAIKHOAN
            // ==========================================
            modelBuilder.Entity<TaiKhoan>()
                .ToTable("TAIKHOAN")
                .HasOne(t => t.VaiTro)
                .WithMany()
                .HasForeignKey(t => t.MaVaiTro)
                .OnDelete(DeleteBehavior.Restrict);

            // ==========================================
            // NAMHOC
            // ==========================================
            modelBuilder.Entity<NamHoc>()
                .ToTable("NAMHOC");

            // ==========================================
            // HOCKY → NAMHOC (N:1)
            // ==========================================
            modelBuilder.Entity<HocKy>()
                .ToTable("HOCKY")
                .HasOne(h => h.NamHoc)
                .WithMany(n => n.HocKys)
                .HasForeignKey(h => h.MaNamHoc)
                .OnDelete(DeleteBehavior.Restrict);

            // ==========================================
            // GIAOVIEN → TAIKHOAN (1:1, nullable)
            // ==========================================
            modelBuilder.Entity<GiaoVien>()
                .ToTable("GIAOVIEN")
                .HasOne(g => g.TaiKhoan)
                .WithOne()
                .HasForeignKey<GiaoVien>(g => g.MaTaiKhoan)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);

            // ==========================================
            // LOP → NAMHOC (N:1), LOP → GIAOVIEN (N:1, nullable)
            // ==========================================
            modelBuilder.Entity<Lop>()
                .ToTable("LOP")
                .HasOne(l => l.NamHoc)
                .WithMany(n => n.Lops)
                .HasForeignKey(l => l.MaNamHoc)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Lop>()
                .HasOne(l => l.GiaoVienChuNhiem)
                .WithMany(g => g.LopChuNhiem)
                .HasForeignKey(l => l.MaGVCN)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);

            // ==========================================
            // HOCSINH → LOP (N:1), HOCSINH → TAIKHOAN (1:1, nullable)
            // ==========================================
            modelBuilder.Entity<HocSinh>()
                .ToTable("HOCSINH")
                .HasOne(h => h.Lop)
                .WithMany(l => l.HocSinhs)
                .HasForeignKey(h => h.MaLop)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<HocSinh>()
                .HasOne(h => h.TaiKhoan)
                .WithOne()
                .HasForeignKey<HocSinh>(h => h.MaTaiKhoan)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);

            // ==========================================
            // MONHOC
            // ==========================================
            modelBuilder.Entity<MonHoc>()
                .ToTable("MONHOC");

            // ==========================================
            // PHANCONG: quan hệ N:N với UNIQUE constraint
            // ==========================================
            modelBuilder.Entity<PhanCong>()
                .ToTable("PHANCONG")
                .HasOne(p => p.GiaoVien)
                .WithMany(g => g.PhanCongs)
                .HasForeignKey(p => p.MaGiaoVien)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PhanCong>()
                .HasOne(p => p.MonHoc)
                .WithMany(m => m.PhanCongs)
                .HasForeignKey(p => p.MaMonHoc)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PhanCong>()
                .HasOne(p => p.Lop)
                .WithMany(l => l.PhanCongs)
                .HasForeignKey(p => p.MaLop)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PhanCong>()
                .HasOne(p => p.NamHoc)
                .WithMany()
                .HasForeignKey(p => p.MaNamHoc)
                .OnDelete(DeleteBehavior.Restrict);

            // Unique constraint: GV + Môn + Lớp + Năm học + Học kỳ (term number)
            modelBuilder.Entity<PhanCong>()
                .HasIndex(p => new { p.MaGiaoVien, p.MaMonHoc, p.MaLop, p.MaNamHoc, p.HocKy })
                .IsUnique();

            // ==========================================
            // DIEM: standard columns in QLHocSinh.sql
            // ==========================================
            modelBuilder.Entity<Diem>()
                .ToTable("DIEM")
                .HasOne(d => d.HocSinh)
                .WithMany(h => h.Diems)
                .HasForeignKey(d => d.MaHocSinh)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Diem>()
                .HasOne(d => d.MonHoc)
                .WithMany(m => m.Diems)
                .HasForeignKey(d => d.MaMonHoc)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Diem>()
                .HasOne(d => d.HocKy)
                .WithMany(h => h.Diems)
                .HasForeignKey(d => d.MaHocKy)
                .OnDelete(DeleteBehavior.Restrict);

            // Unique: 1 học sinh, 1 môn, 1 học kỳ
            modelBuilder.Entity<Diem>()
                .HasIndex(d => new { d.MaHocSinh, d.MaMonHoc, d.MaHocKy })
                .IsUnique();

            // ==========================================
            // THONGBAO
            // ==========================================
            modelBuilder.Entity<ThongBao>()
                .ToTable("THONGBAO")
                .HasOne(t => t.TaiKhoan)
                .WithMany()
                .HasForeignKey(t => t.MaTaiKhoan)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
