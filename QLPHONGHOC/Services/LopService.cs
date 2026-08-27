using Microsoft.EntityFrameworkCore;
using QLPhongHoc.Data;
using QLPhongHoc.Models;

namespace QLPhongHoc.Services
{
    public interface ILopService
    {
        Task<List<LopDto>> GetLopsAsync(int? maNamHoc, int? khoi);
        Task<LopDto?> GetLopByIdAsync(int id);
        Task<PagedResult<HocSinhDto>> GetHocSinhTrongLopAsync(int maLop, int pageNumber, int pageSize);
        Task<LopDto> CreateLopAsync(CreateLopRequest request);
        Task<LopDto?> UpdateLopAsync(int id, UpdateLopRequest request);
        Task<bool> DeleteLopAsync(int id);
        Task<List<LopDto>> GetAllActiveAsync();
    }

    public class LopService : ILopService
    {
        private readonly QLPhongHocContext _context;
        private readonly ILogger<LopService> _logger;

        public LopService(QLPhongHocContext context, ILogger<LopService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<LopDto>> GetLopsAsync(int? maNamHoc, int? khoi)
        {
            var query = _context.LOP.AsNoTracking()
                .Include(l => l.NamHoc)
                .Include(l => l.GiaoVienChuNhiem)
                .AsQueryable();

            if (maNamHoc.HasValue)
                query = query.Where(l => l.MaNamHoc == maNamHoc.Value);

            if (khoi.HasValue)
                query = query.Where(l => l.Khoi == khoi.Value);

            return await query
                .OrderBy(l => l.Khoi)
                .ThenBy(l => l.MaLopCode)
                .Select(l => new LopDto
                {
                    MaLop     = l.MaLop,
                    MaLopCode = l.MaLopCode,
                    TenLop    = l.TenLop,
                    Khoi      = l.Khoi,
                    MaNamHoc  = l.MaNamHoc,
                    TenNamHoc = l.NamHoc != null ? l.NamHoc.TenNamHoc : string.Empty,
                    MaGVCN    = l.MaGVCN,
                    TenGVCN   = l.GiaoVienChuNhiem != null ? l.GiaoVienChuNhiem.HoTen : null,
                    SiSo      = l.SiSo,
                    TrangThai = l.TrangThai
                })
                .ToListAsync();
        }

        public async Task<LopDto?> GetLopByIdAsync(int id)
        {
            var l = await _context.LOP.AsNoTracking()
                .Include(x => x.NamHoc)
                .Include(x => x.GiaoVienChuNhiem)
                .FirstOrDefaultAsync(x => x.MaLop == id);
            if (l == null) return null;

            return new LopDto
            {
                MaLop     = l.MaLop,
                MaLopCode = l.MaLopCode,
                TenLop    = l.TenLop,
                Khoi      = l.Khoi,
                MaNamHoc  = l.MaNamHoc,
                TenNamHoc = l.NamHoc?.TenNamHoc ?? string.Empty,
                MaGVCN    = l.MaGVCN,
                TenGVCN   = l.GiaoVienChuNhiem?.HoTen,
                SiSo      = l.SiSo,
                TrangThai = l.TrangThai
            };
        }

        public async Task<PagedResult<HocSinhDto>> GetHocSinhTrongLopAsync(int maLop, int pageNumber, int pageSize)
        {
            var query = _context.HOCSINH.AsNoTracking()
                .Include(h => h.Lop)
                .Where(h => h.MaLop == maLop);

            var total = await query.CountAsync();
            var items = await query
                .OrderBy(h => h.HoTen)
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
                    MaLop       = h.MaLop,
                    TenLop      = h.Lop != null ? h.Lop.TenLop : string.Empty,
                    TrangThai   = h.TrangThai
                })
                .ToListAsync();

            return new PagedResult<HocSinhDto>
            {
                Items = items, TotalItems = total,
                PageNumber = pageNumber, PageSize = pageSize
            };
        }

        public async Task<LopDto> CreateLopAsync(CreateLopRequest request)
        {
            if (await _context.LOP.AnyAsync(l => l.MaLopCode == request.MaLopCode))
                throw new InvalidOperationException($"Mã lớp '{request.MaLopCode}' đã tồn tại.");

            var entity = new Lop
            {
                MaLopCode = request.MaLopCode,
                TenLop    = request.TenLop,
                Khoi      = request.Khoi,
                MaNamHoc  = request.MaNamHoc,
                MaGVCN    = request.MaGVCN,
                TrangThai = request.TrangThai
            };

            _context.LOP.Add(entity);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Thêm lớp: {MaLopCode}", entity.MaLopCode);
            return (await GetLopByIdAsync(entity.MaLop))!;
        }

        public async Task<LopDto?> UpdateLopAsync(int id, UpdateLopRequest request)
        {
            var entity = await _context.LOP.FindAsync(id);
            if (entity == null) return null;

            if (await _context.LOP.AnyAsync(l => l.MaLopCode == request.MaLopCode && l.MaLop != id))
                throw new InvalidOperationException($"Mã lớp '{request.MaLopCode}' đã tồn tại.");

            entity.MaLopCode = request.MaLopCode;
            entity.TenLop    = request.TenLop;
            entity.Khoi      = request.Khoi;
            entity.MaNamHoc  = request.MaNamHoc;
            entity.MaGVCN    = request.MaGVCN;
            entity.TrangThai = request.TrangThai;
            // Cập nhật sĩ số tự động
            entity.SiSo = await _context.HOCSINH.CountAsync(h => h.MaLop == id && h.TrangThai);

            await _context.SaveChangesAsync();
            return await GetLopByIdAsync(id);
        }

        public async Task<bool> DeleteLopAsync(int id)
        {
            var entity = await _context.LOP.FindAsync(id);
            if (entity == null) return false;

            var hasStudents = await _context.HOCSINH.AnyAsync(h => h.MaLop == id);
            if (hasStudents)
                throw new InvalidOperationException("Không thể xóa lớp còn học sinh. Hãy chuyển lớp học sinh trước.");

            _context.LOP.Remove(entity);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Xóa lớp: {id}", id);
            return true;
        }

        public async Task<List<LopDto>> GetAllActiveAsync()
        {
            return await GetLopsAsync(null, null);
        }
    }
}
