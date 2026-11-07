using Microsoft.EntityFrameworkCore;
using QLPhongHoc.Data;
using QLPhongHoc.Models;

namespace QLPhongHoc.Services
{
    public interface IHocSinhService
    {
        Task<PagedResult<HocSinhDto>> GetHocSinhsAsync(string? keyword, int? maLop, bool? trangThai, int pageNumber, int pageSize);
        Task<HocSinhDto?> GetHocSinhByIdAsync(int id);
        Task<HocSinhDto?> GetHocSinhByMaTaiKhoanAsync(int maTaiKhoan);
        Task<HocSinhDto> CreateHocSinhAsync(CreateHocSinhRequest request);
        Task<HocSinhDto?> UpdateHocSinhAsync(int id, UpdateHocSinhRequest request);
        Task<bool> DeleteHocSinhAsync(int id);
        Task<bool> MaHSExistsAsync(string maHS, int? excludeId = null);
    }

    public class HocSinhService : IHocSinhService
    {
        private readonly QLPhongHocContext _context;
        private readonly ILogger<HocSinhService> _logger;

        public HocSinhService(QLPhongHocContext context, ILogger<HocSinhService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<PagedResult<HocSinhDto>> GetHocSinhsAsync(
            string? keyword, int? maLop, bool? trangThai, int pageNumber, int pageSize)
        {
            var query = _context.HOCSINH.AsNoTracking()
                .Include(h => h.Lop)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var kw = keyword.Trim().ToLower();
                query = query.Where(h =>
                    h.HoTen.ToLower().Contains(kw) ||
                    h.MaHS.ToLower().Contains(kw) ||
                    (h.Email != null && h.Email.ToLower().Contains(kw)) ||
                    (h.SoDienThoai != null && h.SoDienThoai.Contains(kw)));
            }

            if (maLop.HasValue)
                query = query.Where(h => h.MaLop == maLop.Value);

            if (trangThai.HasValue)
                query = query.Where(h => h.TrangThai == trangThai.Value);

            var totalItems = await query.CountAsync();
            var items = await query
                .OrderBy(h => h.Lop.MaLopCode)
                .ThenBy(h => h.HoTen)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(h => new HocSinhDto
                {
                    MaHocSinh   = h.MaHocSinh,
                    MaTaiKhoan  = h.MaTaiKhoan,
                    MaHS        = h.MaHS,
                    HoTen       = h.HoTen,
                    NgaySinh    = h.NgaySinh,
                    GioiTinh    = h.GioiTinh,
                    DiaChi      = h.DiaChi,
                    SoDienThoai = h.SoDienThoai,
                    Email       = h.Email,
                    TenPhuHuynh = h.TenPhuHuynh,
                    SdtPhuHuynh = h.SdtPhuHuynh,
                    MaLop       = h.MaLop,
                    TenLop      = h.Lop != null ? h.Lop.TenLop : string.Empty,
                    TrangThai   = h.TrangThai
                })
                .ToListAsync();

            return new PagedResult<HocSinhDto>
            {
                Items = items, TotalItems = totalItems,
                PageNumber = pageNumber, PageSize = pageSize
            };
        }

        public async Task<HocSinhDto?> GetHocSinhByIdAsync(int id)
        {
            var h = await _context.HOCSINH.AsNoTracking()
                .Include(x => x.Lop)
                .FirstOrDefaultAsync(x => x.MaHocSinh == id);
            if (h == null) return null;

            return MapToDto(h);
        }

        public async Task<HocSinhDto?> GetHocSinhByMaTaiKhoanAsync(int maTaiKhoan)
        {
            var h = await _context.HOCSINH.AsNoTracking()
                .Include(x => x.Lop)
                .FirstOrDefaultAsync(x => x.MaTaiKhoan == maTaiKhoan);
            if (h == null) return null;
            return MapToDto(h);
        }

