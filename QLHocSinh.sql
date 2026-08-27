/* =========================================================
   DATABASE: QUAN LY HOC SINH
   SQL SERVER
   ========================================================= */

-- Nếu database đã tồn tại thì xóa
IF DB_ID('QuanLyHocSinhDB') IS NOT NULL
BEGIN
    ALTER DATABASE QuanLyHocSinhDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE QuanLyHocSinhDB;
END
GO

-- Tạo database
CREATE DATABASE QuanLyHocSinhDB;
GO

USE QuanLyHocSinhDB;
GO


/* =========================================================
   1. BẢNG VAI TRÒ
   ========================================================= */

CREATE TABLE VAITRO
(
    MaVaiTro INT IDENTITY(1,1) PRIMARY KEY,
    TenVaiTro NVARCHAR(50) NOT NULL UNIQUE,
    MoTa NVARCHAR(255) NULL
);
GO


/* =========================================================
   2. BẢNG TÀI KHOẢN
   ========================================================= */

CREATE TABLE TAIKHOAN
(
    MaTaiKhoan INT IDENTITY(1,1) PRIMARY KEY,

    TenDangNhap VARCHAR(50) NOT NULL UNIQUE,

    MatKhau VARCHAR(255) NOT NULL,

    HoTen NVARCHAR(100) NOT NULL,

    Email VARCHAR(100) NULL UNIQUE,

    SoDienThoai VARCHAR(15) NULL,

    MaVaiTro INT NOT NULL,

    TrangThai BIT NOT NULL DEFAULT 1,

    NgayTao DATETIME NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_TAIKHOAN_VAITRO
        FOREIGN KEY (MaVaiTro)
        REFERENCES VAITRO(MaVaiTro)
);
GO


/* =========================================================
   3. BẢNG GIÁO VIÊN
   ========================================================= */

CREATE TABLE GIAOVIEN
(
    MaGiaoVien INT IDENTITY(1,1) PRIMARY KEY,

    MaTaiKhoan INT NULL,

    MaGiaoVienCode VARCHAR(20) NOT NULL UNIQUE,

    HoTen NVARCHAR(100) NOT NULL,

    NgaySinh DATE NULL,

    GioiTinh NVARCHAR(10) NULL,

    Email VARCHAR(100) NULL,

    SoDienThoai VARCHAR(15) NULL,

    DiaChi NVARCHAR(255) NULL,

    ChuyenMon NVARCHAR(100) NULL,

    TrangThai BIT NOT NULL DEFAULT 1,

    CONSTRAINT FK_GIAOVIEN_TAIKHOAN
        FOREIGN KEY (MaTaiKhoan)
        REFERENCES TAIKHOAN(MaTaiKhoan)
);
GO


/* =========================================================
   4. BẢNG NĂM HỌC
   ========================================================= */

CREATE TABLE NAMHOC
(
    MaNamHoc INT IDENTITY(1,1) PRIMARY KEY,

    TenNamHoc NVARCHAR(20) NOT NULL UNIQUE,

    NgayBatDau DATE NULL,

    NgayKetThuc DATE NULL,

    TrangThai BIT NOT NULL DEFAULT 1
);
GO


/* =========================================================
   5. BẢNG LỚP
   ========================================================= */

CREATE TABLE LOP
(
    MaLop INT IDENTITY(1,1) PRIMARY KEY,

    MaLopCode VARCHAR(20) NOT NULL UNIQUE,

    TenLop NVARCHAR(50) NOT NULL,

    Khoi INT NOT NULL,

    MaNamHoc INT NOT NULL,

    MaGiaoVienChuNhiem INT NULL,

    SiSo INT NOT NULL DEFAULT 0,

    TrangThai BIT NOT NULL DEFAULT 1,

    CONSTRAINT CK_LOP_KHOI
        CHECK (Khoi BETWEEN 1 AND 12),

    CONSTRAINT CK_LOP_SISO
        CHECK (SiSo >= 0),

    CONSTRAINT FK_LOP_NAMHOC
        FOREIGN KEY (MaNamHoc)
        REFERENCES NAMHOC(MaNamHoc),

    CONSTRAINT FK_LOP_GIAOVIEN
        FOREIGN KEY (MaGiaoVienChuNhiem)
        REFERENCES GIAOVIEN(MaGiaoVien)
);
GO


