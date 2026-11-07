-- ============================================================
-- KỊCH BẢN T-SQL KHỞI TẠO CƠ SỞ DỮ LIỆU
-- Dự án: Hệ thống Quản lý Học sinh (QLHOCSINH)
-- SCHEMA ĐỒNG BỘ 100% VỚI C# MODELS (EF Core DbContext)
-- Ngày cập nhật: 2026-08-27
-- ============================================================

-- 1. Tạo Database (đồng bộ với appsettings.json)
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'QuanLyHocSinhDB')
BEGIN
    CREATE DATABASE QuanLyHocSinhDB;
END
GO

USE QuanLyHocSinhDB;
GO

-- 2. Xóa các bảng cũ theo thứ tự đảo ngược (tránh lỗi FK)
IF OBJECT_ID('dbo.LICHSUZALO', 'U') IS NOT NULL DROP TABLE dbo.LICHSUZALO;
IF OBJECT_ID('dbo.THONGBAO',  'U') IS NOT NULL DROP TABLE dbo.THONGBAO;
IF OBJECT_ID('dbo.DIEM',      'U') IS NOT NULL DROP TABLE dbo.DIEM;
IF OBJECT_ID('dbo.PHANCONG',  'U') IS NOT NULL DROP TABLE dbo.PHANCONG;
IF OBJECT_ID('dbo.HOCSINH',   'U') IS NOT NULL DROP TABLE dbo.HOCSINH;
IF OBJECT_ID('dbo.LOP',       'U') IS NOT NULL DROP TABLE dbo.LOP;
IF OBJECT_ID('dbo.GIAOVIEN',  'U') IS NOT NULL DROP TABLE dbo.GIAOVIEN;
IF OBJECT_ID('dbo.MONHOC',    'U') IS NOT NULL DROP TABLE dbo.MONHOC;
IF OBJECT_ID('dbo.HOCKY',     'U') IS NOT NULL DROP TABLE dbo.HOCKY;
IF OBJECT_ID('dbo.NAMHOC',    'U') IS NOT NULL DROP TABLE dbo.NAMHOC;
IF OBJECT_ID('dbo.TAIKHOAN',  'U') IS NOT NULL DROP TABLE dbo.TAIKHOAN;
IF OBJECT_ID('dbo.VAITRO',    'U') IS NOT NULL DROP TABLE dbo.VAITRO;
GO

-- ============================================================
-- 3. BẢNG VAITRO (Model: VaiTro.cs)
--    Note: Seed role name = "ADMIN" "TEACHER" "STUDENT" (đồng bộ với Authorize Roles)
-- ============================================================
CREATE TABLE VAITRO (
    MaVaiTro   INT           IDENTITY(1,1) PRIMARY KEY,
    TenVaiTro  NVARCHAR(50)  NOT NULL UNIQUE,
    MoTa       NVARCHAR(255) NULL
);
GO

-- ============================================================
-- 4. BẢNG TAIKHOAN (Model: TaiKhoan.cs)
--    TrangThai = BIT (bool), có NgayTao
-- ============================================================
CREATE TABLE TAIKHOAN (
    MaTaiKhoan  INT          IDENTITY(1,1) PRIMARY KEY,
    TenDangNhap NVARCHAR(50) NOT NULL UNIQUE,
    MatKhau     VARCHAR(255) NOT NULL,
    HoTen       NVARCHAR(100) NOT NULL,
    Email       NVARCHAR(100) NOT NULL UNIQUE,
    SoDienThoai NVARCHAR(15)  NULL,
    MaVaiTro    INT          NOT NULL,
    TrangThai   BIT          NOT NULL DEFAULT 1,
    NgayTao     DATETIME     NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_TAIKHOAN_VAITRO FOREIGN KEY (MaVaiTro) REFERENCES VAITRO(MaVaiTro)
);
GO

-- ============================================================
-- 5. BẢNG NAMHOC (Model: NamHoc.cs)
--    Có NgayBatDau, NgayKetThuc DATE NULL; TrangThai BIT
-- ============================================================
CREATE TABLE NAMHOC (
    MaNamHoc    INT          IDENTITY(1,1) PRIMARY KEY,
    TenNamHoc   NVARCHAR(20) NOT NULL UNIQUE,
    NgayBatDau  DATE         NULL,
    NgayKetThuc DATE         NULL,
    TrangThai   BIT          NOT NULL DEFAULT 1
);
GO

