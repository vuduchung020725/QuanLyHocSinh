using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QLPhongHoc.Models;
using QLPhongHoc.Services;

namespace QLPhongHoc.Controllers.Api
{
    [Route("api/zalo")]
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "ADMIN,TEACHER")]
    public class ApiZaloController : ControllerBase
    {
        private readonly IZaloNotificationService _zaloService;

        public ApiZaloController(IZaloNotificationService zaloService)
        {
            _zaloService = zaloService;
        }

        /// <summary>
        /// POST: /api/zalo/send-bang-diem - Gửi bảng điểm qua Zalo ZNS cho phụ huynh
        /// </summary>
        [HttpPost("send-bang-diem")]
        public async Task<IActionResult> SendBangDiem([FromBody] SendBangDiemApiRequest request)
        {
            if (request.MaHocSinh <= 0)
                return BadRequest(new { success = false, message = "MaHocSinh không hợp lệ." });

            try
            {
                var result = await _zaloService.SendBangDiemNotificationAsync(request.MaHocSinh, request.MaHocKy);
                return Ok(new { success = true, data = result });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// POST: /api/zalo/send-diem-danh - Gửi thông báo nghỉ học/vắng mặt cho phụ huynh
        /// </summary>
        [HttpPost("send-diem-danh")]
        public async Task<IActionResult> SendDiemDanh([FromBody] SendDiemDanhApiRequest request)
        {
            if (request.MaHocSinh <= 0)
                return BadRequest(new { success = false, message = "MaHocSinh không hợp lệ." });

            try
            {
                var ngayNghi = string.IsNullOrWhiteSpace(request.NgayNghi) ? DateTime.Now.ToString("dd/MM/yyyy") : request.NgayNghi;
                var lyDo = string.IsNullOrWhiteSpace(request.LyDo) ? "Không phép" : request.LyDo;
                var result = await _zaloService.SendDiemDanhNotificationAsync(request.MaHocSinh, ngayNghi, lyDo);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// POST: /api/zalo/send-custom - Gửi thông báo tùy biến cho phụ huynh
        /// </summary>
        [HttpPost("send-custom")]
        public async Task<IActionResult> SendCustom([FromBody] SendZaloNotificationRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _zaloService.SendCustomNotificationAsync(request);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// GET: /api/zalo/history?maHocSinh=1&limit=50 - Lấy nhật ký gửi tin nhắn Zalo
        /// </summary>
        [HttpGet("history")]
        public async Task<IActionResult> GetHistory([FromQuery] int? maHocSinh, [FromQuery] int limit = 50)
        {
            var history = await _zaloService.GetLichSuZaloAsync(maHocSinh, limit);
            return Ok(history);
        }
    }

    public class SendBangDiemApiRequest
    {
        public int MaHocSinh { get; set; }
        public int? MaHocKy { get; set; }
    }

    public class SendDiemDanhApiRequest
    {
        public int MaHocSinh { get; set; }
        public string? NgayNghi { get; set; }
        public string? LyDo { get; set; }
    }
}

