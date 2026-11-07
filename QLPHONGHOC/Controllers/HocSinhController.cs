using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLPhongHoc.Models;
using QLPhongHoc.Services;

namespace QLPhongHoc.Controllers
{
    [Authorize(Roles = "ADMIN,TEACHER")]
    public class HocSinhController : Controller
    {
        private readonly IHocSinhService _hocSinhService;
        private readonly ILopService _lopService;

        public HocSinhController(IHocSinhService hocSinhService, ILopService lopService)
        {
            _hocSinhService = hocSinhService;
            _lopService = lopService;
        }

        public async Task<IActionResult> Index(string? keyword, int pageNumber = 1)
        {
            var result = await _hocSinhService.GetHocSinhsAsync(keyword, null, null, pageNumber, 50);
            ViewData["Keyword"] = keyword;
            return View(result);
        }

        public async Task<IActionResult> Create()
        {
            ViewData["Lops"] = await _lopService.GetLopsAsync(null, null);
            return View(new CreateHocSinhRequest());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateHocSinhRequest model)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Lops"] = await _lopService.GetLopsAsync(null, null);
                return View(model);
            }
            await _hocSinhService.CreateHocSinhAsync(model);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var dto = await _hocSinhService.GetHocSinhByIdAsync(id);
            if (dto == null) return NotFound();
            var model = new UpdateHocSinhRequest
            {
                MaHS = dto.MaHS,
                HoTen = dto.HoTen,
                NgaySinh = dto.NgaySinh,
                GioiTinh = dto.GioiTinh,
                DiaChi = dto.DiaChi,
                SoDienThoai = dto.SoDienThoai,
                Email = dto.Email,
                TenPhuHuynh = dto.TenPhuHuynh,
                SdtPhuHuynh = dto.SdtPhuHuynh,
                MaLop = dto.MaLop,
                TrangThai = dto.TrangThai,
                MaTaiKhoan = dto.MaTaiKhoan
            };
            ViewData["Lops"] = await _lopService.GetLopsAsync(null, null);
            ViewData["MaHocSinh"] = id;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateHocSinhRequest model)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Lops"] = await _lopService.GetLopsAsync(null, null);
                ViewData["MaHocSinh"] = id;
                return View(model);
            }
            await _hocSinhService.UpdateHocSinhAsync(id, model);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            var dto = await _hocSinhService.GetHocSinhByIdAsync(id);
            if (dto == null) return NotFound();
            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _hocSinhService.DeleteHocSinhAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
