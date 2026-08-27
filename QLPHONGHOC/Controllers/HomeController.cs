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

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
