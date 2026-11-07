using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using QLPhongHoc.Data;
using QLPhongHoc.Models;

namespace QLPhongHoc.Services
{
    public interface IZaloNotificationService
    {
        Task<LichSuZaloDto> SendBangDiemNotificationAsync(int maHocSinh, int? maHocKy);
        Task<LichSuZaloDto> SendDiemDanhNotificationAsync(int maHocSinh, string ngayNghi, string lyDo);
        Task<LichSuZaloDto> SendCustomNotificationAsync(SendZaloNotificationRequest request);
        Task<List<LichSuZaloDto>> GetLichSuZaloAsync(int? maHocSinh, int limit = 50);
    }

    public class ZaloNotificationService : IZaloNotificationService
    {
        private readonly QLPhongHocContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ZaloNotificationService> _logger;
        private readonly HttpClient _httpClient;

        public ZaloNotificationService(
            QLPhongHocContext context,
            IConfiguration configuration,
            ILogger<ZaloNotificationService> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
            _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
        }

        public async Task<LichSuZaloDto> SendBangDiemNotificationAsync(int maHocSinh, int? maHocKy)
        {
            var hs = await _context.HOCSINH
                .Include(h => h.Lop)
                .FirstOrDefaultAsync(h => h.MaHocSinh == maHocSinh);

            if (hs == null)
                throw new ArgumentException("Không tìm thấy học sinh.");

            if (string.IsNullOrWhiteSpace(hs.SdtPhuHuynh))
                throw new InvalidOperationException($"Học sinh {hs.HoTen} chưa có số điện thoại phụ huynh để gửi tin nhắn Zalo.");

            var diems = await _context.DIEM
                .Include(d => d.MonHoc)
                .Include(d => d.HocKy)
                .Where(d => d.MaHocSinh == maHocSinh && (!maHocKy.HasValue || d.MaHocKy == maHocKy.Value))
                .ToListAsync();

            var tenHocKy = diems.FirstOrDefault()?.HocKy?.TenHocKy ?? "Kỳ hiện tại";
            decimal? dtbChung = diems.Any(d => d.DiemTrungBinh.HasValue)
                ? Math.Round(diems.Where(d => d.DiemTrungBinh.HasValue).Average(d => d.DiemTrungBinh!.Value), 2)
                : null;

            string chiTietDiem = string.Join("; ", diems.Select(d => $"{d.MonHoc.TenMon}: {(d.DiemTrungBinh.HasValue ? d.DiemTrungBinh.Value.ToString("0.0") : "Chưa có")}"));
            string noiDung = $"Kính gửi phụ huynh em {hs.HoTen} (Lớp {hs.Lop?.TenLop}): Kết quả học tập {tenHocKy}: {(dtbChung.HasValue ? $"ĐTB: {dtbChung.Value:0.00}" : "Đang cập nhật")}. Chi tiết: {chiTietDiem}.";

            return await ExecuteSendAsync(hs, "Bảng điểm", noiDung);
        }

        public async Task<LichSuZaloDto> SendDiemDanhNotificationAsync(int maHocSinh, string ngayNghi, string lyDo)
        {
            var hs = await _context.HOCSINH
                .Include(h => h.Lop)
                .FirstOrDefaultAsync(h => h.MaHocSinh == maHocSinh);

            if (hs == null)
                throw new ArgumentException("Không tìm thấy học sinh.");

            if (string.IsNullOrWhiteSpace(hs.SdtPhuHuynh))
                throw new InvalidOperationException($"Học sinh {hs.HoTen} chưa có số điện thoại phụ huynh để gửi tin nhắn Zalo.");

            string noiDung = $"[THÔNG BÁO CHUYÊN CẦN] Kính gửi phụ huynh: Em {hs.HoTen} (Lớp {hs.Lop?.TenLop}) vắng mặt trong buổi học ngày {ngayNghi}. Lý do ghi nhận: {lyDo}. Xin phụ huynh lưu ý phối hợp cùng nhà trường.";

            return await ExecuteSendAsync(hs, "Điểm danh vắng mặt", noiDung);
        }

        public async Task<LichSuZaloDto> SendCustomNotificationAsync(SendZaloNotificationRequest request)
        {
            var hs = await _context.HOCSINH
                .Include(h => h.Lop)
                .FirstOrDefaultAsync(h => h.MaHocSinh == request.MaHocSinh);

            if (hs == null)
                throw new ArgumentException("Không tìm thấy học sinh.");

            if (string.IsNullOrWhiteSpace(hs.SdtPhuHuynh))
                throw new InvalidOperationException($"Học sinh {hs.HoTen} chưa có số điện thoại phụ huynh để gửi tin nhắn Zalo.");

            string noiDung = !string.IsNullOrWhiteSpace(request.NoiDungTuyBien)
                ? request.NoiDungTuyBien
                : $"Thông báo từ nhà trường gửi tới phụ huynh học sinh {hs.HoTen} (Lớp {hs.Lop?.TenLop}).";

            return await ExecuteSendAsync(hs, request.LoaiTinNhan, noiDung);
        }