        public async Task<HocSinhDto> CreateHocSinhAsync(CreateHocSinhRequest request)
        {
            if (await MaHSExistsAsync(request.MaHS))
                throw new InvalidOperationException($"Mã học sinh '{request.MaHS}' đã tồn tại.");

            if (request.NgaySinh >= DateOnly.FromDateTime(DateTime.Today))
                throw new InvalidOperationException("Ngày sinh phải nhỏ hơn ngày hiện tại.");

            var entity = new HocSinh
            {
                MaTaiKhoan  = request.MaTaiKhoan,
                MaHS        = request.MaHS,
                HoTen       = request.HoTen,
                NgaySinh    = request.NgaySinh,
                GioiTinh    = request.GioiTinh,
                DiaChi      = request.DiaChi,
                SoDienThoai = request.SoDienThoai,
                Email       = request.Email,
                TenPhuHuynh = request.TenPhuHuynh,
                SdtPhuHuynh = request.SdtPhuHuynh,
                MaLop       = request.MaLop,
                NgayNhapHoc = DateOnly.FromDateTime(DateTime.Today),
                TrangThai   = request.TrangThai
            };

            _context.HOCSINH.Add(entity);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Thêm học sinh: {MaHS} - {HoTen}", entity.MaHS, entity.HoTen);

            return (await GetHocSinhByIdAsync(entity.MaHocSinh))!;
        }

        public async Task<HocSinhDto?> UpdateHocSinhAsync(int id, UpdateHocSinhRequest request)
        {
            var entity = await _context.HOCSINH.FindAsync(id);
            if (entity == null) return null;

            if (await MaHSExistsAsync(request.MaHS, id))
                throw new InvalidOperationException($"Mã học sinh '{request.MaHS}' đã tồn tại.");

            if (request.NgaySinh >= DateOnly.FromDateTime(DateTime.Today))
                throw new InvalidOperationException("Ngày sinh phải nhỏ hơn ngày hiện tại.");

            entity.MaTaiKhoan  = request.MaTaiKhoan;
            entity.MaHS        = request.MaHS;
            entity.HoTen       = request.HoTen;
            entity.NgaySinh    = request.NgaySinh;
            entity.GioiTinh    = request.GioiTinh;
            entity.DiaChi      = request.DiaChi;
            entity.SoDienThoai = request.SoDienThoai;
            entity.Email       = request.Email;
            entity.TenPhuHuynh = request.TenPhuHuynh;
            entity.SdtPhuHuynh = request.SdtPhuHuynh;
            entity.MaLop       = request.MaLop;
            entity.TrangThai   = request.TrangThai;

            await _context.SaveChangesAsync();
            _logger.LogInformation("Cập nhật học sinh: {id}", id);
            return await GetHocSinhByIdAsync(id);
        }

        public async Task<bool> DeleteHocSinhAsync(int id)
        {
            var entity = await _context.HOCSINH.FindAsync(id);
            if (entity == null) return false;

            var hasDiem = await _context.DIEM.AnyAsync(d => d.MaHocSinh == id);
            if (hasDiem)
            {
                // Soft delete: chuyển trạng thái thành false
                entity.TrangThai = false;
                _logger.LogInformation("Soft-delete học sinh {id} (có dữ liệu điểm)", id);
            }
            else
            {
                _context.HOCSINH.Remove(entity);
                _logger.LogInformation("Hard-delete học sinh {id}", id);
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> MaHSExistsAsync(string maHS, int? excludeId = null)
        {
            return await _context.HOCSINH.AnyAsync(h =>
                h.MaHS == maHS && (excludeId == null || h.MaHocSinh != excludeId));
        }

        private static HocSinhDto MapToDto(HocSinh h) => new()
        {
            MaHocSinh   = h.MaHocSinh,
            MaTaiKhoan  = h.MaTaiKhoan,
            MaHS        = h.MaHS,
            HoTen       = h.HoTen,
            NgaySinh    = h.NgaySinh,
            GioiTinh    = h.GioiTinh,
            DiaChi      = h.DiaChi,
            SoDienThoai = h.SoDienThoai,
            Email       = h.Email,
            TenPhuHuynh = h.TenPhuHuynh,
            SdtPhuHuynh = h.SdtPhuHuynh,
            MaLop       = h.MaLop,
            TenLop      = h.Lop?.TenLop ?? string.Empty,
            TrangThai   = h.TrangThai
        };
    }
}
