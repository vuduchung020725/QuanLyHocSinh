using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLPhongHoc.Models;
using QLPhongHoc.Services;

namespace QLPhongHoc.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "ADMIN,TEACHER")]
    public class ApiGiaoVienController : ControllerBase
    {
        private readonly IGiaoVienService _giaoVienService;

        public ApiGiaoVienController(IGiaoVienService giaoVienService)
        {
            _giaoVienService = giaoVienService;
        }

        [HttpGet]
        public async Task<IActionResult> GetGiaoViens([FromQuery] string? keyword, [FromQuery] string? trangThai, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            bool? trangThaiBool = null;
            if (!string.IsNullOrWhiteSpace(trangThai))
            {
                if (bool.TryParse(trangThai, out var b)) trangThaiBool = b;
                else if (trangThai == "1" || trangThai.Equals("hoatdong", StringComparison.OrdinalIgnoreCase) || trangThai.Equals("daday", StringComparison.OrdinalIgnoreCase)) trangThaiBool = true;
                else if (trangThai == "0" || trangThai.Equals("khoa", StringComparison.OrdinalIgnoreCase) || trangThai.Equals("nghiphep", StringComparison.OrdinalIgnoreCase)) trangThaiBool = false;
            }

            var result = await _giaoVienService.GetGiaoViensAsync(keyword, trangThaiBool, pageNumber, pageSize);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetGiaoVienById(int id)
        {
            var result = await _giaoVienService.GetGiaoVienByIdAsync(id);
            if (result == null) return NotFound(new { message = "Không tìm thấy giáo viên." });
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> CreateGiaoVien([FromBody] CreateGiaoVienRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var result = await _giaoVienService.CreateGiaoVienAsync(request);
                return CreatedAtAction(nameof(GetGiaoVienById), new { id = result.MaGiaoVien }, result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> UpdateGiaoVien(int id, [FromBody] UpdateGiaoVienRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var result = await _giaoVienService.UpdateGiaoVienAsync(id, request);
                if (result == null) return NotFound(new { message = "Không tìm thấy giáo viên." });
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> DeleteGiaoVien(int id)
        {
            try
            {
                var success = await _giaoVienService.DeleteGiaoVienAsync(id);
                if (!success) return NotFound(new { message = "Không tìm thấy giáo viên." });
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
