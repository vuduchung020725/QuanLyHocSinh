using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using QLPhongHoc.Models;
using QLPhongHoc.Models.Auth;
using QLPhongHoc.Services;

namespace QLPhongHoc.Controllers.Api
{
    [ApiController]
    [Route("api/v1/accounts")]
    [Authorize(Roles = "ADMIN")]
    public class ApiAccountController : ControllerBase
    {
        private readonly IAccountService _accountService;
        private readonly ILogger<ApiAccountController> _logger;

        public ApiAccountController(IAccountService accountService, ILogger<ApiAccountController> logger)
        {
            _accountService = accountService;
            _logger = logger;
        }

        /// <summary>
        /// Lấy danh sách tài khoản (có phân trang, lọc, tìm kiếm)
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAccounts(
            [FromQuery] string? keyword,
            [FromQuery] string? trangThai,
            [FromQuery] int? maVaiTro,
            [FromQuery] int page = 1,
            [FromQuery] int size = 10)
        {
            try
            {
                if (page < 1) page = 1;
                if (size < 1) size = 10;

                bool? trangThaiBool = null;
                if (!string.IsNullOrWhiteSpace(trangThai))
                {
                    if (bool.TryParse(trangThai, out var b)) trangThaiBool = b;
                    else if (trangThai.Equals("1", StringComparison.Ordinal)) trangThaiBool = true;
                    else if (trangThai.Equals("0", StringComparison.Ordinal)) trangThaiBool = false;
                    else if (trangThai.Equals("hoatdong", StringComparison.OrdinalIgnoreCase) ||
                             trangThai.Equals("active", StringComparison.OrdinalIgnoreCase)) trangThaiBool = true;
                    else if (trangThai.Equals("khoa", StringComparison.OrdinalIgnoreCase) ||
                             trangThai.Equals("locked", StringComparison.OrdinalIgnoreCase)) trangThaiBool = false;
                }

                var result = await _accountService.GetAccountsAsync(keyword, trangThaiBool, maVaiTro, page, size);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi xảy ra trong ApiAccountController -> GetAccounts");
                return StatusCode(500, new ApiErrorResponse
                {
                    ErrorCode = "INTERNAL_SERVER_ERROR",
                    Message = "Đã xảy ra lỗi hệ thống khi tải danh sách tài khoản."
                });
            }
        }

        /// <summary>
        /// Lấy chi tiết tài khoản theo ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAccountById(int id)
        {
            try
            {
                var account = await _accountService.GetAccountByIdAsync(id);
                if (account == null)
                {
                    return NotFound(new ApiErrorResponse
                    {
                        ErrorCode = "ACCOUNT_NOT_FOUND",
                        Message = $"Không tìm thấy tài khoản với mã {id}"
                    });
                }
                return Ok(account);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi xảy ra trong ApiAccountController -> GetAccountById ({id})");
                return StatusCode(500, new ApiErrorResponse
                {
                    ErrorCode = "INTERNAL_SERVER_ERROR",
                    Message = "Đã xảy ra lỗi hệ thống khi lấy thông tin tài khoản."
                });
            }
        }

        /// <summary>
        /// Cập nhật thông tin tài khoản (Admin cập nhật)
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAccount(int id, [FromBody] UpdateAccountRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiErrorResponse
                    {
                        ErrorCode = "INVALID_INPUT",
                        Message = "Thông tin cập nhật không hợp lệ",
                        Details = ModelState
                    });
                }

                var updated = await _accountService.UpdateAccountAsync(id, request);
                if (updated == null)
                {
                    return NotFound(new ApiErrorResponse
                    {
                        ErrorCode = "ACCOUNT_NOT_FOUND",
                        Message = $"Không tìm thấy tài khoản với mã {id} để cập nhật"
                    });
                }

                return Ok(updated);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiErrorResponse
                {
                    ErrorCode = "EMAIL_EXISTS",
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi xảy ra trong ApiAccountController -> UpdateAccount ({id})");
                return StatusCode(500, new ApiErrorResponse
                {
                    ErrorCode = "INTERNAL_SERVER_ERROR",
                    Message = "Đã xảy ra lỗi hệ thống khi cập nhật tài khoản."
                });
            }
        }

        /// <summary>
        /// Phê duyệt đăng ký tài khoản (Admin duyệt trạng thái Hoạt động / Từ chối)
        /// </summary>
        [HttpPost("{id}/approve")]
        public async Task<IActionResult> ApproveAccount(int id, [FromBody] ApproveAccountRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new ApiErrorResponse
                    {
                        ErrorCode = "INVALID_INPUT",
                        Message = "Dữ liệu duyệt không hợp lệ",
                        Details = ModelState
                    });
                }

                var result = await _accountService.ApproveAccountAsync(id, request);
                if (!result)
                {
                    return NotFound(new ApiErrorResponse
                    {
                        ErrorCode = "ACCOUNT_NOT_FOUND",
                        Message = $"Không tìm thấy tài khoản hoặc tài khoản không thể duyệt"
                    });
                }

                return Ok(new { Message = $"Đã duyệt tài khoản với trạng thái {request.TrangThai} thành công." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new ApiErrorResponse
                {
                    ErrorCode = "INVALID_OPERATION",
                    Message = ex.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi xảy ra trong ApiAccountController -> ApproveAccount ({id})");
                return StatusCode(500, new ApiErrorResponse
                {
                    ErrorCode = "INTERNAL_SERVER_ERROR",
                    Message = "Đã xảy ra lỗi hệ thống khi duyệt tài khoản."
                });
            }
        }

        /// <summary>
        /// Xóa tài khoản (Khóa mềm hoặc xóa cứng tùy thuộc liên kết dữ liệu)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAccount(int id)
        {
            try
            {
                var result = await _accountService.DeleteAccountAsync(id);
                if (!result)
                {
                    return NotFound(new ApiErrorResponse
                    {
                        ErrorCode = "ACCOUNT_NOT_FOUND",
                        Message = $"Không tìm thấy tài khoản với mã {id} để xóa"
                    });
                }

                return Ok(new { Message = "Đã thực hiện xóa tài khoản thành công." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi xảy ra trong ApiAccountController -> DeleteAccount ({id})");
                return StatusCode(500, new ApiErrorResponse
                {
                    ErrorCode = "INTERNAL_SERVER_ERROR",
                    Message = "Đã xảy ra lỗi hệ thống khi xóa tài khoản."
                });
            }
        }
    }
}
