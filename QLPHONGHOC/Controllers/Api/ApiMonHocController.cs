using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLPhongHoc.Models;
using QLPhongHoc.Services;

namespace QLPhongHoc.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "ADMIN,TEACHER,STUDENT")]
    public class ApiMonHocController : ControllerBase
    {
        private readonly IMonHocService _monHocService;

        public ApiMonHocController(IMonHocService monHocService)
        {
            _monHocService = monHocService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _monHocService.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _monHocService.GetByIdAsync(id);
            if (result == null) return NotFound(new { message = "Không tìm thấy môn học." });
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Create([FromBody] CreateMonHocRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var result = await _monHocService.CreateAsync(request);
                return CreatedAtAction(nameof(GetById), new { id = result.MaMonHoc }, result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateMonHocRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var result = await _monHocService.UpdateAsync(id, request);
                if (result == null) return NotFound(new { message = "Không tìm thấy môn học." });
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var success = await _monHocService.DeleteAsync(id);
                if (!success) return NotFound(new { message = "Không tìm thấy môn học." });
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
