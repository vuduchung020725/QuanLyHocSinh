using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using QLPhongHoc.Data;

namespace QLPhongHoc.Services
{
    public interface IExcelExportService
    {
        Task<byte[]> ExportBangDiemLopAsync(int maLop, int maHocKy);
    }

    public class ExcelExportService : IExcelExportService
    {
        private readonly QLPhongHocContext _context;

        public ExcelExportService(QLPhongHocContext context)
        {
            _context = context;
        }

        public async Task<byte[]> ExportBangDiemLopAsync(int maLop, int maHocKy)
        {
            var lop = await _context.LOP
                .Include(l => l.GiaoVienChuNhiem)
                .FirstOrDefaultAsync(l => l.MaLop == maLop);

            if (lop == null)
                throw new ArgumentException("Không tìm thấy thông tin lớp học.");

            var hocKy = await _context.HOCKY
                .Include(h => h.NamHoc)
                .FirstOrDefaultAsync(h => h.MaHocKy == maHocKy);

            if (hocKy == null)
                throw new ArgumentException("Không tìm thấy thông tin học kỳ.");

            var hocSinhs = await _context.HOCSINH
                .Where(h => h.MaLop == maLop && h.TrangThai)
                .OrderBy(h => h.HoTen)
                .ToListAsync();

            var monHocs = await _context.MONHOC
                .Where(m => m.TrangThai)
                .OrderBy(m => m.MaMonHoc)
                .ToListAsync();

            var studentIds = hocSinhs.Select(h => h.MaHocSinh).ToList();
            var diems = await _context.DIEM
                .Where(d => d.MaHocKy == maHocKy && studentIds.Contains(d.MaHocSinh))
                .ToListAsync();

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("Bảng Điểm");

            // Màu chủ đạo: Navy #1B365D
            var navyColor = XLColor.FromArgb(27, 54, 93);

            // Tiêu đề
            int totalCols = 4 + monHocs.Count + 3; // STT, MaHS, HoTen, NgaySinh + MonHocs + DTB Chung, XepLoai, SdtPhuHuynh
            ws.Range(1, 1, 1, totalCols).Merge();
            var titleCell = ws.Cell(1, 1);
            titleCell.Value = $"BẢNG ĐIỂM TỔNG KẾT — LỚP {lop.TenLop.ToUpper()}";
            titleCell.Style.Font.Bold = true;
            titleCell.Style.Font.FontSize = 16;
            titleCell.Style.Font.FontColor = navyColor;
            titleCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            ws.Range(2, 1, 2, totalCols).Merge();
            var subCell = ws.Cell(2, 1);
            var namHocStr = hocKy.NamHoc != null ? hocKy.NamHoc.TenNamHoc : "Năm học 2024 - 2025";
            subCell.Value = $"{hocKy.TenHocKy} ({namHocStr}) | GVCN: {(lop.GiaoVienChuNhiem != null ? lop.GiaoVienChuNhiem.HoTen : "Chưa phân công")} | Ngày xuất: {DateTime.Now:dd/MM/yyyy HH:mm}";
            subCell.Style.Font.Italic = true;
            subCell.Style.Font.FontSize = 11;
            subCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            // Headers hàng 4
            int headerRow = 4;
            int col = 1;

            ws.Cell(headerRow, col++).Value = "STT";
            ws.Cell(headerRow, col++).Value = "Mã HS";
            ws.Cell(headerRow, col++).Value = "Họ và tên";
            ws.Cell(headerRow, col++).Value = "Ngày sinh";

            foreach (var mon in monHocs)
            {
                ws.Cell(headerRow, col++).Value = mon.TenMon;
            }

            ws.Cell(headerRow, col++).Value = "ĐTB Chung";
            ws.Cell(headerRow, col++).Value = "Xếp loại";
            ws.Cell(headerRow, col++).Value = "SĐT Phụ huynh";

            // Định dạng Header
            var headerRange = ws.Range(headerRow, 1, headerRow, totalCols);
            headerRange.Style.Fill.BackgroundColor = navyColor;
            headerRange.Style.Font.FontColor = XLColor.White;
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            ws.Row(headerRow).Height = 28;

            // Đổ dữ liệu học sinh
            int currentRow = headerRow + 1;
            int stt = 1;

            foreach (var hs in hocSinhs)
            {
                col = 1;
                ws.Cell(currentRow, col++).Value = stt++;
                ws.Cell(currentRow, col++).Value = hs.MaHS;
                ws.Cell(currentRow, col++).Value = hs.HoTen;
                ws.Cell(currentRow, col++).Value = hs.NgaySinh.ToString("dd/MM/yyyy");

                decimal totalDtbMon = 0m;
                int countMonCoDiem = 0;

                foreach (var mon in monHocs)
                {
                    var diem = diems.FirstOrDefault(d => d.MaHocSinh == hs.MaHocSinh && d.MaMonHoc == mon.MaMonHoc);
                    var cell = ws.Cell(currentRow, col++);
                    if (diem?.DiemTrungBinh != null)
                    {
                        cell.Value = diem.DiemTrungBinh.Value;
                        cell.Style.NumberFormat.Format = "0.00";
                        totalDtbMon += diem.DiemTrungBinh.Value;
                        countMonCoDiem++;
                    }
                    else
                    {
                        cell.Value = "-";
                        cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    }
                }

                // ĐTB Chung
                var dtbChungCell = ws.Cell(currentRow, col++);
                var xepLoaiCell = ws.Cell(currentRow, col++);

                if (countMonCoDiem > 0)
                {
                    decimal dtbChung = Math.Round(totalDtbMon / countMonCoDiem, 2);
                    dtbChungCell.Value = dtbChung;
                    dtbChungCell.Style.NumberFormat.Format = "0.00";
                    dtbChungCell.Style.Font.Bold = true;

                    string xepLoai = dtbChung switch
                    {
                        >= 8.5m => "Giỏi",
                        >= 7.0m => "Khá",
                        >= 5.0m => "Trung bình",
                        >= 3.5m => "Yếu",
                        _ => "Kém"
                    };
                    xepLoaiCell.Value = xepLoai;
                    xepLoaiCell.Style.Font.Bold = true;
                    xepLoaiCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    if (xepLoai == "Giỏi")
                    {
                        xepLoaiCell.Style.Font.FontColor = XLColor.FromArgb(16, 124, 65);
                    }
                    else if (xepLoai == "Yếu" || xepLoai == "Kém")
                    {
                        xepLoaiCell.Style.Font.FontColor = XLColor.FromArgb(197, 34, 31);
                    }
                }
                else
                {
                    dtbChungCell.Value = "-";
                    dtbChungCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    xepLoaiCell.Value = "Chưa xếp loại";
                    xepLoaiCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                }

                // SĐT Phụ huynh
                var sdtCell = ws.Cell(currentRow, col++);
                sdtCell.Value = hs.SdtPhuHuynh ?? "-";
                sdtCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                // Kẻ border cho từng hàng
                var dataRowRange = ws.Range(currentRow, 1, currentRow, totalCols);
                dataRowRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                dataRowRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                dataRowRange.Style.Border.OutsideBorderColor = XLColor.FromArgb(220, 220, 220);
                dataRowRange.Style.Border.InsideBorderColor = XLColor.FromArgb(235, 235, 235);

                if (currentRow % 2 == 0)
                {
                    dataRowRange.Style.Fill.BackgroundColor = XLColor.FromArgb(248, 250, 252);
                }

                currentRow++;
            }

            // Thiết lập độ rộng cột chuẩn thay vì AdjustToContents (tránh lock SixLabors font measurement)
            ws.Column(1).Width = 8;   // STT
            ws.Column(2).Width = 14;  // Mã HS
            ws.Column(3).Width = 26;  // Họ và tên
            ws.Column(4).Width = 14;  // Ngày sinh
            for (int c = 5; c <= 4 + monHocs.Count; c++)
            {
                ws.Column(c).Width = 12; // Môn học
            }
            ws.Column(5 + monHocs.Count).Width = 14; // ĐTB Chung
            ws.Column(6 + monHocs.Count).Width = 16; // Xếp loại
            ws.Column(7 + monHocs.Count).Width = 18; // SĐT Phụ huynh

            using var ms = new MemoryStream();
            workbook.SaveAs(ms);
            return ms.ToArray();
        }
    }
}

