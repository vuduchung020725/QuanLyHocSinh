# Hệ Thống Quản Lý Học Sinh & Đào Tạo (QLHOCSINH)

Hệ thống quản lý học sinh và đào tạo trường học hiện đại, bảo mật và toàn diện được xây dựng trên nền tảng **ASP.NET Core MVC (.NET 10)** kết hợp **Entity Framework Core**, **SQL Server**, **ClosedXML**, **Chart.js** và tích hợp thông báo phụ huynh qua **Zalo ZNS**.

---

## 🌟 Tính Năng Nổi Bật Theo Lộ Trình 4 Giai Đoạn

### 1. Dọn Dẹp & Tối Ưu Hệ Thống (Giai Đoạn 1)
- Loại bỏ hoàn toàn các mã nguồn thừa của hệ thống phòng học cũ (`PhongHoc`, `ThietBi`, `BaoTri`, `SuCo`...).
- Nâng cấp bảo mật thư viện `System.IdentityModel.Tokens.Jwt` lên phiên bản **8.0.1** (loại bỏ hoàn toàn lỗ hổng CVE).
- Xử lý triệt để các cảnh báo Nullable types, đảm bảo biên dịch đạt chuẩn `0 Warning(s), 0 Error(s)`.

### 2. Phê Duyệt Tài Khoản & Ràng Buộc Nghiệp Vụ Giảng Dạy (Giai Đoạn 2)
- **Quy trình phê duyệt tài khoản người dùng (`/Account/ManageUsers`)**: Tài khoản mới đăng ký ở trạng thái chờ duyệt (`TrangThai = false`), chỉ Quản trị viên (ADMIN) mới có quyền duyệt kích hoạt, khóa hoặc phân quyền.
- **Ràng buộc phân công chuyên môn (`PHANCONG`)**: Giáo viên chỉ được phép nhập/chỉnh sửa điểm cho lớp và môn học mà mình được Ban giám hiệu phân công trong học kỳ. Nếu không được phân công, hệ thống chặn với mã phản hồi `403 Forbidden`.
- **Chuẩn hóa công thức ĐTB môn theo Bộ GD&ĐT**:
  $$\text{ĐTB môn} = \frac{\text{Điểm miệng} \times 1 + \text{Điểm 15P} \times 1 + \text{Điểm giữa kỳ} \times 2 + \text{Điểm cuối kỳ} \times 3}{7}$$