-- ============================================================
-- 6. BẢNG HOCKY (Model: HocKy.cs)
--    [NotMapped] TuNgay/DenNgay => KHÔNG tạo cột
--    Có SoHocKy INT, TrangThai BIT
-- ============================================================
CREATE TABLE HOCKY (
    MaHocKy  INT          IDENTITY(1,1) PRIMARY KEY,
    TenHocKy NVARCHAR(30) NOT NULL,
    MaNamHoc INT          NOT NULL,
    SoHocKy  INT          NOT NULL DEFAULT 1,
    TrangThai BIT         NOT NULL DEFAULT 1,
    CONSTRAINT FK_HOCKY_NAMHOC FOREIGN KEY (MaNamHoc) REFERENCES NAMHOC(MaNamHoc)
);
GO

-- ============================================================
-- 7. BẢNG GIAOVIEN (Model: GiaoVien.cs)
--    MaGV => Column "MaGiaoVienCode"; TrangThai BIT; NgaySinh DATE NULL
-- ============================================================
CREATE TABLE GIAOVIEN (
    MaGiaoVien    INT           IDENTITY(1,1) PRIMARY KEY,
    MaTaiKhoan    INT           NULL,
    MaGiaoVienCode NVARCHAR(20) NOT NULL UNIQUE,
    HoTen         NVARCHAR(100) NOT NULL,
    NgaySinh      DATE          NULL,
    GioiTinh      NVARCHAR(10)  NULL,
    SoDienThoai   NVARCHAR(15)  NULL,
    Email         NVARCHAR(100) NULL,
    DiaChi        NVARCHAR(255) NULL,
    ChuyenMon     NVARCHAR(100) NOT NULL,
    TrangThai     BIT           NOT NULL DEFAULT 1,
    CONSTRAINT FK_GIAOVIEN_TAIKHOAN FOREIGN KEY (MaTaiKhoan) REFERENCES TAIKHOAN(MaTaiKhoan)
);
GO

-- ============================================================
-- 8. BẢNG LOP (Model: Lop.cs)
--    Khoi INT (không phải NVARCHAR); MaGVCN => "MaGiaoVienChuNhiem"; TrangThai BIT
-- ============================================================
CREATE TABLE LOP (
    MaLop              INT          IDENTITY(1,1) PRIMARY KEY,
    MaLopCode          NVARCHAR(20) NOT NULL UNIQUE,
    TenLop             NVARCHAR(50) NOT NULL,
    Khoi               INT          NOT NULL,
    MaNamHoc           INT          NOT NULL,
    MaGiaoVienChuNhiem INT          NULL,
    SiSo               INT          NOT NULL DEFAULT 0,
    TrangThai          BIT          NOT NULL DEFAULT 1,
    CONSTRAINT FK_LOP_NAMHOC  FOREIGN KEY (MaNamHoc)           REFERENCES NAMHOC(MaNamHoc),
    CONSTRAINT FK_LOP_GVCN    FOREIGN KEY (MaGiaoVienChuNhiem) REFERENCES GIAOVIEN(MaGiaoVien)
);
GO

-- ============================================================
-- 9. BẢNG HOCSINH (Model: HocSinh.cs)
--    MaHS => Column "MaHocSinhCode"; NgaySinh DATE NOT NULL; NgayNhapHoc DATE NULL
--    TrangThai BIT; GioiTinh NVARCHAR(10) NOT NULL
-- ============================================================
CREATE TABLE HOCSINH (
    MaHocSinh     INT           IDENTITY(1,1) PRIMARY KEY,
    MaTaiKhoan    INT           NULL,
    MaHocSinhCode NVARCHAR(20)  NOT NULL UNIQUE,
    HoTen         NVARCHAR(100) NOT NULL,
    NgaySinh      DATE          NOT NULL,
    GioiTinh      NVARCHAR(10)  NOT NULL,
    DiaChi        NVARCHAR(255) NULL,
    SoDienThoai   NVARCHAR(15)  NULL,
    Email         NVARCHAR(100) NULL,
    TenPhuHuynh   NVARCHAR(100) NULL,
    SdtPhuHuynh   VARCHAR(15)   NULL,
    MaLop         INT           NOT NULL,
    NgayNhapHoc   DATE          NULL,
    TrangThai     BIT           NOT NULL DEFAULT 1,
    CONSTRAINT FK_HOCSINH_TAIKHOAN FOREIGN KEY (MaTaiKhoan) REFERENCES TAIKHOAN(MaTaiKhoan),
    CONSTRAINT FK_HOCSINH_LOP      FOREIGN KEY (MaLop)      REFERENCES LOP(MaLop)
);
GO

