using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLPhongHoc.Models;
using QLPhongHoc.Services;

namespace QLPhongHoc.Controllers
{
    [Authorize(Roles = "ADMIN,TEACHER,STUDENT")]
    public class DiemController : Controller
    {
        private readonly IDiemService _diemService;
        private readonly IHocSinhService _hocSinhService;
        private readonly IMonHocService _monHocService;
        private readonly INamHocService _namHocService;

        public DiemController(IDiemService diemService, IHocSinhService hocSinhService, IMonHocService monHocService, INamHocService namHocService)
        {
            _diemService = diemService;
            _hocSinhService = hocSinhService;
            _monHocService = monHocService;
            _namHocService = namHocService;
        }

        public async Task<IActionResult> Index(int? maHocSinh, int? maMonHoc, int? maHocKy)
        {
            var data = await _diemService.GetDiemsAsync(maHocSinh, maMonHoc, maHocKy, null, 1, 200);
            return View(data);
        }

        [Authorize(Roles = "ADMIN,TEACHER")]
        public async Task<IActionResult> Create()
        {
            ViewData["HocSinhs"] = await _hocSinhService.GetHocSinhsAsync(null, null, null, 1, 500);
            ViewData["MonHocs"] = await _monHocService.GetAllAsync();
            var nh = await _namHocService.GetHienTaiAsync();
            ViewData["HocKys"] = nh != null ? await _namHocService.GetHocKysByNamHocAsync(nh.MaNamHoc) : new List<HocKyDto>();
            return View(new NhapDiemRequest());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ADMIN,TEACHER")]
        public async Task<IActionResult> Create(NhapDiemRequest model)
        {
            if (!ModelState.IsValid)
            {
                ViewData["HocSinhs"] = await _hocSinhService.GetHocSinhsAsync(null, null, null, 1, 500);
                ViewData["MonHocs"] = await _monHocService.GetAllAsync();
                var nh = await _namHocService.GetHienTaiAsync();
                ViewData["HocKys"] = nh != null ? await _namHocService.GetHocKysByNamHocAsync(nh.MaNamHoc) : new List<HocKyDto>();
                return View(model);
            }
            await _diemService.UpsertDiemAsync(model, maGiaoVienNhap: 0);
            return RedirectToAction(nameof(Index));
        }
    }
}
