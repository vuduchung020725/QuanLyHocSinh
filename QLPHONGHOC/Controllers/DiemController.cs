using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLPhongHoc.Data;
using QLPhongHoc.Helpers;
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
        private readonly ILopService _lopService;
        private readonly IExcelExportService _excelExportService;
        private readonly IZaloNotificationService _zaloService;
        private readonly QLPhongHocContext _dbContext;

        public DiemController(
            IDiemService diemService,
            IHocSinhService hocSinhService,
            IMonHocService monHocService,
            INamHocService namHocService,
            ILopService lopService,
            IExcelExportService excelExportService,
            IZaloNotificationService zaloService,
            QLPhongHocContext dbContext)
        {
            _diemService = diemService;
            _hocSinhService = hocSinhService;
            _monHocService = monHocService;
            _namHocService = namHocService;
            _lopService = lopService;
            _excelExportService = excelExportService;
            _zaloService = zaloService;
            _dbContext = dbContext;
        }

        public async Task<IActionResult> Index(int? maHocSinh, int? maMonHoc, int? maHocKy, int? maLop)
        {
            ViewData["ActivePage"] = "Diem";
            ViewData["Title"] = "Quản lý Điểm số";
            ViewData["Lops"] = await _lopService.GetLopsAsync(null, null);
            ViewData["MonHocs"] = await _monHocService.GetAllAsync();
            var nh = await _namHocService.GetHienTaiAsync();
            ViewData["HocKys"] = nh != null ? await _namHocService.GetHocKysByNamHocAsync(nh.MaNamHoc) : new List<HocKyDto>();
            ViewData["CurrentLop"] = maLop;
            ViewData["CurrentHocKy"] = maHocKy;

            var data = await _diemService.GetDiemsAsync(maHocSinh, maMonHoc, maHocKy, maLop, 1, 200);
            return View(data);
        }

        [Authorize(Roles = "ADMIN,TEACHER")]
        public async Task<IActionResult> Create()
        {
            await LoadDropdownsAsync();
            return View(new NhapDiemRequest());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ADMIN,TEACHER")]
        public async Task<IActionResult> Create(NhapDiemRequest model)
        {
            if (!ModelState.IsValid)
            {
                await LoadDropdownsAsync();
                return View(model);
            }

            try
            {
                var role = HttpContext.Session.GetString(SessionHelper.SessionTenVaiTro);
                bool isAdmin = User.IsInRole("ADMIN") || (role != null && role.Equals("ADMIN", StringComparison.OrdinalIgnoreCase));

                int maGiaoVienNhap = 0;
                if (!isAdmin)
                {
                    var maTaiKhoanStr = HttpContext.Session.GetString(SessionHelper.SessionUserId);
                    if (int.TryParse(maTaiKhoanStr, out int maTaiKhoan))
                    {
                        var gv = await _dbContext.GIAOVIEN.AsNoTracking().FirstOrDefaultAsync(g => g.MaTaiKhoan == maTaiKhoan);
                        if (gv != null)
                        {
                            maGiaoVienNhap = gv.MaGiaoVien;
                        }
                    }

                    if (maGiaoVienNhap == 0)
                    {
                        ModelState.AddModelError(string.Empty, "Tài khoản giáo viên chưa được liên kết với hồ sơ Giáo viên.");
                        await LoadDropdownsAsync();
                        return View(model);
                    }
                }

                await _diemService.UpsertDiemAsync(model, maGiaoVienNhap, isAdmin);
                TempData["Success"] = "Nhập và tính điểm trung bình thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (UnauthorizedAccessException ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await LoadDropdownsAsync();
                return View(model);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);
                await LoadDropdownsAsync();
                return View(model);
            }
        }

        /// <summary>
        /// GET: /Diem/ExportExcel - Xuất bảng điểm lớp ra file .xlsx bằng ClosedXML
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "ADMIN,TEACHER")]
        public async Task<IActionResult> ExportExcel(int maLop, int maHocKy)
        {
            try
            {
                var bytes = await _excelExportService.ExportBangDiemLopAsync(maLop, maHocKy);
                var lop = await _dbContext.LOP.FindAsync(maLop);
                var tenLopSlug = lop?.TenLop?.Replace(" ", "_") ?? $"Lop{maLop}";
                var fileName = $"BangDiem_{tenLopSlug}_HK{maHocKy}_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";
                return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Lỗi khi xuất bảng điểm Excel: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// POST: /Diem/SendZaloBangDiem - Gửi bảng điểm tới Zalo phụ huynh học sinh
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "ADMIN,TEACHER")]
        public async Task<IActionResult> SendZaloBangDiem(int maHocSinh, int? maHocKy)
        {
            try
            {
                var result = await _zaloService.SendBangDiemNotificationAsync(maHocSinh, maHocKy);
                TempData["Success"] = $"Đã gửi thông báo điểm qua Zalo cho phụ huynh học sinh {result.TenHocSinh} (SĐT: {result.SoDienThoaiNhan}) thành công!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Không thể gửi Zalo: {ex.Message}";
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadDropdownsAsync()
        {
            ViewData["HocSinhs"] = await _hocSinhService.GetHocSinhsAsync(null, null, null, 1, 500);
            ViewData["MonHocs"] = await _monHocService.GetAllAsync();
            var nh = await _namHocService.GetHienTaiAsync();
            ViewData["HocKys"] = nh != null ? await _namHocService.GetHocKysByNamHocAsync(nh.MaNamHoc) : new List<HocKyDto>();
        }
    }
}