-- ============================================================
-- 10. BẢNG MONHOC (Model: MonHoc.cs)
--     MaMon => "MaMonHocCode"; TenMon => "TenMonHoc"; GhiChu => "MoTa"; SoTiet INT NULL
-- ============================================================
CREATE TABLE MONHOC (
    MaMonHoc     INT          IDENTITY(1,1) PRIMARY KEY,
    MaMonHocCode NVARCHAR(20) NOT NULL UNIQUE,
    TenMonHoc    NVARCHAR(100) NOT NULL,
    SoTiet       INT          NULL DEFAULT 0,
    MoTa         NVARCHAR(255) NULL,
    TrangThai    BIT          NOT NULL DEFAULT 1
);
GO

-- ============================================================
-- 11. BẢNG PHANCONG (Model: PhanCong.cs)
--     KHÔNG có MaHocKy FK; có MaNamHoc FK + HocKy INT (thuần túy)
--     Có NgayPhanCong DATE, TrangThai BIT
-- ============================================================
CREATE TABLE PHANCONG (
    MaPhanCong  INT IDENTITY(1,1) PRIMARY KEY,
    MaGiaoVien  INT NOT NULL,
    MaMonHoc    INT NOT NULL,
    MaLop       INT NOT NULL,
    MaNamHoc    INT NOT NULL,
    HocKy       INT NOT NULL DEFAULT 1,
    NgayPhanCong DATE NOT NULL DEFAULT GETDATE(),
    TrangThai   BIT NOT NULL DEFAULT 1,
    CONSTRAINT FK_PHANCONG_GV     FOREIGN KEY (MaGiaoVien) REFERENCES GIAOVIEN(MaGiaoVien),
    CONSTRAINT FK_PHANCONG_MON    FOREIGN KEY (MaMonHoc)   REFERENCES MONHOC(MaMonHoc),
    CONSTRAINT FK_PHANCONG_LOP    FOREIGN KEY (MaLop)      REFERENCES LOP(MaLop),
    CONSTRAINT FK_PHANCONG_NAMHOC FOREIGN KEY (MaNamHoc)   REFERENCES NAMHOC(MaNamHoc),
    CONSTRAINT UQ_PHANCONG UNIQUE (MaGiaoVien, MaMonHoc, MaLop, MaNamHoc, HocKy)
);
GO

-- ============================================================
-- 12. BẢNG DIEM (Model: Diem.cs)
--     KHÔNG computed column DTB/XepLoai; DiemTrungBinh là cột thường (nullable)
--     Không có MaGiaoVienNhap; Có GhiChu NVARCHAR(255) NULL; NgayCapNhat DATETIME
-- ============================================================
CREATE TABLE DIEM (
    MaDiem        INT           IDENTITY(1,1) PRIMARY KEY,
    MaHocSinh     INT           NOT NULL,
    MaMonHoc      INT           NOT NULL,
    MaHocKy       INT           NOT NULL,
    DiemMieng     DECIMAL(4,2)  NULL,
    Diem15Phut    DECIMAL(4,2)  NULL,
    DiemGiuaKy    DECIMAL(4,2)  NULL,
    DiemCuoiKy    DECIMAL(4,2)  NULL,
    DiemTrungBinh DECIMAL(4,2)  NULL,
    GhiChu        NVARCHAR(255) NULL,
    NgayCapNhat   DATETIME      NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_DIEM_HOCSINH FOREIGN KEY (MaHocSinh) REFERENCES HOCSINH(MaHocSinh),
    CONSTRAINT FK_DIEM_MONHOC  FOREIGN KEY (MaMonHoc)  REFERENCES MONHOC(MaMonHoc),
    CONSTRAINT FK_DIEM_HOCKY   FOREIGN KEY (MaHocKy)   REFERENCES HOCKY(MaHocKy),
    CONSTRAINT UQ_DIEM UNIQUE (MaHocSinh, MaMonHoc, MaHocKy)
);
GO

-- ============================================================
-- 13. BẢNG THONGBAO (Model: ThongBao.cs) - THIẾU, BỔ SUNG
-- ============================================================
CREATE TABLE THONGBAO (
    MaThongBao  INT           IDENTITY(1,1) PRIMARY KEY,
    TieuDe      NVARCHAR(200) NOT NULL,
    NoiDung     NVARCHAR(MAX) NOT NULL,
    MaTaiKhoan  INT           NULL,
    NgayDang    DATETIME      NOT NULL DEFAULT GETDATE(),
    TrangThai   BIT           NOT NULL DEFAULT 1,
    CONSTRAINT FK_THONGBAO_TAIKHOAN FOREIGN KEY (MaTaiKhoan) REFERENCES TAIKHOAN(MaTaiKhoan) ON DELETE SET NULL
);
GO