/* =========================================================
   6. BẢNG HỌC SINH
   ========================================================= */

CREATE TABLE HOCSINH
(
    MaHocSinh INT IDENTITY(1,1) PRIMARY KEY,

    MaHocSinhCode VARCHAR(20) NOT NULL UNIQUE,

    MaTaiKhoan INT NULL,

    HoTen NVARCHAR(100) NOT NULL,

    NgaySinh DATE NOT NULL,

    GioiTinh NVARCHAR(10) NOT NULL,

    DiaChi NVARCHAR(255) NULL,

    SoDienThoai VARCHAR(15) NULL,

    Email VARCHAR(100) NULL,

    MaLop INT NOT NULL,

    NgayNhapHoc DATE NULL,

    TrangThai BIT NOT NULL DEFAULT 1,

    CONSTRAINT FK_HOCSINH_TAIKHOAN
        FOREIGN KEY (MaTaiKhoan)
        REFERENCES TAIKHOAN(MaTaiKhoan),

    CONSTRAINT FK_HOCSINH_LOP
        FOREIGN KEY (MaLop)
        REFERENCES LOP(MaLop),

    CONSTRAINT CK_HOCSINH_GIOITINH
        CHECK (GioiTinh IN (N'Nam', N'Nữ', N'Khác'))
);
GO


/* =========================================================
   7. BẢNG MÔN HỌC
   ========================================================= */

CREATE TABLE MONHOC
(
    MaMonHoc INT IDENTITY(1,1) PRIMARY KEY,

    MaMonHocCode VARCHAR(20) NOT NULL UNIQUE,

    TenMonHoc NVARCHAR(100) NOT NULL UNIQUE,

    SoTiet INT NULL,

    MoTa NVARCHAR(255) NULL,

    TrangThai BIT NOT NULL DEFAULT 1,

    CONSTRAINT CK_MONHOC_SOTIET
        CHECK (SoTiet IS NULL OR SoTiet > 0)
);
GO


/* =========================================================
   8. BẢNG HỌC KỲ
   ========================================================= */

CREATE TABLE HOCKY
(
    MaHocKy INT IDENTITY(1,1) PRIMARY KEY,

    TenHocKy NVARCHAR(50) NOT NULL,

    SoHocKy INT NOT NULL,

    MaNamHoc INT NOT NULL,

    TrangThai BIT NOT NULL DEFAULT 1,

    CONSTRAINT CK_HOCKY_SO
        CHECK (SoHocKy IN (1,2,3)),

    CONSTRAINT FK_HOCKY_NAMHOC
        FOREIGN KEY (MaNamHoc)
        REFERENCES NAMHOC(MaNamHoc),

    CONSTRAINT UQ_HOCKY
        UNIQUE (SoHocKy, MaNamHoc)
);
GO


/* =========================================================
   9. BẢNG PHÂN CÔNG GIÁO VIÊN
   ========================================================= */

CREATE TABLE PHANCONG
(
    MaPhanCong INT IDENTITY(1,1) PRIMARY KEY,

    MaGiaoVien INT NOT NULL,

    MaLop INT NOT NULL,

    MaMonHoc INT NOT NULL,

    MaNamHoc INT NOT NULL,

    HocKy INT NOT NULL,

    NgayPhanCong DATE NOT NULL DEFAULT GETDATE(),

    TrangThai BIT NOT NULL DEFAULT 1,

    CONSTRAINT FK_PHANCONG_GIAOVIEN
        FOREIGN KEY (MaGiaoVien)
        REFERENCES GIAOVIEN(MaGiaoVien),

    CONSTRAINT FK_PHANCONG_LOP
        FOREIGN KEY (MaLop)
        REFERENCES LOP(MaLop),

    CONSTRAINT FK_PHANCONG_MONHOC
        FOREIGN KEY (MaMonHoc)
        REFERENCES MONHOC(MaMonHoc),

    CONSTRAINT FK_PHANCONG_NAMHOC
        FOREIGN KEY (MaNamHoc)
        REFERENCES NAMHOC(MaNamHoc),

    CONSTRAINT CK_PHANCONG_HOCKY
        CHECK (HocKy IN (1,2,3)),

    CONSTRAINT UQ_PHANCONG
        UNIQUE
        (
            MaGiaoVien,
            MaLop,
            MaMonHoc,
            MaNamHoc,
            HocKy
        )
);
GO


