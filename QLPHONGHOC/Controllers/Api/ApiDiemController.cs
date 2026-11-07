using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLPhongHoc.Models;
using QLPhongHoc.Services;

namespace QLPhongHoc.Controllers.Api
{
    [Route("api/diem")]
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "ADMIN,TEACHER,STUDENT")]
    public class ApiDiemController : ControllerBase
    {
        private readonly IDiemService _diemService;
        private readonly IExcelExportService _excelExportService;
        private readonly Data.QLPhongHocContext _dbContext;

        public ApiDiemController(
            IDiemService diemService,
            IExcelExportService excelExportService,
            Data.QLPhongHocContext dbContext)
        {
            _diemService = diemService;
            _excelExportService = excelExportService;
            _dbContext = dbContext;
        }

        private async Task<int?> GetHocSinhIdOfCurrentStudentAsync()
        {
            if (!User.IsInRole("STUDENT")) return null;
            var maTaiKhoanStr = User.FindFirst("MaTaiKhoan")?.Value;
            if (string.IsNullOrEmpty(maTaiKhoanStr) || !int.TryParse(maTaiKhoanStr, out var maTaiKhoan)) return null;
            var hs = await _dbContext.HOCSINH.AsNoTracking().FirstOrDefaultAsync(h => h.MaTaiKhoan == maTaiKhoan);
            return hs?.MaHocSinh;
        }

        private async Task<int?> GetMaGiaoVienOfCurrentTeacherAsync()
        {
            if (!User.IsInRole("TEACHER")) return null;
            var maTaiKhoanStr = User.FindFirst("MaTaiKhoan")?.Value;
            if (string.IsNullOrEmpty(maTaiKhoanStr) || !int.TryParse(maTaiKhoanStr, out var maTaiKhoan)) return null;
            var gv = await _dbContext.GIAOVIEN.AsNoTracking().FirstOrDefaultAsync(g => g.MaTaiKhoan == maTaiKhoan);
            return gv?.MaGiaoVien;
        }

        [HttpGet]
        public async Task<IActionResult> GetDiems([FromQuery] int? maHocSinh, [FromQuery] int? maMonHoc, [FromQuery] int? maHocKy, [FromQuery] int? maLop, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var currentHsId = await GetHocSinhIdOfCurrentStudentAsync();
            if (currentHsId.HasValue)
                maHocSinh = currentHsId.Value;

            var result = await _diemService.GetDiemsAsync(maHocSinh, maMonHoc, maHocKy, maLop, pageNumber, pageSize);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDiemById(int id)
        {
            var result = await _diemService.GetDiemByIdAsync(id);
            if (result == null) return NotFound(new { message = "Không tìm thấy điểm." });

            var currentHsId = await GetHocSinhIdOfCurrentStudentAsync();
            if (currentHsId.HasValue && result.MaHocSinh != currentHsId.Value)
                return Forbid();

            return Ok(result);
        }

        [HttpGet("hocsinh/{maHocSinh}")]
        public async Task<IActionResult> GetDiemOfHocSinh(int maHocSinh, [FromQuery] int? maHocKy)
        {
            var currentHsId = await GetHocSinhIdOfCurrentStudentAsync();
            if (currentHsId.HasValue && currentHsId.Value != maHocSinh)
                return Forbid();

            var result = await _diemService.GetDiemOfHocSinhAsync(maHocSinh, maHocKy);
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN,TEACHER")]
        public async Task<IActionResult> UpsertDiem([FromBody] NhapDiemRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                bool isAdmin = User.IsInRole("ADMIN");
                int maGiaoVienNhap = 0;

                if (!isAdmin)
                {
                    var gvId = await GetMaGiaoVienOfCurrentTeacherAsync();
                    if (!gvId.HasValue)
                    {
                        return StatusCode(StatusCodes.Status403Forbidden, new { success = false, message = "Tài khoản giáo viên chưa được liên kết với dữ liệu GIAOVIEN trong hệ thống. Vui lòng liên hệ ADMIN." });
                    }
                    maGiaoVienNhap = gvId.Value;
                }

                var result = await _diemService.UpsertDiemAsync(request, maGiaoVienNhap, isAdmin);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { success = false, message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// POST: /api/diem/update-grade - Endpoint chuẩn theo đặc tả hệ thống
        /// </summary>
        [HttpPost("update-grade")]
        [Authorize(Roles = "ADMIN,TEACHER")]
        public Task<IActionResult> UpdateGrade([FromBody] NhapDiemRequest request) => UpsertDiem(request);

        /// <summary>
        /// GET: /api/diem/export-excel?maLop=1&maHocKy=1 - Tải file Excel bảng điểm của lớp
        /// </summary>
        [HttpGet("export-excel")]
        [Authorize(Roles = "ADMIN,TEACHER")]
        public async Task<IActionResult> ExportExcel([FromQuery] int maLop, [FromQuery] int maHocKy)
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
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> DeleteDiem(int id)
        {
            try
            {
                var success = await _diemService.DeleteDiemAsync(id);
                if (!success) return NotFound(new { message = "Không tìm thấy điểm." });
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