-- ============================================================
-- 14. BẢNG LICHSUZALO (Model: LichSuZalo.cs) - Quản lý gửi ZNS Phụ huynh
-- ============================================================
CREATE TABLE LICHSUZALO (
    MaLichSuZalo    INT           IDENTITY(1,1) PRIMARY KEY,
    MaHocSinh       INT           NOT NULL,
    SoDienThoaiNhan VARCHAR(15)   NOT NULL,
    LoaiTinNhan     NVARCHAR(50)  NOT NULL,
    NoiDungGui      NVARCHAR(MAX) NOT NULL,
    TrangThai       NVARCHAR(50)  NOT NULL DEFAULT N'Thành công',
    MessageIdZalo   VARCHAR(100)  NULL,
    ThoiGianGui     DATETIME      NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_LICHSUZALO_HOCSINH FOREIGN KEY (MaHocSinh) REFERENCES HOCSINH(MaHocSinh) ON DELETE CASCADE
);
GO

-- ============================================================
-- 14. INDEX tối ưu hiệu năng (đồng bộ với cột mới)
-- ============================================================
CREATE NONCLUSTERED INDEX IX_TAIKHOAN_TenDangNhap   ON TAIKHOAN(TenDangNhap);
CREATE NONCLUSTERED INDEX IX_TAIKHOAN_Email          ON TAIKHOAN(Email);
CREATE NONCLUSTERED INDEX IX_HOCSINH_MaLop           ON HOCSINH(MaLop);
CREATE NONCLUSTERED INDEX IX_HOCSINH_TrangThai        ON HOCSINH(TrangThai);
CREATE NONCLUSTERED INDEX IX_HOCSINH_MaHocSinhCode    ON HOCSINH(MaHocSinhCode);
CREATE NONCLUSTERED INDEX IX_DIEM_HocSinh_HocKy      ON DIEM(MaHocSinh, MaHocKy);
CREATE NONCLUSTERED INDEX IX_DIEM_MonHoc_HocKy       ON DIEM(MaMonHoc, MaHocKy);
CREATE NONCLUSTERED INDEX IX_LOP_MaNamHoc_Khoi       ON LOP(MaNamHoc, Khoi);
CREATE NONCLUSTERED INDEX IX_PHANCONG_GV_HocKy       ON PHANCONG(MaGiaoVien, HocKy);
CREATE NONCLUSTERED INDEX IX_GIAOVIEN_MaGiaoVienCode  ON GIAOVIEN(MaGiaoVienCode);
CREATE NONCLUSTERED INDEX IX_MONHOC_MaMonHocCode      ON MONHOC(MaMonHocCode);
-- MaTaiKhoan là 1-1 optional: tạo index thường (App layer đảm bảo unique khi NOT NULL qua EF HasOne/WithOne)
CREATE NONCLUSTERED INDEX IX_GIAOVIEN_MaTaiKhoan      ON GIAOVIEN(MaTaiKhoan);
CREATE NONCLUSTERED INDEX IX_HOCSINH_MaTaiKhoan       ON HOCSINH(MaTaiKhoan);
GO

-- ============================================================
-- 15. SEED DATA (đồng bộ tên cột, kiểu, và role names ADMIN/TEACHER/STUDENT)
-- ============================================================

-- Seed Vai Trò (ĐỒNG BỘ với Program.cs DbSeeder: ADMIN, TEACHER, STUDENT)
SET IDENTITY_INSERT VAITRO ON;
INSERT INTO VAITRO (MaVaiTro, TenVaiTro, MoTa) VALUES (1, N'ADMIN',    N'Quản trị hệ thống');
INSERT INTO VAITRO (MaVaiTro, TenVaiTro, MoTa) VALUES (2, N'TEACHER',  N'Giáo viên');
INSERT INTO VAITRO (MaVaiTro, TenVaiTro, MoTa) VALUES (3, N'STUDENT',  N'Học sinh');
SET IDENTITY_INSERT VAITRO OFF;
GO