/* =========================================================
   10. BẢNG ĐIỂM
   ========================================================= */

CREATE TABLE DIEM
(
    MaDiem INT IDENTITY(1,1) PRIMARY KEY,

    MaHocSinh INT NOT NULL,

    MaMonHoc INT NOT NULL,

    MaHocKy INT NOT NULL,

    DiemMieng DECIMAL(4,2) NULL,

    Diem15Phut DECIMAL(4,2) NULL,

    DiemGiuaKy DECIMAL(4,2) NULL,

    DiemCuoiKy DECIMAL(4,2) NULL,

    DiemTrungBinh DECIMAL(4,2) NULL,

    GhiChu NVARCHAR(255) NULL,

    NgayCapNhat DATETIME NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_DIEM_HOCSINH
        FOREIGN KEY (MaHocSinh)
        REFERENCES HOCSINH(MaHocSinh),

    CONSTRAINT FK_DIEM_MONHOC
        FOREIGN KEY (MaMonHoc)
        REFERENCES MONHOC(MaMonHoc),

    CONSTRAINT FK_DIEM_HOCKY
        FOREIGN KEY (MaHocKy)
        REFERENCES HOCKY(MaHocKy),

    CONSTRAINT CK_DIEM_MIE
        CHECK (DiemMieng IS NULL OR (DiemMieng >= 0 AND DiemMieng <= 10)),

    CONSTRAINT CK_DIEM_15PHUT
        CHECK (Diem15Phut IS NULL OR (Diem15Phut >= 0 AND Diem15Phut <= 10)),

    CONSTRAINT CK_DIEM_GIUAKY
        CHECK (DiemGiuaKy IS NULL OR (DiemGiuaKy >= 0 AND DiemGiuaKy <= 10)),

    CONSTRAINT CK_DIEM_CUOIKY
        CHECK (DiemCuoiKy IS NULL OR (DiemCuoiKy >= 0 AND DiemCuoiKy <= 10)),

    CONSTRAINT CK_DIEM_TRUNGBINH
        CHECK (DiemTrungBinh IS NULL OR (DiemTrungBinh >= 0 AND DiemTrungBinh <= 10)),

    CONSTRAINT UQ_DIEM
        UNIQUE
        (
            MaHocSinh,
            MaMonHoc,
            MaHocKy
        )
);
GO


/* =========================================================
   11. BẢNG THÔNG BÁO
   ========================================================= */

CREATE TABLE THONGBAO
(
    MaThongBao INT IDENTITY(1,1) PRIMARY KEY,

    TieuDe NVARCHAR(200) NOT NULL,

    NoiDung NVARCHAR(MAX) NOT NULL,

    MaTaiKhoan INT NULL,

    NgayDang DATETIME NOT NULL DEFAULT GETDATE(),

    TrangThai BIT NOT NULL DEFAULT 1,

    CONSTRAINT FK_THONGBAO_TAIKHOAN
        FOREIGN KEY (MaTaiKhoan)
        REFERENCES TAIKHOAN(MaTaiKhoan)
);
GO


/* =========================================================
   INDEX
   ========================================================= */

CREATE INDEX IX_HOCSINH_HOTEN
ON HOCSINH(HoTen);
GO

CREATE INDEX IX_HOCSINH_MALOP
ON HOCSINH(MaLop);
GO

CREATE INDEX IX_GIAOVIEN_HOTEN
ON GIAOVIEN(HoTen);
GO

CREATE INDEX IX_DIEM_HOCSINH
ON DIEM(MaHocSinh);
GO

CREATE INDEX IX_DIEM_MONHOC
ON DIEM(MaMonHoc);
GO


/* =========================================================
   DỮ LIỆU MẪU
   ========================================================= */

