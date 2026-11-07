# Kế Hoạch Kiểm Thử Toàn Diện (Comprehensive Test Plan)
## Hệ Thống Quản Lý Học Sinh & Đào Tạo (QLHOCSINH)

---

## 🎯 Mục Tiêu Kiểm Thử
Xác minh tính đúng đắn, an toàn và hiệu năng của toàn bộ các tính năng từ Giai đoạn 1 đến Giai đoạn 4 theo tài liệu Đặc tả Kỹ thuật và Lộ trình phát triển:
1. Ràng buộc bảo mật & xác thực người dùng.
2. Quy trình kiểm duyệt tài khoản (Account Approval Flow).
3. Ràng buộc phân công chuyên môn giáo viên (Teacher Assignment Constraint).
4. Tính toán điểm số chuẩn Bộ GD&ĐT.
5. Xuất báo cáo bảng điểm Excel (.xlsx).
6. Tích hợp gửi thông báo Zalo ZNS cho phụ huynh học sinh.
7. Hiển thị bảng điều khiển Visual Dashboard & biểu đồ phân tích.

---

## 🧪 Kịch Bản Kiểm Thử Chi Tiết

### Test Case 1: Khởi Động & Health Check Hệ Thống
- **Thực hiện**:
  1. Chạy `dotnet run --project "QLPHONGHOC\QLPHONGHOC.csproj"`.
  2. Gửi request `GET http://localhost:5266/api/health`.
  3. Gửi request `GET http://localhost:5266/api/health/db`.
- **Kết quả mong đợi**:
  - API Health trả về mã `200 OK` với trạng thái `UP`.
  - Database Health trả về `canConnect: true` và số lượng bản ghi của các bảng dữ liệu.

---

### Test Case 2: Quy Trình Phê Duyệt Tài Khoản (Giai Đoạn 2)
- **Thực hiện**:
  1. Đăng ký tài khoản mới tại `/Account/Register` (ví dụ: `giaovien_moi` / `123456`).
  2. Đăng nhập ngay với `giaovien_moi` / `123456`.
  3. Đăng nhập tài khoản `admin` / `123456` và điều hướng tới trang `/Account/ManageUsers`.
  4. Xác nhận thấy tài khoản `giaovien_moi` trong danh sách với trạng thái **Chờ duyệt** (Inactive).
  5. Nhấn nút **Kích hoạt / Duyệt**.
  6. Đăng xuất `admin` và đăng nhập lại bằng `giaovien_moi`.
- **Kết quả mong đợi**:
  - Ở bước 2: Đăng nhập thất bại với thông báo: *"Tài khoản chưa được kích hoạt hoặc đang bị khóa"*.
  - Ở bước 6: Đăng nhập thành công và vào được hệ thống.

---

### Test Case 3: Ràng Buộc Phân Công Giảng Dạy & Tính ĐTB (Giai Đoạn 2)
- **Thực hiện**:
  1. Đăng nhập với tài khoản giáo viên môn Toán `gv_toan` / `123456`.
  2. Truy cập `/Diem` và chọn lớp mà giáo viên **được phân công** (ví dụ lớp 10A1, môn Toán).
  3. Nhập điểm: Miệng = 9, 15P = 8, Giữa kỳ = 8.5, Cuối kỳ = 9.0 -> Lưu điểm.
  4. Thử cố ý can thiệp hoặc chọn lớp/môn học mà giáo viên **không được phân công** (ví dụ môn Văn hoặc lớp 12A1) qua API:
     ```http
     POST /api/diem/upsert
     Content-Type: application/json
     {
       "MaHocSinh": 1,
       "MaMonHoc": 2,  // Môn không được phân công
       "MaHocKy": 1,
       "DiemMieng": 8.0,
       "DiemCuoiKy": 7.0
     }
     ```
- **Kết quả mong đợi**:
  - Bước 3: ĐTB tính đúng theo công thức:
    $$\frac{9 \times 1 + 8 \times 1 + 8.5 \times 2 + 9 \times 3}{7} = \frac{9 + 8 + 17 + 27}{7} = \frac{61}{7} \approx 8.71$$
    Xếp loại đạt **Giỏi**.
  - Bước 4: Hệ thống chặn lại và trả về mã lỗi **403 Forbidden** kèm thông báo: *"Giáo viên không được phân công giảng dạy môn học này cho lớp tương ứng."*

---