-- Seed Tài Khoản (mật khẩu đều là: 123456)
-- Lưu ý: MatKhau ghi dạng plaintext "123456" – ứng dụng startup sẽ tự động hash bằng BCrypt (DbSeeder self-healing)
-- TrangThai = 1 (Hoạt động) / 0 (Khóa)
SET IDENTITY_INSERT TAIKHOAN ON;
INSERT INTO TAIKHOAN (MaTaiKhoan, TenDangNhap, MatKhau, HoTen, Email, SoDienThoai, MaVaiTro, TrangThai, NgayTao, LockedEnabled, AccessFailedCount) VALUES
(1, N'admin',    N'123456', N'Quản Trị Viên',  N'admin@school.edu.vn',        '0987654321', 1, 1, GETDATE(), 0, 0),
(2, N'gv_toan',  N'123456', N'Nguyễn Văn An',  N'nvan@school.edu.vn',         '0987654322', 2, 1, GETDATE(), 0, 0),
(3, N'gv_van',   N'123456', N'Trần Thị Bình',  N'ttbinh@school.edu.vn',       '0987654323', 2, 1, GETDATE(), 0, 0),
(4, N'gv_ly',    N'123456', N'Lê Minh Cường',  N'lmcuong@school.edu.vn',      '0987654324', 2, 1, GETDATE(), 0, 0),
(5, N'hs_001',   N'123456', N'Phạm Thị Dung',  N'ptdung@student.edu.vn',      '0912345001', 3, 1, GETDATE(), 0, 0),
(6, N'hs_002',   N'123456', N'Hoàng Văn Em',   N'hvem@student.edu.vn',        '0912345002', 3, 1, GETDATE(), 0, 0),
(7, N'hs_003',   N'123456', N'Vũ Thị Phương',  N'vtphuong@student.edu.vn',    '0912345003', 3, 1, GETDATE(), 0, 0),
(8, N'teacher',  N'123456', N'Giáo Viên Mẫu',  N'teacher@qlhocsinh.edu.vn',   '0912002001', 2, 1, GETDATE(), 0, 0),
(9, N'student',  N'123456', N'Học Sinh Mẫu',   N'student@qlhocsinh.edu.vn',   '0912345999', 3, 1, GETDATE(), 0, 0);
SET IDENTITY_INSERT TAIKHOAN OFF;
GO

-- Seed Năm Học (TrangThai BIT: 1=Năm hiện tại, 0=cũ/sắp tới)
SET IDENTITY_INSERT NAMHOC ON;
INSERT INTO NAMHOC (MaNamHoc, TenNamHoc, NgayBatDau, NgayKetThuc, TrangThai) VALUES
(1, N'2023-2024', '2023-09-05', '2024-05-31', 0),
(2, N'2024-2025', '2024-09-05', '2025-05-31', 1),
(3, N'2025-2026', '2025-09-01', '2026-05-30', 0);
SET IDENTITY_INSERT NAMHOC OFF;
GO

-- Seed Học Kỳ (cho năm học 2024-2025, MaNamHoc = 2)
SET IDENTITY_INSERT HOCKY ON;
INSERT INTO HOCKY (MaHocKy, TenHocKy, MaNamHoc, SoHocKy, TrangThai) VALUES
(1, N'Học kỳ 1', 2, 1, 1),
(2, N'Học kỳ 2', 2, 2, 1);
SET IDENTITY_INSERT HOCKY OFF;
GO

-- Seed Giáo Viên (MaTaiKhoan: gv_toan=2, gv_van=3, gv_ly=4)
SET IDENTITY_INSERT GIAOVIEN ON;
INSERT INTO GIAOVIEN (MaGiaoVien, MaTaiKhoan, MaGiaoVienCode, HoTen, NgaySinh, GioiTinh, SoDienThoai, Email, DiaChi, ChuyenMon, TrangThai) VALUES
(1, 2,    N'GV001', N'Nguyễn Văn An',  '1985-03-15', N'Nam', '0987654322', N'nvan@school.edu.vn',    N'12 Lê Lợi, Hà Nội',       N'Toán học',  1),
(2, 3,    N'GV002', N'Trần Thị Bình',  '1990-07-22', N'Nữ',  '0987654323', N'ttbinh@school.edu.vn',  N'45 Nguyễn Huệ, Hà Nội',   N'Ngữ văn',   1),
(3, 4,    N'GV003', N'Lê Minh Cường',  '1988-11-10', N'Nam', '0987654324', N'lmcuong@school.edu.vn', N'78 Trần Phú, Hà Nội',     N'Vật lý',    1),
(4, 8,    N'GV004', N'Phạm Thị Dung',  '1992-05-03', N'Nữ',  '0912001001', N'teacher@qlhocsinh.edu.vn', N'23 Đinh Tiên Hoàng, HN', N'Hóa học',  1),
(5, NULL, N'GV005', N'Hoàng Văn Giang','1987-09-18', N'Nam', '0912001002', N'hvgiang@school.edu.vn', N'56 Bà Triệu, Hà Nội',     N'Tiếng Anh', 1);
SET IDENTITY_INSERT GIAOVIEN OFF;
GO

