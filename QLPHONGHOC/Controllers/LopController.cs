using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLPhongHoc.Models;
using QLPhongHoc.Services;

namespace QLPhongHoc.Controllers
{
    [Authorize(Roles = "ADMIN,TEACHER")]
    public class LopController : Controller
    {
        private readonly ILopService _lopService;
        private readonly INamHocService _namHocService;
        private readonly IGiaoVienService _giaoVienService;

        public LopController(ILopService lopService, INamHocService namHocService, IGiaoVienService giaoVienService)
        {
            _lopService = lopService;
            _namHocService = namHocService;
            _giaoVienService = giaoVienService;
        }

        public async Task<IActionResult> Index(int? maNamHoc)
        {
            var items = await _lopService.GetLopsAsync(maNamHoc, null);
            ViewData["NamHocs"] = await _namHocService.GetAllAsync();
            return View(items);
        }

        public async Task<IActionResult> Create()
        {
            ViewData["NamHocs"] = await _namHocService.GetAllAsync();
            ViewData["GiaoViens"] = await _giaoVienService.GetGiaoViensAsync(null, null, 1, 200);
            return View(new CreateLopRequest());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateLopRequest model)
        {
            if (!ModelState.IsValid)
            {
                ViewData["NamHocs"] = await _namHocService.GetAllAsync();
                ViewData["GiaoViens"] = await _giaoVienService.GetGiaoViensAsync(null, null, 1, 200);
                return View(model);
            }
            await _lopService.CreateLopAsync(model);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var dto = await _lopService.GetLopByIdAsync(id);
            if (dto == null) return NotFound();
            var model = new UpdateLopRequest
            {
                MaLopCode = dto.MaLopCode,
                TenLop = dto.TenLop,
                Khoi = dto.Khoi,
                MaNamHoc = dto.MaNamHoc,
                MaGVCN = dto.MaGVCN,
                TrangThai = dto.TrangThai
            };
            ViewData["NamHocs"] = await _namHocService.GetAllAsync();
            ViewData["GiaoViens"] = await _giaoVienService.GetGiaoViensAsync(null, null, 1, 200);
            ViewData["MaLop"] = id;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateLopRequest model)
        {
            if (!ModelState.IsValid)
            {
                ViewData["NamHocs"] = await _namHocService.GetAllAsync();
                ViewData["GiaoViens"] = await _giaoVienService.GetGiaoViensAsync(null, null, 1, 200);
                ViewData["MaLop"] = id;
                return View(model);
            }
            await _lopService.UpdateLopAsync(id, model);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            var dto = await _lopService.GetLopByIdAsync(id);
            if (dto == null) return NotFound();
            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _lopService.DeleteLopAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