### Test Case 4: Xuất Bảng Điểm Lớp Ra File Excel (Giai Đoạn 3)
- **Thực hiện**:
  1. Đăng nhập với quyền `admin` hoặc `gv_toan`.
  2. Truy cập trang `/Diem`.
  3. Nhấn nút **"Xuất Bảng Điểm Excel (.xlsx)"** hoặc gửi request:
     `GET /api/diem/export-excel?maLop=1&maHocKy=1`.
  4. Mở file `.xlsx` tải về bằng Microsoft Excel hoặc công cụ xem bảng tính.
- **Kết quả mong đợi**:
  - File tải về thành công với tên dạng `BangDiem_Lop_10A1_HK1.xlsx`.
  - Tiêu đề bảng trang trọng với nền màu xanh Navy (`#1B365D`), chữ trắng.
  - Các cột bao gồm: STT, Mã HS, Họ và Tên, Điểm từng môn học, Điểm TB Chung, Xếp Loại (tô màu theo thang điểm), Tên Phụ Huynh, Số Điện Thoại Phụ Huynh.

---

### Test Case 5: Gửi Thông Báo Điểm Số Phụ Huynh Qua Zalo ZNS (Giai Đoạn 3)
- **Thực hiện**:
  1. Trong trang danh sách điểm `/Diem`, tìm một học sinh đã có số điện thoại phụ huynh.
  2. Nhấn nút **"Gửi Zalo"** hoặc gọi API:
     ```http
     POST /api/zalo/send-bang-diem
     Content-Type: application/json
     {
       "MaHocSinh": 1,
       "MaHocKy": 1
     }
     ```
  3. Kiểm tra nhật ký gửi tin nhắn tại:
     `GET /api/zalo/history?maHocSinh=1`.
- **Kết quả mong đợi**:
  - API phản hồi `200 OK` với thông báo gửi tin nhắn thành công.
  - Trong bảng `LICHSUZALO`, bản ghi mới được tạo với trạng thái `Success`, số điện thoại nhận đã được chuẩn hóa sang định dạng `84xxxxxxxxx`.

---

### Test Case 6: Kiểm Tra Dashboard & Biểu Đồ Analytics (Giai Đoạn 4)
- **Thực hiện**:
  1. Đăng nhập với tài khoản `admin` / `123456`.
  2. Truy cập trang chủ `/Home/Index`.
  3. Kiểm tra hiển thị của:
     - 4 thẻ KPI số liệu (Tổng học sinh, Tổng giáo viên, Tổng số lớp, Môn học).
     - Banner cảnh báo tài khoản chờ duyệt (nếu có tài khoản chưa kích hoạt).
     - Biểu đồ tròn Donut "Phân loại học lực toàn trường".
     - Biểu đồ cột "Sĩ số học sinh theo khối".
     - Bảng 5 bản ghi cập nhật điểm mới nhất.
  4. Kiểm tra dữ liệu JSON từ API:
     - `GET /api/home/dashboard-summary`
     - `GET /api/home/dashboard-charts`
- **Kết quả mong đợi**:
  - Cả hai biểu đồ Chart.js hiển thị mượt mà, responsive, không có lỗi JavaScript trên trình duyệt Console.
  - Số liệu phản ánh chính xác từ cơ sở dữ liệu.

---

## 📋 Bảng Tổng Kết Kết Quả Kiểm Thử (Checklist)

| STT | Hạng mục kiểm thử | Trạng thái | Ghi chú |
|---|---|:---:|---|
| 1 | Biên dịch dự án `dotnet build` | ✅ ĐẠT | 0 Warning(s), 0 Error(s) |
| 2 | Loại bỏ hoàn toàn mã nguồn thừa cũ | ✅ ĐẠT | Không còn phụ thuộc module phòng học |
| 3 | Nâng cấp bảo mật JWT lên v8.0.1 | ✅ ĐẠT | Khắc phục hoàn toàn CVE GHSA-59j7-ghrg-fj52 |
| 4 | Luồng phê duyệt tài khoản người dùng | ✅ ĐẠT | Giao diện quản lý Bootstrap 5 trực quan |
| 5 | Ràng buộc phân công chuyên môn giáo viên | ✅ ĐẠT | Chặn truy cập trái phép với HTTP 403 |
| 6 | Công thức tính ĐTB chuẩn Bộ GD&ĐT | ✅ ĐẠT | Trọng số: Miệng×1, 15P×1, GK×2, CK×3 |
| 7 | Xuất Excel bảng điểm ClosedXML | ✅ ĐẠT | Định dạng chuyên nghiệp, tự động căn cột |
| 8 | Tích hợp Zalo ZNS báo điểm phụ huynh | ✅ ĐẠT | Lưu vết đầy đủ vào bảng LICHSUZALO |
| 9 | Dashboard số liệu & Biểu đồ Chart.js | ✅ ĐẠT | Donut chart, Bar chart & Activity feed |
