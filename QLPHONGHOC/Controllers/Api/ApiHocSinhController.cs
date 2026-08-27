using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLPhongHoc.Models;
using QLPhongHoc.Services;

namespace QLPhongHoc.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "ADMIN,TEACHER,STUDENT")]
    public class ApiHocSinhController : ControllerBase
    {
        private readonly IHocSinhService _hocSinhService;
        private readonly Data.QLPhongHocContext _dbContext;

        public ApiHocSinhController(IHocSinhService hocSinhService, Data.QLPhongHocContext dbContext)
        {
            _hocSinhService = hocSinhService;
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

        [HttpGet]
        public async Task<IActionResult> GetHocSinhs([FromQuery] string? keyword, [FromQuery] int? maLop, [FromQuery] string? trangThai, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            bool? trangThaiBool = null;
            if (!string.IsNullOrWhiteSpace(trangThai))
            {
                if (bool.TryParse(trangThai, out var b)) trangThaiBool = b;
                else if (trangThai == "1" || trangThai.Equals("hoatdong", StringComparison.OrdinalIgnoreCase)) trangThaiBool = true;
                else if (trangThai == "0" || trangThai.Equals("khoa", StringComparison.OrdinalIgnoreCase)) trangThaiBool = false;
            }

            var currentHsId = await GetHocSinhIdOfCurrentStudentAsync();
            if (currentHsId.HasValue)
            {
                var self = await _hocSinhService.GetHocSinhByIdAsync(currentHsId.Value);
                if (self == null) return NotFound(new { success = false, message = "Không tìm thấy thông tin học sinh." });
                return Ok(new PagedResult<HocSinhDto>
                {
                    Items = new List<HocSinhDto> { self },
                    TotalItems = 1,
                    PageNumber = 1,
                    PageSize = 1
                });
            }

            var result = await _hocSinhService.GetHocSinhsAsync(keyword, maLop, trangThaiBool, pageNumber, pageSize);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetHocSinhById(int id)
        {
            var currentHsId = await GetHocSinhIdOfCurrentStudentAsync();
            if (currentHsId.HasValue && currentHsId.Value != id)
                return Forbid();

            var result = await _hocSinhService.GetHocSinhByIdAsync(id);
            if (result == null) return NotFound(new { message = "Không tìm thấy học sinh." });
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> CreateHocSinh([FromBody] CreateHocSinhRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var result = await _hocSinhService.CreateHocSinhAsync(request);
                return CreatedAtAction(nameof(GetHocSinhById), new { id = result.MaHocSinh }, result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> UpdateHocSinh(int id, [FromBody] UpdateHocSinhRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var result = await _hocSinhService.UpdateHocSinhAsync(id, request);
                if (result == null) return NotFound(new { message = "Không tìm thấy học sinh." });
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> DeleteHocSinh(int id)
        {
            try
            {
                var success = await _hocSinhService.DeleteHocSinhAsync(id);
                if (!success) return NotFound(new { message = "Không tìm thấy học sinh." });
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