-- Seed Lớp (MaNamHoc=2 = 2024-2025, MaGiaoVienChuNhiem từ bảng GIAOVIEN)
-- Khoi INT: 10, 11, 12
SET IDENTITY_INSERT LOP ON;
INSERT INTO LOP (MaLop, MaLopCode, TenLop, Khoi, MaNamHoc, MaGiaoVienChuNhiem, SiSo, TrangThai) VALUES
(1, N'10A1', N'Lớp 10A1', 10, 2, 1, 35, 1),
(2, N'10A2', N'Lớp 10A2', 10, 2, 2, 33, 1),
(3, N'11B1', N'Lớp 11B1', 11, 2, 3, 36, 1),
(4, N'11B2', N'Lớp 11B2', 11, 2, 4, 34, 1),
(5, N'12C1', N'Lớp 12C1', 12, 2, 5, 38, 1);
SET IDENTITY_INSERT LOP OFF;
GO

-- Seed Học Sinh (MaTaiKhoan: hs_001=5, hs_002=6, hs_003=7; MaLop: 1-5)
SET IDENTITY_INSERT HOCSINH ON;
INSERT INTO HOCSINH (MaHocSinh, MaTaiKhoan, MaHocSinhCode, HoTen, NgaySinh, GioiTinh, DiaChi, SoDienThoai, Email, TenPhuHuynh, SdtPhuHuynh, MaLop, NgayNhapHoc, TrangThai) VALUES
(1, 5,    N'HS001', N'Phạm Thị Dung',    '2009-04-12', N'Nữ',  N'12 Lý Thường Kiệt, HN', '0912345001', N'ptdung@student.edu.vn',    N'Phạm Văn Hùng', '0981110001', 1, '2024-09-05', 1),
(2, 6,    N'HS002', N'Hoàng Văn Em',     '2009-08-25', N'Nam', N'34 Hai Bà Trưng, HN',   '0912345002', N'hvem@student.edu.vn',      N'Hoàng Thị Lan', '0981110002', 1, '2024-09-05', 1),
(3, 7,    N'HS003', N'Vũ Thị Phương',    '2009-01-30', N'Nữ',  N'56 Ngô Quyền, HN',      '0912345003', N'vtphuong@student.edu.vn',  N'Vũ Đức Thịnh',  '0981110003', 1, '2024-09-05', 1),
(4, 9,    N'HS004', N'Đinh Quang Hải',   '2009-06-14', N'Nam', N'78 Tràng Thi, HN',      '0912345004', N'student@qlhocsinh.edu.vn', N'Đinh Văn Long', '0981110004', 1, '2024-09-05', 1),
(5, NULL, N'HS005', N'Ngô Thị Lan',      '2009-09-05', N'Nữ',  N'90 Hàng Bông, HN',      '0912345005', NULL,                        N'Ngô Đức An',    '0981110005', 2, '2024-09-05', 1),
(6, NULL, N'HS006', N'Bùi Văn Minh',     '2008-03-18', N'Nam', N'23 Hàng Gai, HN',       '0912345006', NULL,                        N'Bùi Quang Huy', '0981110006', 3, '2023-09-01', 1),
(7, NULL, N'HS007', N'Đỗ Thị Nga',       '2008-11-22', N'Nữ',  N'45 Hàng Đào, HN',       '0912345007', NULL,                        N'Đỗ Văn Thành',  '0981110007', 3, '2023-09-01', 1),
(8, NULL, N'HS008', N'Lý Văn Oanh',      '2008-07-08', N'Nam', N'67 Hàng Ngang, HN',     '0912345008', NULL,                        N'Lý Quang Minh', '0981110008', 4, '2023-09-01', 1),
(9, NULL, N'HS009', N'Mai Thị Phượng',   '2007-05-15', N'Nữ',  N'89 Đinh Lễ, HN',        '0912345009', NULL,                        N'Mai Văn Tuấn',  '0981110009', 5, '2022-08-25', 1),
(10,NULL, N'HS010', N'Trịnh Văn Quân',   '2007-12-20', N'Nam', N'11 Lê Thái Tổ, HN',     '0912345010', NULL,                        N'Trịnh Bá Sơn',  '0981110010', 5, '2022-08-25', 1);
SET IDENTITY_INSERT HOCSINH OFF;
GO

