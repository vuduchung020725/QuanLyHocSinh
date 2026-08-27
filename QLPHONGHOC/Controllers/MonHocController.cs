using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLPhongHoc.Models;
using QLPhongHoc.Services;

namespace QLPhongHoc.Controllers
{
    [Authorize(Roles = "ADMIN,TEACHER,STUDENT")]
    public class MonHocController : Controller
    {
        private readonly IMonHocService _monHocService;

        public MonHocController(IMonHocService monHocService)
        {
            _monHocService = monHocService;
        }

        public async Task<IActionResult> Index()
        {
            var items = await _monHocService.GetAllAsync();
            return View(items);
        }

        [Authorize(Roles = "ADMIN")]
        public IActionResult Create() => View(new CreateMonHocRequest());

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Create(CreateMonHocRequest model)
        {
            if (!ModelState.IsValid) return View(model);
            await _monHocService.CreateAsync(model);
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Edit(int id)
        {
            var dto = await _monHocService.GetByIdAsync(id);
            if (dto == null) return NotFound();
            var model = new UpdateMonHocRequest { MaMon = dto.MaMon, TenMon = dto.TenMon, SoTiet = dto.SoTiet, GhiChu = dto.GhiChu };
            ViewData["MaMonHoc"] = id;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Edit(int id, UpdateMonHocRequest model)
        {
            if (!ModelState.IsValid) { ViewData["MaMonHoc"] = id; return View(model); }
            await _monHocService.UpdateAsync(id, model);
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "ADMIN")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _monHocService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Details(int id)
        {
            var dto = await _monHocService.GetByIdAsync(id);
            if (dto == null) return NotFound();
            return View(dto);
        }
    }
}
