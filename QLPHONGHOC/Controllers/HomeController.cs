using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLPhongHoc.Data;
using QLPhongHoc.Models;

namespace QLPhongHoc.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly QLPhongHocContext _db;

        public HomeController(ILogger<HomeController> logger, QLPhongHocContext db)
        {
            _logger = logger;
            _db = db;
        }

        [Authorize]
        public IActionResult Index()
        {
            ViewData["Title"] = "Dashboard";
            ViewData["ActivePage"] = "Dashboard";
            ViewData["PageTitle"] = "Tổng quan";
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        /// <summary>
        /// GET /api/home/dashboard-summary – trả về tổng số liệu dashboard (AJAX)
        /// </summary>
        [HttpGet]
        [Authorize]
        [Route("api/home/dashboard-summary")]
        public async Task<IActionResult> DashboardSummary()
        {
            try
            {
                var tongHocSinh = await _db.HOCSINH.AsNoTracking().CountAsync(h => h.TrangThai == true);
                var tongGiaoVien = await _db.GIAOVIEN.AsNoTracking().CountAsync(g => g.TrangThai == true);
                var tongLop = await _db.LOP.AsNoTracking().CountAsync(l => l.TrangThai == true);
                var tongMonHoc = await _db.MONHOC.AsNoTracking().CountAsync(m => m.TrangThai == true);

                return Ok(new DashboardSummaryDto
                {
                    TongSoHocSinh = tongHocSinh,
                    TongSoGiaoVien = tongGiaoVien,
                    TongSoLop = tongLop,
                    TongSoMonHoc = tongMonHoc,
                    ThoiGianCapNhat = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = ex.Message });
            }
        }

        // New endpoint for dashboard charts data
        [HttpGet]
        [Authorize]
        [Route("api/home/dashboard-charts")]
        public async Task<IActionResult> DashboardCharts()
        {
            try
            {
                // 1. Xếp loại học lực (current semester)
                var currentHocKy = await _db.HOCKY.OrderByDescending(h => h.MaHocKy).FirstOrDefaultAsync();
                var currentHocKyId = currentHocKy?.MaHocKy ?? 0;
                var diems = await _db.DIEM
                    .AsNoTracking()
                    .Where(d => d.MaHocKy == currentHocKyId && d.DiemTrungBinh != null)
                    .Select(d => new { d.DiemTrungBinh, d.MaHocSinh })
                    .ToListAsync();

                var xepLoai = new XepLoaiChartDto();
                foreach (var item in diems)
                {
                    if (item.DiemTrungBinh >= 8.5m) xepLoai.Gioi++;
                    else if (item.DiemTrungBinh >= 7.0m) xepLoai.Kha++;
                    else if (item.DiemTrungBinh >= 5.0m) xepLoai.TrungBinh++;
                    else xepLoai.Yeu++;
                }

                // 2. Sĩ số theo khối
                var siSo = await _db.HOCSINH
                    .AsNoTracking()
                    .Where(h => h.TrangThai == true)
                    .GroupBy(h => h.Lop.Khoi)
                    .Select(g => new { Khoi = g.Key, Count = g.Count() })
                    .ToListAsync();
                var siSoKhoi = new SiSoKhoiChartDto();
                foreach (var g in siSo)
                {
                    switch (g.Khoi)
                    {
                        case 10: siSoKhoi.Khoi10 = g.Count; break;
                        case 11: siSoKhoi.Khoi11 = g.Count; break;
                        case 12: siSoKhoi.Khoi12 = g.Count; break;
                    }
                }

                // 3. Tài khoản chờ duyệt
                var taiKhoanCho = await _db.TAIKHOAN.AsNoTracking().CountAsync(t => t.TrangThai == false);

                // 4. Hoạt động gần nhất (last 5 grade updates)
                var recent = await _db.DIEM
                    .AsNoTracking()
                    .OrderByDescending(d => d.NgayCapNhat)
                    .Take(5)
                    .Select(d => new HoatDongGanDayDto
                    {
                        TenHocSinh = d.HocSinh.HoTen,
                        TenMonHoc = d.MonHoc.TenMon,
                        TenLop = d.HocSinh.Lop.TenLop,
                        DiemTrungBinh = d.DiemTrungBinh,
                        NgayCapNhat = d.NgayCapNhat
                    })
                    .ToListAsync();

                var result = new DashboardChartsDto
                {
                    XepLoaiHocLuc = xepLoai,
                    SiSoTheoKhoi = siSoKhoi,
                    TaiKhoanChoDuyet = taiKhoanCho,
                    HoatDongGanDay = recent
                };

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = ex.Message });
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