-- Seed Môn Học (SoTiet INT NULL)
SET IDENTITY_INSERT MONHOC ON;
INSERT INTO MONHOC (MaMonHoc, MaMonHocCode, TenMonHoc, SoTiet, MoTa, TrangThai) VALUES
(1,  N'TOAN', N'Toán học',       4, N'Toán đại số và hình học', 1),
(2,  N'VAN',  N'Ngữ văn',        4, N'Văn học và tiếng Việt',   1),
(3,  N'ANH',  N'Tiếng Anh',      3, N'Ngoại ngữ 1',             1),
(4,  N'LY',   N'Vật lý',         3, NULL,                       1),
(5,  N'HOA',  N'Hóa học',        3, NULL,                       1),
(6,  N'SINH', N'Sinh học',       2, NULL,                       1),
(7,  N'SU',   N'Lịch sử',        2, NULL,                       1),
(8,  N'DIA',  N'Địa lý',         2, NULL,                       1),
(9,  N'GDCD', N'Giáo dục công dân', 1, NULL,                    1),
(10, N'THE',  N'Thể dục',        2, NULL,                       1);
SET IDENTITY_INSERT MONHOC OFF;
GO

-- Seed Phân Công (GV dạy môn, lớp, MaNamHoc, HocKy INT, MaNamHoc FK tới NAMHOC)
-- MaGiaoVien: GV001=1(Toan), GV002=2(Van), GV003=3(Ly), GV004=4(Hoa), GV005=5(Anh)
-- MaMonHoc:   TOAN=1, VAN=2, ANH=3, LY=4, HOA=5
-- MaLop:      10A1=1, 10A2=2, 11B1=3, 11B2=4, 12C1=5
-- MaNamHoc=2 (2024-2025); HocKy=1 (HK1), 2 (HK2)
SET IDENTITY_INSERT PHANCONG ON;
INSERT INTO PHANCONG (MaPhanCong, MaGiaoVien, MaMonHoc, MaLop, MaNamHoc, HocKy, NgayPhanCong, TrangThai) VALUES
(1,  1, 1, 1, 2, 1, '2024-08-20', 1), -- GV001 dạy Toán, 10A1, HK1
(2,  1, 1, 2, 2, 1, '2024-08-20', 1), -- GV001 dạy Toán, 10A2, HK1
(3,  1, 1, 1, 2, 2, '2024-08-20', 1), -- GV001 dạy Toán, 10A1, HK2
(4,  1, 1, 2, 2, 2, '2024-08-20', 1), -- GV001 dạy Toán, 10A2, HK2
(5,  2, 2, 1, 2, 1, '2024-08-20', 1), -- GV002 dạy Văn, 10A1, HK1
(6,  2, 2, 2, 2, 1, '2024-08-20', 1), -- GV002 dạy Văn, 10A2, HK1
(7,  3, 4, 3, 2, 1, '2024-08-20', 1), -- GV003 dạy Lý,  11B1, HK1
(8,  3, 4, 4, 2, 1, '2024-08-20', 1), -- GV003 dạy Lý,  11B2, HK1
(9,  4, 5, 3, 2, 1, '2024-08-20', 1), -- GV004 dạy Hóa, 11B1, HK1
(10, 5, 3, 1, 2, 1, '2024-08-20', 1), -- GV005 dạy Anh, 10A1, HK1
(11, 5, 3, 2, 2, 1, '2024-08-20', 1); -- GV005 dạy Anh, 10A2, HK1
SET IDENTITY_INSERT PHANCONG OFF;
GO

