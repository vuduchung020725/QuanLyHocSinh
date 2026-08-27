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
    public interface IDiemService
    {
        Task<PagedResult<DiemDto>> GetDiemsAsync(int? maHocSinh, int? maMonHoc, int? maHocKy, int? maLop, int pageNumber, int pageSize);
        Task<DiemDto?> GetDiemByIdAsync(int id);
        Task<DiemDto?> GetDiemByKeyAsync(int maHocSinh, int maMonHoc, int maHocKy);
        Task<DiemDto> UpsertDiemAsync(NhapDiemRequest request, int maGiaoVienNhap);
        Task<List<DiemDto>> GetDiemOfHocSinhAsync(int maHocSinh, int? maHocKy);
        Task<bool> DeleteDiemAsync(int id);
    }

    public class DiemService : IDiemService
    {
        private readonly QLPhongHocContext _context;
        private readonly ILogger<DiemService> _logger;

        public DiemService(QLPhongHocContext context, ILogger<DiemService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<PagedResult<DiemDto>> GetDiemsAsync(
            int? maHocSinh, int? maMonHoc, int? maHocKy, int? maLop, int pageNumber, int pageSize)
        {
            var query = _context.DIEM.AsNoTracking()
                .Include(d => d.HocSinh).ThenInclude(h => h.Lop)
                .Include(d => d.MonHoc)
                .Include(d => d.HocKy)
                .AsQueryable();

            if (maHocSinh.HasValue) query = query.Where(d => d.MaHocSinh == maHocSinh.Value);
            if (maMonHoc.HasValue)  query = query.Where(d => d.MaMonHoc  == maMonHoc.Value);
            if (maHocKy.HasValue)   query = query.Where(d => d.MaHocKy   == maHocKy.Value);
            if (maLop.HasValue)     query = query.Where(d => d.HocSinh.MaLop == maLop.Value);

            var total = await query.CountAsync();
            var items = await query
                .OrderBy(d => d.HocSinh.Lop.MaLopCode)
                .ThenBy(d => d.HocSinh.HoTen)
                .ThenBy(d => d.MonHoc.TenMon)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(d => MapToDto(d))
                .ToListAsync();

            return new PagedResult<DiemDto> { Items = items, TotalItems = total, PageNumber = pageNumber, PageSize = pageSize };
        }

        public async Task<DiemDto?> GetDiemByIdAsync(int id)
        {
            var d = await _context.DIEM.AsNoTracking()
                .Include(x => x.HocSinh).ThenInclude(h => h.Lop)
                .Include(x => x.MonHoc)
                .Include(x => x.HocKy)
                .FirstOrDefaultAsync(x => x.MaDiem == id);
            return d == null ? null : MapToDto(d);
        }

        public async Task<DiemDto?> GetDiemByKeyAsync(int maHocSinh, int maMonHoc, int maHocKy)
        {
            var d = await _context.DIEM.AsNoTracking()
                .Include(x => x.HocSinh).ThenInclude(h => h.Lop)
                .Include(x => x.MonHoc)
                .Include(x => x.HocKy)
                .FirstOrDefaultAsync(x => x.MaHocSinh == maHocSinh && x.MaMonHoc == maMonHoc && x.MaHocKy == maHocKy);
            return d == null ? null : MapToDto(d);
        }

        public async Task<DiemDto> UpsertDiemAsync(NhapDiemRequest request, int maGiaoVienNhap)
        {
            ValidateDiem(request);

            decimal? dtb = null;
            if (request.DiemMieng.HasValue && request.Diem15Phut.HasValue && request.DiemGiuaKy.HasValue && request.DiemCuoiKy.HasValue)
            {
                dtb = Math.Round(
                    request.DiemMieng.Value * 0.1m +
                    request.Diem15Phut.Value * 0.2m +
                    request.DiemGiuaKy.Value * 0.3m +
                    request.DiemCuoiKy.Value * 0.4m, 2);
            }

            var existing = await _context.DIEM.FirstOrDefaultAsync(d =>
                d.MaHocSinh == request.MaHocSinh &&
                d.MaMonHoc  == request.MaMonHoc   &&
                d.MaHocKy   == request.MaHocKy);

            if (existing != null)
            {
                existing.DiemMieng      = request.DiemMieng;
                existing.Diem15Phut     = request.Diem15Phut;
                existing.DiemGiuaKy     = request.DiemGiuaKy;
                existing.DiemCuoiKy     = request.DiemCuoiKy;
                existing.DiemTrungBinh  = dtb;
                existing.NgayCapNhat    = DateTime.Now;
                await _context.SaveChangesAsync();
                _logger.LogInformation("Cập nhật điểm MaDiem={id}", existing.MaDiem);
                return (await GetDiemByIdAsync(existing.MaDiem))!;
            }
            else
            {
                var entity = new Diem
                {
                    MaHocSinh       = request.MaHocSinh,
                    MaMonHoc        = request.MaMonHoc,
                    MaHocKy         = request.MaHocKy,
                    DiemMieng       = request.DiemMieng,
                    Diem15Phut      = request.Diem15Phut,
                    DiemGiuaKy      = request.DiemGiuaKy,
                    DiemCuoiKy      = request.DiemCuoiKy,
                    DiemTrungBinh  = dtb,
                    NgayCapNhat     = DateTime.Now
                };
                _context.DIEM.Add(entity);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Nhập điểm mới MaDiem={id}", entity.MaDiem);
                return (await GetDiemByIdAsync(entity.MaDiem))!;
            }
        }

        public async Task<List<DiemDto>> GetDiemOfHocSinhAsync(int maHocSinh, int? maHocKy)
        {
            var query = _context.DIEM.AsNoTracking()
                .Include(d => d.HocSinh).ThenInclude(h => h.Lop)
                .Include(d => d.MonHoc)
                .Include(d => d.HocKy)
                .Where(d => d.MaHocSinh == maHocSinh);

            if (maHocKy.HasValue)
                query = query.Where(d => d.MaHocKy == maHocKy.Value);

            return await query
                .OrderBy(d => d.HocKy.MaHocKy)
                .ThenBy(d => d.MonHoc.TenMon)
                .Select(d => MapToDto(d))
                .ToListAsync();
        }

        public async Task<bool> DeleteDiemAsync(int id)
        {
            var entity = await _context.DIEM.FindAsync(id);
            if (entity == null) return false;
            _context.DIEM.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }

        private static void ValidateDiem(NhapDiemRequest r)
        {
            static bool invalid(decimal? v) => v.HasValue && (v < 0 || v > 10);
            if (invalid(r.DiemMieng) || invalid(r.Diem15Phut) || invalid(r.DiemGiuaKy) || invalid(r.DiemCuoiKy))
                throw new InvalidOperationException("Điểm phải nằm trong khoảng 0 – 10.");
        }

        private static string? CalculateXepLoai(decimal? dtb)
        {
            if (!dtb.HasValue) return null;
            if (dtb >= 8.5m) return "Giỏi";
            if (dtb >= 7.0m) return "Khá";
            if (dtb >= 5.0m) return "Trung bình";
            if (dtb >= 3.5m) return "Yếu";
            return "Kém";
        }

        private static DiemDto MapToDto(Diem d) => new()
        {
            MaDiem           = d.MaDiem,
            MaHocSinh        = d.MaHocSinh,
            MaHS             = d.HocSinh?.MaHS            ?? string.Empty,
            TenHocSinh       = d.HocSinh?.HoTen           ?? string.Empty,
            TenLop           = d.HocSinh?.Lop?.TenLop     ?? string.Empty,
            MaMonHoc         = d.MaMonHoc,
            TenMon           = d.MonHoc?.TenMon            ?? string.Empty,
            MaHocKy          = d.MaHocKy,
            TenHocKy         = d.HocKy?.TenHocKy          ?? string.Empty,
            DiemMieng        = d.DiemMieng,
            Diem15Phut       = d.Diem15Phut,
            DiemGiuaKy       = d.DiemGiuaKy,
            DiemCuoiKy       = d.DiemCuoiKy,
            DiemTrungBinh    = d.DiemTrungBinh,
            XepLoai          = d.DiemTrungBinh.HasValue ? CalculateXepLoai(d.DiemTrungBinh.Value) : null,
            NgayCapNhat      = d.NgayCapNhat,
            TenGiaoVienNhap  = null
        };
    }
}
