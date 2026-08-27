using Microsoft.EntityFrameworkCore;
using QLPhongHoc.Data;
using QLPhongHoc.Models;

namespace QLPhongHoc.Services
{
    public interface IMonHocService
    {
        Task<List<MonHocDto>> GetAllAsync();
        Task<MonHocDto?> GetByIdAsync(int id);
        Task<MonHocDto> CreateAsync(CreateMonHocRequest request);
        Task<MonHocDto?> UpdateAsync(int id, UpdateMonHocRequest request);
        Task<bool> DeleteAsync(int id);
    }

    public interface INamHocService
    {
        Task<List<NamHocDto>> GetAllAsync();
        Task<NamHocDto?> GetHienTaiAsync();
        Task<List<HocKyDto>> GetHocKysByNamHocAsync(int maNamHoc);
        Task<HocKyDto?> GetHocKyByIdAsync(int id);
    }

    // ==========================================
    // MonHocService
    // ==========================================
    public class MonHocService : IMonHocService
    {
        private readonly QLPhongHocContext _context;
        private readonly ILogger<MonHocService> _logger;

        public MonHocService(QLPhongHocContext context, ILogger<MonHocService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<MonHocDto>> GetAllAsync()
        {
            return await _context.MONHOC.AsNoTracking()
                .OrderBy(m => m.TenMon)
                .Select(m => new MonHocDto
                {
                    MaMonHoc = m.MaMonHoc,
                    MaMon    = m.MaMon,
                    TenMon   = m.TenMon,
                    SoTiet   = m.SoTiet ?? 0,
                    GhiChu   = m.GhiChu
                })
                .ToListAsync();
        }

        public async Task<MonHocDto?> GetByIdAsync(int id)
        {
            var m = await _context.MONHOC.AsNoTracking().FirstOrDefaultAsync(x => x.MaMonHoc == id);
            if (m == null) return null;
            return new MonHocDto { MaMonHoc = m.MaMonHoc, MaMon = m.MaMon, TenMon = m.TenMon, SoTiet = m.SoTiet ?? 0, GhiChu = m.GhiChu };
        }

        public async Task<MonHocDto> CreateAsync(CreateMonHocRequest request)
        {
            if (await _context.MONHOC.AnyAsync(m => m.MaMon == request.MaMon))
                throw new InvalidOperationException($"Mã môn '{request.MaMon}' đã tồn tại.");

            var entity = new MonHoc { MaMon = request.MaMon, TenMon = request.TenMon, SoTiet = request.SoTiet, GhiChu = request.GhiChu };
            _context.MONHOC.Add(entity);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Thêm môn học: {MaMon}", entity.MaMon);
            return (await GetByIdAsync(entity.MaMonHoc))!;
        }

        public async Task<MonHocDto?> UpdateAsync(int id, UpdateMonHocRequest request)
        {
            var entity = await _context.MONHOC.FindAsync(id);
            if (entity == null) return null;

            if (await _context.MONHOC.AnyAsync(m => m.MaMon == request.MaMon && m.MaMonHoc != id))
                throw new InvalidOperationException($"Mã môn '{request.MaMon}' đã tồn tại.");

            entity.MaMon  = request.MaMon;
            entity.TenMon = request.TenMon;
            entity.SoTiet = request.SoTiet;
            entity.GhiChu = request.GhiChu;

            await _context.SaveChangesAsync();
            return await GetByIdAsync(id);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.MONHOC.FindAsync(id);
            if (entity == null) return false;

            if (await _context.DIEM.AnyAsync(d => d.MaMonHoc == id))
                throw new InvalidOperationException("Không thể xóa môn học đã có dữ liệu điểm.");

            _context.MONHOC.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
    }

    // ==========================================
    // NamHocService
    // ==========================================
    public class NamHocService : INamHocService
    {
        private readonly QLPhongHocContext _context;

        public NamHocService(QLPhongHocContext context) => _context = context;

        public async Task<List<NamHocDto>> GetAllAsync()
        {
            return await _context.NAMHOC.AsNoTracking()
                .OrderByDescending(n => n.TenNamHoc)
                .Select(n => new NamHocDto { MaNamHoc = n.MaNamHoc, TenNamHoc = n.TenNamHoc, TrangThai = n.TrangThai })
                .ToListAsync();
        }

        public async Task<NamHocDto?> GetHienTaiAsync()
        {
            var n = await _context.NAMHOC.AsNoTracking()
                .OrderByDescending(x => x.MaNamHoc)
                .FirstOrDefaultAsync(x => x.TrangThai);
            if (n == null) return null;
            return new NamHocDto { MaNamHoc = n.MaNamHoc, TenNamHoc = n.TenNamHoc, TrangThai = n.TrangThai };
        }

        public async Task<List<HocKyDto>> GetHocKysByNamHocAsync(int maNamHoc)
        {
            return await _context.HOCKY.AsNoTracking()
                .Include(h => h.NamHoc)
                .Where(h => h.MaNamHoc == maNamHoc)
                .OrderBy(h => h.SoHocKy)
                .Select(h => new HocKyDto
                {
                    MaHocKy  = h.MaHocKy,
                    TenHocKy = h.TenHocKy,
                    MaNamHoc = h.MaNamHoc,
                    TenNamHoc = h.NamHoc != null ? h.NamHoc.TenNamHoc : string.Empty,
                    TuNgay   = h.TuNgay,
                    DenNgay  = h.DenNgay
                })
                .ToListAsync();
        }

        public async Task<HocKyDto?> GetHocKyByIdAsync(int id)
        {
            var h = await _context.HOCKY.AsNoTracking()
                .Include(x => x.NamHoc)
                .FirstOrDefaultAsync(x => x.MaHocKy == id);
            if (h == null) return null;

            return new HocKyDto
            {
                MaHocKy  = h.MaHocKy,
                TenHocKy = h.TenHocKy,
                MaNamHoc = h.MaNamHoc,
                TenNamHoc = h.NamHoc?.TenNamHoc ?? string.Empty,
                TuNgay   = h.TuNgay,
                DenNgay  = h.DenNgay
            };
        }
    }
}
