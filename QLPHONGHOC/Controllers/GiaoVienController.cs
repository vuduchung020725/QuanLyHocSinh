using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLPhongHoc.Models;
using QLPhongHoc.Services;

namespace QLPhongHoc.Controllers
{
    [Authorize(Roles = "ADMIN")]
    public class GiaoVienController : Controller
    {
        private readonly IGiaoVienService _giaoVienService;

        public GiaoVienController(IGiaoVienService giaoVienService)
        {
            _giaoVienService = giaoVienService;
        }

        public async Task<IActionResult> Index(string? keyword)
        {
            var result = await _giaoVienService.GetGiaoViensAsync(keyword, null, 1, 200);
            return View(result);
        }

        public IActionResult Create() => View(new CreateGiaoVienRequest());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateGiaoVienRequest model)
        {
            if (!ModelState.IsValid) return View(model);
            await _giaoVienService.CreateGiaoVienAsync(model);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var dto = await _giaoVienService.GetGiaoVienByIdAsync(id);
            if (dto == null) return NotFound();
            var model = new UpdateGiaoVienRequest
            {
                MaGV = dto.MaGV,
                HoTen = dto.HoTen,
                NgaySinh = dto.NgaySinh,
                GioiTinh = dto.GioiTinh,
                SoDienThoai = dto.SoDienThoai,
                Email = dto.Email,
                DiaChi = dto.DiaChi,
                ChuyenMon = dto.ChuyenMon,
                TrangThai = dto.TrangThai,
                MaTaiKhoan = dto.MaTaiKhoan
            };
            ViewData["MaGiaoVien"] = id;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateGiaoVienRequest model)
        {
            if (!ModelState.IsValid) { ViewData["MaGiaoVien"] = id; return View(model); }
            await _giaoVienService.UpdateGiaoVienAsync(id, model);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            var dto = await _giaoVienService.GetGiaoVienByIdAsync(id);
            if (dto == null) return NotFound();
            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _giaoVienService.DeleteGiaoVienAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
