using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QLPhongHoc.Data;
using QLPhongHoc.Models;

namespace QLPhongHoc.Services
{
    public interface IAccountService
    {
        Task<PagedResult<AccountDto>> GetAccountsAsync(string? keyword, bool? trangThai, int? maVaiTro, int pageNumber, int pageSize);
        Task<AccountDto?> GetAccountByIdAsync(int id);
        Task<AccountDto?> UpdateAccountAsync(int id, UpdateAccountRequest request);
        Task<bool> ApproveAccountAsync(int id, ApproveAccountRequest request);
        Task<bool> DeleteAccountAsync(int id); // Xóa mềm: đổi trạng thái thành false hoặc xóa cứng
    }

    public class AccountService : IAccountService
    {
        private readonly QLPhongHocContext _context;
        private readonly ILogger<AccountService> _logger;

        public AccountService(QLPhongHocContext context, ILogger<AccountService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<PagedResult<AccountDto>> GetAccountsAsync(string? keyword, bool? trangThai, int? maVaiTro, int pageNumber, int pageSize)
        {
            try
            {
                var query = _context.TAIKHOAN.AsNoTracking().Include(t => t.VaiTro).AsQueryable();

                if (!string.IsNullOrWhiteSpace(keyword))
                {
                    var kw = keyword.Trim().ToLower();
                    query = query.Where(t => t.TenDangNhap.ToLower().Contains(kw) ||
                                             t.HoTen.ToLower().Contains(kw) ||
                                             t.Email.ToLower().Contains(kw) ||
                                             (t.SoDienThoai != null && t.SoDienThoai.Contains(kw)));
                }

                if (trangThai.HasValue)
                {
                    query = query.Where(t => t.TrangThai == trangThai.Value);
                }

                if (maVaiTro.HasValue)
                {
                    query = query.Where(t => t.MaVaiTro == maVaiTro.Value);
                }

                var totalItems = await query.CountAsync();
                var items = await query
                    .OrderBy(t => t.MaTaiKhoan)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(t => new AccountDto
                    {
                        MaTaiKhoan = t.MaTaiKhoan,
                        TenDangNhap = t.TenDangNhap,
                        HoTen = t.HoTen,
                        Email = t.Email,
                        SoDienThoai = t.SoDienThoai,
                        MaVaiTro = t.MaVaiTro,
                        TenVaiTro = t.VaiTro != null ? t.VaiTro.TenVaiTro : string.Empty,
                        TrangThai = t.TrangThai
                    })
                    .ToListAsync();

                return new PagedResult<AccountDto>
                {
                    Items = items,
                    TotalItems = totalItems,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi xảy ra trong GetAccountsAsync");
                throw;
            }
        }

        public async Task<AccountDto?> GetAccountByIdAsync(int id)
        {
            try
            {
                var user = await _context.TAIKHOAN
                    .AsNoTracking()
                    .Include(t => t.VaiTro)
                    .FirstOrDefaultAsync(t => t.MaTaiKhoan == id);

                if (user == null) return null;

                return new AccountDto
                {
                    MaTaiKhoan = user.MaTaiKhoan,
                    TenDangNhap = user.TenDangNhap,
                    HoTen = user.HoTen,
                    Email = user.Email,
                    SoDienThoai = user.SoDienThoai,
                    MaVaiTro = user.MaVaiTro,
                    TenVaiTro = user.VaiTro != null ? user.VaiTro.TenVaiTro : string.Empty,
                    TrangThai = user.TrangThai
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi xảy ra trong GetAccountByIdAsync cho MaTaiKhoan: {id}");
                throw;
            }
        }

        public async Task<AccountDto?> UpdateAccountAsync(int id, UpdateAccountRequest request)
        {
            try
            {
                var user = await _context.TAIKHOAN.FindAsync(id);
                if (user == null) return null;

                // Kiểm tra trùng Email
                var emailExists = await _context.TAIKHOAN.AnyAsync(t => t.Email == request.Email && t.MaTaiKhoan != id);
                if (emailExists)
                {
                    throw new InvalidOperationException("Email đã được sử dụng bởi một tài khoản khác.");
                }

                // Cập nhật thông tin
                user.HoTen = request.HoTen;
                user.Email = request.Email;
                user.SoDienThoai = request.SoDienThoai;
                user.MaVaiTro = request.MaVaiTro;
                user.TrangThai = request.TrangThai;

                _context.TAIKHOAN.Update(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Admin cập nhật thành công tài khoản: {user.TenDangNhap}");

                return await GetAccountByIdAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi xảy ra trong UpdateAccountAsync cho MaTaiKhoan: {id}");
                throw;
            }
        }

        public async Task<bool> ApproveAccountAsync(int id, ApproveAccountRequest request)
        {
            try
            {
                var user = await _context.TAIKHOAN.FindAsync(id);
                if (user == null) return false;

                if (user.TrangThai)
                {
                    throw new InvalidOperationException("Tài khoản đã được duyệt hoặc đang hoạt động.");
                }

                user.TrangThai = request.TrangThai;
                _context.TAIKHOAN.Update(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Thay đổi trạng thái duyệt cho tài khoản {user.TenDangNhap} thành {request.TrangThai}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi xảy ra trong ApproveAccountAsync cho MaTaiKhoan: {id}");
                throw;
            }
        }

        public async Task<bool> DeleteAccountAsync(int id)
        {
            try
            {
                var user = await _context.TAIKHOAN.FindAsync(id);
                if (user == null) return false;

                // Để an toàn, chúng ta thực hiện khóa thay vì xóa cứng nếu tài khoản đã có dữ liệu liên quan
                var hasHocSinh = await _context.HOCSINH.AnyAsync(y => y.MaTaiKhoan == id);
                var hasGiaoVien = await _context.GIAOVIEN.AnyAsync(s => s.MaTaiKhoan == id);

                if (hasHocSinh || hasGiaoVien)
                {
                    // Chuyển sang Khóa (Soft delete/Deactivate)
                    user.TrangThai = false;
                    _context.TAIKHOAN.Update(user);
                    _logger.LogInformation($"Tài khoản {user.TenDangNhap} có liên quan dữ liệu khác, chuyển trạng thái thành Khóa.");
                }
                else
                {
                    // Xóa cứng khỏi database
                    _context.TAIKHOAN.Remove(user);
                    _logger.LogInformation($"Xóa cứng tài khoản {user.TenDangNhap} thành công.");
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Lỗi xảy ra trong DeleteAccountAsync cho MaTaiKhoan: {id}");
                throw;
            }
        }
    }
}
