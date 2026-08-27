using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLPhongHoc.Models;
using QLPhongHoc.Services;

namespace QLPhongHoc.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "ADMIN,TEACHER,STUDENT")]
    public class ApiLopController : ControllerBase
    {
        private readonly ILopService _lopService;

        public ApiLopController(ILopService lopService)
        {
            _lopService = lopService;
        }

        [HttpGet]
        public async Task<IActionResult> GetLops([FromQuery] int? maNamHoc, [FromQuery] string? khoi)
        {
            int? khoiInt = null;
            if (!string.IsNullOrWhiteSpace(khoi) && int.TryParse(khoi, out var k))
            {
                khoiInt = k;
            }

            var result = await _lopService.GetLopsAsync(maNamHoc, khoiInt);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetLopById(int id)
        {
            var result = await _lopService.GetLopByIdAsync(id);
            if (result == null) return NotFound(new { message = "Không tìm thấy lớp." });
            return Ok(result);
        }

        [HttpGet("{id}/hocsinh")]
        public async Task<IActionResult> GetHocSinhTrongLop(int id, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _lopService.GetHocSinhTrongLopAsync(id, pageNumber, pageSize);
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> CreateLop([FromBody] CreateLopRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var result = await _lopService.CreateLopAsync(request);
                return CreatedAtAction(nameof(GetLopById), new { id = result.MaLop }, result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> UpdateLop(int id, [FromBody] UpdateLopRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var result = await _lopService.UpdateLopAsync(id, request);
                if (result == null) return NotFound(new { message = "Không tìm thấy lớp." });
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> DeleteLop(int id)
        {
            try
            {
                var success = await _lopService.DeleteLopAsync(id);
                if (!success) return NotFound(new { message = "Không tìm thấy lớp." });
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
