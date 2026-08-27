using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QLPhongHoc.Data;
using QLPhongHoc.Models;

namespace QLPhongHoc.Services
{
    public interface IPhanCongService
    {
        Task<List<PhanCongDto>> GetPhanCongsAsync(int? maGiaoVien, int? maLop, int? maMonHoc, int? maNamHoc, int? hocKy);
        Task<PhanCongDto?> GetPhanCongByIdAsync(int id);
        Task<PhanCongDto> CreatePhanCongAsync(CreatePhanCongRequest request);
        Task<bool> DeletePhanCongAsync(int id);
    }

    public class PhanCongService : IPhanCongService
    {
        private readonly QLPhongHocContext _context;
        private readonly ILogger<PhanCongService> _logger;

        public PhanCongService(QLPhongHocContext context, ILogger<PhanCongService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<PhanCongDto>> GetPhanCongsAsync(int? maGiaoVien, int? maLop, int? maMonHoc, int? maNamHoc, int? hocKy)
        {
            var query = _context.PHANCONG.AsNoTracking()
                .Include(p => p.GiaoVien)
                .Include(p => p.Lop)
                .Include(p => p.MonHoc)
                .Include(p => p.NamHoc)
                .AsQueryable();

            if (maGiaoVien.HasValue) query = query.Where(p => p.MaGiaoVien == maGiaoVien.Value);
            if (maLop.HasValue)      query = query.Where(p => p.MaLop == maLop.Value);
            if (maMonHoc.HasValue)   query = query.Where(p => p.MaMonHoc == maMonHoc.Value);
            if (maNamHoc.HasValue)   query = query.Where(p => p.MaNamHoc == maNamHoc.Value);
            if (hocKy.HasValue)      query = query.Where(p => p.HocKy == hocKy.Value);

            return await query
                .OrderBy(p => p.NamHoc.TenNamHoc)
                .ThenBy(p => p.HocKy)
                .ThenBy(p => p.Lop.TenLop)
                .Select(p => new PhanCongDto
                {
                    MaPhanCong = p.MaPhanCong,
                    MaGiaoVien = p.MaGiaoVien,
                    TenGiaoVien = p.GiaoVien.HoTen,
                    MaMonHoc = p.MaMonHoc,
                    TenMon = p.MonHoc.TenMon,
                    MaLop = p.MaLop,
                    TenLop = p.Lop.TenLop,
                    MaNamHoc = p.MaNamHoc,
                    TenNamHoc = p.NamHoc.TenNamHoc,
                    HocKy = p.HocKy
                })
                .ToListAsync();
        }

        public async Task<PhanCongDto?> GetPhanCongByIdAsync(int id)
        {
            var p = await _context.PHANCONG.AsNoTracking()
                .Include(p => p.GiaoVien)
                .Include(p => p.Lop)
                .Include(p => p.MonHoc)
                .Include(p => p.NamHoc)
                .FirstOrDefaultAsync(x => x.MaPhanCong == id);

            if (p == null) return null;

            return new PhanCongDto
            {
                MaPhanCong = p.MaPhanCong,
                MaGiaoVien = p.MaGiaoVien,
                TenGiaoVien = p.GiaoVien.HoTen,
                MaMonHoc = p.MaMonHoc,
                TenMon = p.MonHoc.TenMon,
                MaLop = p.MaLop,
                TenLop = p.Lop.TenLop,
                MaNamHoc = p.MaNamHoc,
                TenNamHoc = p.NamHoc.TenNamHoc,
                HocKy = p.HocKy
            };
        }

        public async Task<PhanCongDto> CreatePhanCongAsync(CreatePhanCongRequest request)
        {
            if (await _context.PHANCONG.AnyAsync(p =>
                p.MaGiaoVien == request.MaGiaoVien &&
                p.MaLop == request.MaLop &&
                p.MaMonHoc == request.MaMonHoc &&
                p.MaNamHoc == request.MaNamHoc &&
                p.HocKy == request.HocKy))
            {
                throw new InvalidOperationException("Phân công này đã tồn tại.");
            }

            var entity = new PhanCong
            {
                MaGiaoVien = request.MaGiaoVien,
                MaLop = request.MaLop,
                MaMonHoc = request.MaMonHoc,
                MaNamHoc = request.MaNamHoc,
                HocKy = request.HocKy,
                NgayPhanCong = DateOnly.FromDateTime(DateTime.Now),
                TrangThai = true
            };

            _context.PHANCONG.Add(entity);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Tạo phân công giảng dạy ID={id}", entity.MaPhanCong);

            return (await GetPhanCongByIdAsync(entity.MaPhanCong))!;
        }

        public async Task<bool> DeletePhanCongAsync(int id)
        {
            var entity = await _context.PHANCONG.FindAsync(id);
            if (entity == null) return false;

            _context.PHANCONG.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