        public async Task<List<LichSuZaloDto>> GetLichSuZaloAsync(int? maHocSinh, int limit = 50)
        {
            var query = _context.LICHSUZALO
                .Include(z => z.HocSinh).ThenInclude(h => h.Lop)
                .AsNoTracking()
                .AsQueryable();

            if (maHocSinh.HasValue)
            {
                query = query.Where(z => z.MaHocSinh == maHocSinh.Value);
            }

            var items = await query
                .OrderByDescending(z => z.ThoiGianGui)
                .Take(limit)
                .Select(z => new LichSuZaloDto
                {
                    MaLichSuZalo = z.MaLichSuZalo,
                    MaHocSinh = z.MaHocSinh,
                    TenHocSinh = z.HocSinh != null ? z.HocSinh.HoTen : string.Empty,
                    MaHS = z.HocSinh != null ? z.HocSinh.MaHS : string.Empty,
                    TenLop = z.HocSinh != null && z.HocSinh.Lop != null ? z.HocSinh.Lop.TenLop : string.Empty,
                    SoDienThoaiNhan = z.SoDienThoaiNhan,
                    LoaiTinNhan = z.LoaiTinNhan,
                    NoiDungGui = z.NoiDungGui,
                    TrangThai = z.TrangThai,
                    MessageIdZalo = z.MessageIdZalo,
                    ThoiGianGui = z.ThoiGianGui
                })
                .ToListAsync();

            return items;
        }

        private async Task<LichSuZaloDto> ExecuteSendAsync(HocSinh hs, string loaiTin, string noiDung)
        {
            // Bước 2: Chuẩn hóa số điện thoại về chuẩn Zalo (84xxx)
            string phoneFormatted = FormatPhoneNumberForZalo(hs.SdtPhuHuynh!);

            string messageId = $"ZNS_{DateTime.Now:yyyyMMddHHmmss}_{Guid.NewGuid().ToString("N")[..8]}";
            string trangThai = "Thành công";

            try
            {
                var accessToken = _configuration["ZaloSettings:AccessToken"];
                var templateId = _configuration["ZaloSettings:TemplateId"];

                if (!string.IsNullOrEmpty(accessToken) && !string.IsNullOrEmpty(templateId))
                {
                    // Bước 3 & 4: Gọi Zalo Cloud API thật nếu có cấu hình
                    var payload = new
                    {
                        phone = phoneFormatted,
                        template_id = templateId,
                        template_data = new
                        {
                            student_name = hs.HoTen,
                            content = noiDung,
                            date = DateTime.Now.ToString("dd/MM/yyyy HH:mm")
                        }
                    };

                    var requestMessage = new HttpRequestMessage(HttpMethod.Post, "https://business.openapi.zalo.me/message/template");
                    requestMessage.Headers.Add("access_token", accessToken);
                    requestMessage.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

                    var res = await _httpClient.SendAsync(requestMessage);
                    var resJson = await res.Content.ReadAsStringAsync();
                    _logger.LogInformation("Zalo API Response: {res}", resJson);

                    if (!res.IsSuccessStatusCode)
                    {
                        trangThai = "Thất bại";
                    }
                }
                else
                {
                    // Mô phỏng gửi thành công qua Zalo Sandbox/Gateway
                    _logger.LogInformation("Gửi Zalo ZNS (Simulated) tới {phone} cho học sinh {hs}: {nd}", phoneFormatted, hs.HoTen, noiDung);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi gọi Zalo ZNS API");
                trangThai = "Thất bại";
            }

            // Bước 5: Ghi nhật ký vào bảng LICHSUZALO
            var entity = new LichSuZalo
            {
                MaHocSinh = hs.MaHocSinh,
                SoDienThoaiNhan = phoneFormatted,
                LoaiTinNhan = loaiTin,
                NoiDungGui = noiDung,
                TrangThai = trangThai,
                MessageIdZalo = messageId,
                ThoiGianGui = DateTime.Now
            };

            _context.LICHSUZALO.Add(entity);
            await _context.SaveChangesAsync();

            return new LichSuZaloDto
            {
                MaLichSuZalo = entity.MaLichSuZalo,
                MaHocSinh = entity.MaHocSinh,
                TenHocSinh = hs.HoTen,
                MaHS = hs.MaHS,
                TenLop = hs.Lop?.TenLop ?? string.Empty,
                SoDienThoaiNhan = entity.SoDienThoaiNhan,
                LoaiTinNhan = entity.LoaiTinNhan,
                NoiDungGui = entity.NoiDungGui,
                TrangThai = entity.TrangThai,
                MessageIdZalo = entity.MessageIdZalo,
                ThoiGianGui = entity.ThoiGianGui
            };
        }

        private static string FormatPhoneNumberForZalo(string phone)
        {
            var cleaned = new string(phone.Where(char.IsDigit).ToArray());
            if (cleaned.StartsWith("0"))
            {
                return "84" + cleaned[1..];
            }
            if (!cleaned.StartsWith("84") && cleaned.Length >= 9)
            {
                return "84" + cleaned;
            }
            return cleaned;
        }
    }
}