### 3. Xuất Báo Cáo Excel & Gửi Thông Báo Phụ Huynh Qua Zalo ZNS (Giai Đoạn 3)
- **Xuất bảng điểm lớp ra Excel chuẩn ClosedXML (`.xlsx`)**: Định dạng chuẩn Enterprise với tiêu đề xanh Navy (#1B365D), đầy đủ điểm các môn học, điểm trung bình chung, xếp loại học lực tô màu trực quan và số điện thoại phụ huynh.
- **Tích hợp Zalo ZNS Notification Service**: Tự động chuẩn hóa số điện thoại quốc tế (+84 / 84xxx), gửi bảng điểm học kỳ và thông báo vắng học/sự kiện đến phụ huynh học sinh; lưu nhật ký chi tiết vào bảng `LICHSUZALO`.
- Hỗ trợ chế độ **Simulation Mode** an toàn khi chưa cấu hình Access Token thực tế.

### 4. Bảng Điều Khiển Tổng Quan (Visual Dashboard & Analytics) (Giai Đoạn 4)
- **4 Thẻ KPI Real-time**: Thống kê số lượng học sinh đang học, giáo viên đang công tác, lớp học và môn học đào tạo.
- **Biểu đồ Donut (Chart.js)**: Phân bố xếp loại học lực toàn trường (Giỏi $\ge 8.5$, Khá $7.0 - 8.4$, Trung bình $5.0 - 6.9$, Yếu $< 5.0$).
- **Biểu đồ Cột (Chart.js)**: Sĩ số học sinh theo khối (Khối 10, Khối 11, Khối 12).
- **Cảnh báo phê duyệt tài khoản**: Banner thông báo tự động hiển thị khi có tài khoản mới đang chờ quản trị viên duyệt.
- **Hoạt động cập nhật điểm gần nhất**: Bảng lịch sử 5 bản ghi điểm số được giáo viên cập nhật mới nhất.

---

## 🛠️ Tech Stack & Kiến Trúc

- **Backend**: ASP.NET Core 10.0 MVC, C# 13, EF Core 10.0
- **Database**: Microsoft SQL Server (`QuanLyHocSinhDB`)
- **Authentication**: Cookie Authentication kết hợp JSON Web Token (JWT)
- **Reporting & Office**: ClosedXML 0.104.2
- **Frontend & Visualization**: Bootstrap 5, Modern CSS Variables (Navy `#1B365D`, Teal `#008080`, Background `#F8FAFC`), Chart.js 4.4.0 (CDN)

---

## 🚀 Hướng Dẫn Cài Đặt & Chạy Ứng Dụng

### 1. Chuẩn Bị Cơ Sở Dữ Liệu
Mở SQL Server Management Studio (SSMS) hoặc Azure Data Studio và chạy file kịch bản SQL có sẵn trong dự án:
```powershell
init_database_qlhocsinh.sql
```
File này sẽ tự động tạo cơ sở dữ liệu `QuanLyHocSinhDB`, các bảng quan hệ (`TAIKHOAN`, `HOCSINH`, `GIAOVIEN`, `LOP`, `MONHOC`, `HOCKY`, `PHANCONG`, `DIEM`, `LICHSUZALO`) và dữ liệu mẫu đầy đủ.

### 2. Cấu Hình Chuỗi Kết Nối & Zalo ZNS
Mở file `QLPHONGHOC/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.\\SQLEXPRESS;Database=QuanLyHocSinhDB;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "JwtSettings": {
    "SecretKey": "QLHocSinh-SecretKey-MustBe-AtLeast-32-Characters-Long-For-Security!",
    "Issuer": "QLHocSinhAPI",
    "Audience": "QLHocSinhClients",
    "ExpiryMinutes": 120
  },
  "ZaloSettings": {
    "AccessToken": "",
    "TemplateId": "345678",
    "SenderPhone": ""
  }
}
```
*(Nếu chưa có tài khoản Zalo ZNS chính thức, hệ thống sẽ tự động chuyển sang chế độ giả lập an toàn và ghi log vào bảng `LICHSUZALO`)*.

### 3. Biên Dịch & Chạy Ứng Dụng
```powershell
# Di chuyển tới thư mục gốc hoặc chỉ định file dự án
dotnet restore "QLPHONGHOC/QLPHONGHOC.csproj"
dotnet build "QLPHONGHOC/QLPHONGHOC.csproj"
dotnet run --project "QLPHONGHOC/QLPHONGHOC.csproj"
```

Truy cập hệ thống tại: `http://localhost:5266`

---

## 👥 Tài Khoản Mặc Định Thử Nghiệm

| Tên đăng nhập | Mật khẩu | Vai trò | Quyền hạn chính |
|---|---|---|---|
| `admin` | `123456` | **ADMIN** | Toàn quyền hệ thống, phê duyệt tài khoản, phân công giảng dạy, xem dashboard |
| `gv_toan` / `teacher` | `123456` | **TEACHER** | Nhập/sửa điểm các lớp được phân công, xuất Excel, gửi Zalo cho phụ huynh |
| `hs_nam` / `student` | `123456` | **STUDENT** | Tra cứu điểm số cá nhân, thời khóa biểu và kết quả rèn luyện |

---

## 📡 Danh Sách API Quan Trọng

- `GET /api/home/dashboard-summary`: Lấy 4 chỉ số thống kê tổng quan (Học sinh, Giáo viên, Lớp, Môn).
- `GET /api/home/dashboard-charts`: Lấy dữ liệu phân loại học lực, sĩ số theo khối, số tài khoản chờ duyệt và hoạt động gần nhất.
- `POST /api/diem/upsert` hoặc `POST /api/diem/update-grade`: Cập nhật điểm thành phần (kiểm tra phân công giảng dạy).
- `GET /api/diem/export-excel?maLop={id}&maHocKy={id}`: Tải file Excel bảng điểm lớp học.
- `POST /api/zalo/send-bang-diem`: Gửi tin nhắn ZNS báo điểm cho phụ huynh học sinh.
- `GET /api/zalo/history?maHocSinh={id}`: Lấy lịch sử gửi tin Zalo của học sinh.
- `GET /api/health`: Kiểm tra trạng thái hoạt động của hệ thống.