-- Seed Điểm (học kỳ 1, MaHocKy=1, không có MaGiaoVienNhap)
-- Tính DTB = (Miệng×1 + 15phút×1 + GiữaKỳ×2 + CuốiKỳ×3) / 7, lưu vào cột DiemTrungBinh
SET IDENTITY_INSERT DIEM ON;
INSERT INTO DIEM (MaDiem, MaHocSinh, MaMonHoc, MaHocKy, DiemMieng, Diem15Phut, DiemGiuaKy, DiemCuoiKy, DiemTrungBinh, GhiChu, NgayCapNhat) VALUES
-- HS001-HS004 ở lớp 10A1, môn Toán (MaMonHoc=1)
(1,  1, 1, 1, 8.5, 7.0, 7.5, 8.0, CAST((8.5*1 + 7.0*1 + 7.5*2 + 8.0*3)/7.0 AS DECIMAL(4,2)), N'Điểm đầu năm HK1', GETDATE()),
(2,  2, 1, 1, 6.0, 7.5, 6.5, 7.0, CAST((6.0*1 + 7.5*1 + 6.5*2 + 7.0*3)/7.0 AS DECIMAL(4,2)), N'Điểm đầu năm HK1', GETDATE()),
(3,  3, 1, 1, 9.0, 8.5, 9.0, 9.5, CAST((9.0*1 + 8.5*1 + 9.0*2 + 9.5*3)/7.0 AS DECIMAL(4,2)), N'Học sinh giỏi',     GETDATE()),
(4,  4, 1, 1, 5.0, 5.5, 5.0, 4.5, CAST((5.0*1 + 5.5*1 + 5.0*2 + 4.5*3)/7.0 AS DECIMAL(4,2)), N'Cần cố gắng thêm', GETDATE()),
-- Môn Văn (MaMonHoc=2)
(5,  1, 2, 1, 8.0, 7.5, 8.0, 8.5, CAST((8.0*1 + 7.5*1 + 8.0*2 + 8.5*3)/7.0 AS DECIMAL(4,2)), NULL, GETDATE()),
(6,  2, 2, 1, 7.0, 6.5, 7.0, 7.5, CAST((7.0*1 + 6.5*1 + 7.0*2 + 7.5*3)/7.0 AS DECIMAL(4,2)), NULL, GETDATE()),
(7,  3, 2, 1, 9.0, 9.0, 8.5, 9.0, CAST((9.0*1 + 9.0*1 + 8.5*2 + 9.0*3)/7.0 AS DECIMAL(4,2)), N'Học sinh giỏi văn', GETDATE()),
(8,  4, 2, 1, 5.5, 5.0, 5.5, 5.0, CAST((5.5*1 + 5.0*1 + 5.5*2 + 5.0*3)/7.0 AS DECIMAL(4,2)), NULL, GETDATE()),
-- Môn Anh (MaMonHoc=3)
(9,  1, 3, 1, 7.5, 8.0, 7.0, 8.0, CAST((7.5*1 + 8.0*1 + 7.0*2 + 8.0*3)/7.0 AS DECIMAL(4,2)), NULL, GETDATE()),
(10, 2, 3, 1, 9.0, 8.5, 9.0, 9.0, CAST((9.0*1 + 8.5*1 + 9.0*2 + 9.0*3)/7.0 AS DECIMAL(4,2)), N'Giỏi ngoại ngữ', GETDATE()),
(11, 3, 3, 1, 6.0, 7.0, 6.5, 7.0, CAST((6.0*1 + 7.0*1 + 6.5*2 + 7.0*3)/7.0 AS DECIMAL(4,2)), NULL, GETDATE()),
(12, 4, 3, 1, 4.5, 5.0, 4.5, 5.0, CAST((4.5*1 + 5.0*1 + 4.5*2 + 5.0*3)/7.0 AS DECIMAL(4,2)), N'Cần học thêm', GETDATE()),
-- HS trong lớp 11B1 (HS006=MaHS 6, HS007=MaHS 7), môn Lý (MaMonHoc=4)
(13, 6, 4, 1, 8.0, 7.5, 8.0, 8.5, CAST((8.0*1 + 7.5*1 + 8.0*2 + 8.5*3)/7.0 AS DECIMAL(4,2)), NULL, GETDATE()),
(14, 7, 4, 1, 6.5, 7.0, 6.5, 7.0, CAST((6.5*1 + 7.0*1 + 6.5*2 + 7.0*3)/7.0 AS DECIMAL(4,2)), NULL, GETDATE());
SET IDENTITY_INSERT DIEM OFF;
GO

PRINT N'================================================================';
PRINT N'✅ Database QuanLyHocSinhDB da duoc khoi tao THANH CONG!';
PRINT N'================================================================';
PRINT N'   - 3 vai tro (dong bo code): ADMIN, TEACHER, STUDENT';
PRINT N'   - 7 tai khoan (mat khau tat ca: admin123)';
PRINT N'   - 5 giao vien, 5 lop (khoi INT 10/11/12), 10 mon hoc, 10 hoc sinh';
PRINT N'   - 11 phan cong, 14 ban ghi diem (tinh san DiemTrungBinh)';
PRINT N'   - Tat ca TrangThai = BIT (0/1), ten cot 100% dong bo Models';
PRINT N'================================================================';
GO
