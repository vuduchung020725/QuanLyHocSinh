using Microsoft.EntityFrameworkCore;
using QLPhongHoc.Data;
using QLPhongHoc.Models;

namespace QLPhongHoc.Services
{
    public interface IGiaoVienService
    {
        Task<PagedResult<GiaoVienDto>> GetGiaoViensAsync(string? keyword, bool? trangThai, int pageNumber, int pageSize);
        Task<GiaoVienDto?> GetGiaoVienByIdAsync(int id);
        Task<GiaoVienDto?> GetGiaoVienByMaTaiKhoanAsync(int maTaiKhoan);
        Task<GiaoVienDto> CreateGiaoVienAsync(CreateGiaoVienRequest request);
        Task<GiaoVienDto?> UpdateGiaoVienAsync(int id, UpdateGiaoVienRequest request);
        Task<bool> DeleteGiaoVienAsync(int id);
        Task<List<GiaoVienDto>> GetAllActiveAsync();
    }

    public class GiaoVienService : IGiaoVienService
    {
        private readonly QLPhongHocContext _context;
        private readonly ILogger<GiaoVienService> _logger;

        public GiaoVienService(QLPhongHocContext context, ILogger<GiaoVienService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<PagedResult<GiaoVienDto>> GetGiaoViensAsync(
            string? keyword, bool? trangThai, int pageNumber, int pageSize)
        {
            var query = _context.GIAOVIEN.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = keyword.Trim().ToLower();
                query = query.Where(g =>
                    g.HoTen.ToLower().Contains(kw) ||
                    g.MaGV.ToLower().Contains(kw) ||
                    g.ChuyenMon.ToLower().Contains(kw) ||
                    (g.Email != null && g.Email.ToLower().Contains(kw)));
            }

            if (trangThai.HasValue)
                query = query.Where(g => g.TrangThai == trangThai.Value);

            var totalItems = await query.CountAsync();
            var items = await query
                .OrderBy(g => g.HoTen)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(g => new GiaoVienDto
                {
                    MaGiaoVien  = g.MaGiaoVien,
                    MaTaiKhoan  = g.MaTaiKhoan,
                    MaGV        = g.MaGV,
                    HoTen       = g.HoTen,
                    NgaySinh    = g.NgaySinh,
                    GioiTinh    = g.GioiTinh,
                    SoDienThoai = g.SoDienThoai,
                    Email       = g.Email,
                    DiaChi      = g.DiaChi,
                    ChuyenMon   = g.ChuyenMon,
                    TrangThai   = g.TrangThai
                })
                .ToListAsync();

            return new PagedResult<GiaoVienDto>
            {
                Items = items, TotalItems = totalItems,
                PageNumber = pageNumber, PageSize = pageSize
            };
        }

        public async Task<GiaoVienDto?> GetGiaoVienByIdAsync(int id)
        {
            var g = await _context.GIAOVIEN.AsNoTracking().FirstOrDefaultAsync(x => x.MaGiaoVien == id);
            return g == null ? null : MapToDto(g);
        }

        public async Task<GiaoVienDto?> GetGiaoVienByMaTaiKhoanAsync(int maTaiKhoan)
        {
            var g = await _context.GIAOVIEN.AsNoTracking().FirstOrDefaultAsync(x => x.MaTaiKhoan == maTaiKhoan);
            return g == null ? null : MapToDto(g);
        }

        public async Task<GiaoVienDto> CreateGiaoVienAsync(CreateGiaoVienRequest request)
        {
            if (await _context.GIAOVIEN.AnyAsync(g => g.MaGV == request.MaGV))
                throw new InvalidOperationException($"Mã giáo viên '{request.MaGV}' đã tồn tại.");

            var entity = new GiaoVien
            {
                MaTaiKhoan  = request.MaTaiKhoan,
                MaGV        = request.MaGV,
                HoTen       = request.HoTen,
                NgaySinh    = request.NgaySinh,
                GioiTinh    = request.GioiTinh,
                SoDienThoai = request.SoDienThoai,
                Email       = request.Email,
                DiaChi      = request.DiaChi,
                ChuyenMon   = request.ChuyenMon,
                TrangThai   = request.TrangThai
            };

            _context.GIAOVIEN.Add(entity);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Thêm giáo viên: {MaGV} - {HoTen}", entity.MaGV, entity.HoTen);
            return (await GetGiaoVienByIdAsync(entity.MaGiaoVien))!;
        }

        public async Task<GiaoVienDto?> UpdateGiaoVienAsync(int id, UpdateGiaoVienRequest request)
        {
            var entity = await _context.GIAOVIEN.FindAsync(id);
            if (entity == null) return null;

            if (await _context.GIAOVIEN.AnyAsync(g => g.MaGV == request.MaGV && g.MaGiaoVien != id))
                throw new InvalidOperationException($"Mã giáo viên '{request.MaGV}' đã tồn tại.");

            entity.MaTaiKhoan  = request.MaTaiKhoan;
            entity.MaGV        = request.MaGV;
            entity.HoTen       = request.HoTen;
            entity.NgaySinh    = request.NgaySinh;
            entity.GioiTinh    = request.GioiTinh;
            entity.SoDienThoai = request.SoDienThoai;
            entity.Email       = request.Email;
            entity.DiaChi      = request.DiaChi;
            entity.ChuyenMon   = request.ChuyenMon;
            entity.TrangThai   = request.TrangThai;

            await _context.SaveChangesAsync();
            _logger.LogInformation("Cập nhật giáo viên: {id}", id);
            return await GetGiaoVienByIdAsync(id);
        }

        public async Task<bool> DeleteGiaoVienAsync(int id)
        {
            var entity = await _context.GIAOVIEN.FindAsync(id);
            if (entity == null) return false;

            var hasRelated = await _context.PHANCONG.AnyAsync(p => p.MaGiaoVien == id)
                          || await _context.LOP.AnyAsync(l => l.MaGVCN == id);

            if (hasRelated)
            {
                entity.TrangThai = false;
                _logger.LogInformation("Soft-delete GV {id}", id);
            }
            else
            {
                _context.GIAOVIEN.Remove(entity);
                _logger.LogInformation("Hard-delete GV {id}", id);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<List<GiaoVienDto>> GetAllActiveAsync()
        {
            return await _context.GIAOVIEN.AsNoTracking()
                .Where(g => g.TrangThai)
                .OrderBy(g => g.HoTen)
                .Select(g => MapToDto(g))
                .ToListAsync();
        }

        private static GiaoVienDto MapToDto(GiaoVien g) => new()
        {
            MaGiaoVien  = g.MaGiaoVien,
            MaTaiKhoan  = g.MaTaiKhoan,
            MaGV        = g.MaGV,
            HoTen       = g.HoTen,
            NgaySinh    = g.NgaySinh,
            GioiTinh    = g.GioiTinh,
            SoDienThoai = g.SoDienThoai,
            Email       = g.Email,
            DiaChi      = g.DiaChi,
            ChuyenMon   = g.ChuyenMon,
            TrangThai   = g.TrangThai
        };
    }
}