-- Vai trò
INSERT INTO VAITRO (TenVaiTro, MoTa)
VALUES
(N'ADMIN', N'Quản trị viên hệ thống'),
(N'TEACHER', N'Giáo viên'),
(N'STUDENT', N'Học sinh');
GO


-- Tài khoản
-- Lưu ý: khi làm thực tế, MatKhau phải được hash bằng BCrypt/JWT
INSERT INTO TAIKHOAN
(
    TenDangNhap,
    MatKhau,
    HoTen,
    Email,
    SoDienThoai,
    MaVaiTro
)
VALUES
(
    'admin',
    '123456',
    N'Quản trị viên',
    'admin@gmail.com',
    '0900000001',
    1
),
(
    'giaovien01',
    '123456',
    N'Nguyễn Văn An',
    'an@gmail.com',
    '0900000002',
    2
),
(
    'giaovien02',
    '123456',
    N'Trần Thị Bình',
    'binh@gmail.com',
    '0900000003',
    2
),
(
    'hocsinh01',
    '123456',
    N'Nguyễn Văn Minh',
    'minh@gmail.com',
    '0900000004',
    3
);
GO


-- Giáo viên
INSERT INTO GIAOVIEN
(
    MaTaiKhoan,
    MaGiaoVienCode,
    HoTen,
    NgaySinh,
    GioiTinh,
    Email,
    SoDienThoai,
    DiaChi,
    ChuyenMon
)
VALUES
(
    2,
    'GV001',
    N'Nguyễn Văn An',
    '1985-05-10',
    N'Nam',
    'an@gmail.com',
    '0900000002',
    N'Hà Nội',
    N'Toán học'
),
(
    3,
    'GV002',
    N'Trần Thị Bình',
    '1988-09-20',
    N'Nữ',
    'binh@gmail.com',
    '0900000003',
    N'Hà Nội',
    N'Ngữ văn'
);
GO


-- Năm học
INSERT INTO NAMHOC
(
    TenNamHoc,
    NgayBatDau,
    NgayKetThuc
)
VALUES
(
    N'2026-2027',
    '2026-09-01',
    '2027-05-31'
);
GO


-- Lớp
INSERT INTO LOP
(
    MaLopCode,
    TenLop,
    Khoi,
    MaNamHoc,
    MaGiaoVienChuNhiem
)
VALUES
(
    '10A1',
    N'10A1',
    10,
    1,
    1
),
(
    '10A2',
    N'10A2',
    10,
    1,
    2
),
(
    '11A1',
    N'11A1',
    11,
    1,
    1
);
GO


-- Môn học
INSERT INTO MONHOC
(
    MaMonHocCode,
    TenMonHoc,
    SoTiet
)
VALUES
('TOAN', N'Toán', 90),
('VAN', N'Ngữ văn', 90),
('ANH', N'Tiếng Anh', 60),
('LY', N'Vật lý', 60),
('HOA', N'Hóa học', 60),
('SINH', N'Sinh học', 60),
('TIN', N'Tin học', 45);
GO


-- Học kỳ
INSERT INTO HOCKY
(
    TenHocKy,
    SoHocKy,
    MaNamHoc
)
VALUES
(
    N'Học kỳ 1',
    1,
    1
),
(
    N'Học kỳ 2',
    2,
    1
);
GO


/* =========================================================
   CẬP NHẬT SĨ SỐ LỚP
   ========================================================= */

UPDATE LOP
SET SiSo =
(
    SELECT COUNT(*)
    FROM HOCSINH HS
    WHERE HS.MaLop = LOP.MaLop
);
GO


/* =========================================================
   KIỂM TRA DATABASE
   ========================================================= */

SELECT * FROM VAITRO;
SELECT * FROM TAIKHOAN;
SELECT * FROM GIAOVIEN;
SELECT * FROM NAMHOC;
SELECT * FROM LOP;
SELECT * FROM HOCSINH;
SELECT * FROM MONHOC;
SELECT * FROM HOCKY;
SELECT * FROM PHANCONG;
SELECT * FROM DIEM;
SELECT * FROM THONGBAO;
GO