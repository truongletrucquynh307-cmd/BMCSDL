-- ============================================================
-- FILE 2: QLDH DATABASE + SECURITY (Chạy với user QLDH)
-- ============================================================

SET SERVEROUTPUT ON;
ALTER SESSION SET NLS_DATE_FORMAT = 'DD-MM-YYYY';

-- ============================================================
-- PHẦN 1: XÓA BẢNG CŨ (nếu có)
-- ============================================================
BEGIN EXECUTE IMMEDIATE 'DROP TABLE HOADON CASCADE CONSTRAINTS'; EXCEPTION WHEN OTHERS THEN NULL; END;
/
BEGIN EXECUTE IMMEDIATE 'DROP TABLE CHITIETDONHANG CASCADE CONSTRAINTS'; EXCEPTION WHEN OTHERS THEN NULL; END;
/
BEGIN EXECUTE IMMEDIATE 'DROP TABLE DONHANG CASCADE CONSTRAINTS'; EXCEPTION WHEN OTHERS THEN NULL; END;
/
BEGIN EXECUTE IMMEDIATE 'DROP TABLE SANPHAM CASCADE CONSTRAINTS'; EXCEPTION WHEN OTHERS THEN NULL; END;
/
BEGIN EXECUTE IMMEDIATE 'DROP TABLE LOAISP CASCADE CONSTRAINTS'; EXCEPTION WHEN OTHERS THEN NULL; END;
/
BEGIN EXECUTE IMMEDIATE 'DROP TABLE NCC CASCADE CONSTRAINTS'; EXCEPTION WHEN OTHERS THEN NULL; END;
/
BEGIN EXECUTE IMMEDIATE 'DROP TABLE KHACHHANG CASCADE CONSTRAINTS'; EXCEPTION WHEN OTHERS THEN NULL; END;
/
BEGIN EXECUTE IMMEDIATE 'DROP TABLE LOAIKH CASCADE CONSTRAINTS'; EXCEPTION WHEN OTHERS THEN NULL; END;
/
BEGIN EXECUTE IMMEDIATE 'DROP TABLE TAIKHOAN CASCADE CONSTRAINTS'; EXCEPTION WHEN OTHERS THEN NULL; END;
/
BEGIN EXECUTE IMMEDIATE 'DROP TABLE NHANVIEN CASCADE CONSTRAINTS'; EXCEPTION WHEN OTHERS THEN NULL; END;
/
BEGIN EXECUTE IMMEDIATE 'DROP TABLE AUDIT_LOG CASCADE CONSTRAINTS'; EXCEPTION WHEN OTHERS THEN NULL; END;
/
BEGIN EXECUTE IMMEDIATE 'DROP TABLE CRYPTO_KEYS CASCADE CONSTRAINTS'; EXCEPTION WHEN OTHERS THEN NULL; END;
/

-- ============================================================
-- PHẦN 2: TẠO CÁC BẢNG
-- ============================================================

-- Bảng LOẠI KHÁCH HÀNG
CREATE TABLE LOAIKH (
    MALOAIKH   VARCHAR2(5) PRIMARY KEY,
    TENLOAIKH  NVARCHAR2(50)
);

-- Bảng KHÁCH HÀNG (TENKH, DIACHI, SDT sẽ được mã hóa ở C#)
CREATE TABLE KHACHHANG (
    MAKH    VARCHAR2(5) PRIMARY KEY,
    TENKH   NVARCHAR2(500),    -- Tăng size để chứa encrypted data
    DIACHI  NVARCHAR2(1000),   -- Tăng size để chứa encrypted data
    SDT     VARCHAR2(500),     -- Tăng size để chứa encrypted data
    EMAIL   VARCHAR2(100),
    MALOAIKH VARCHAR2(5),
    CONSTRAINT FK_KH_LOAIKH FOREIGN KEY (MALOAIKH) REFERENCES LOAIKH(MALOAIKH)
);

-- Bảng NHÂN VIÊN (DIACHI, SDT sẽ được mã hóa ở Oracle)
CREATE TABLE NHANVIEN (
    MANV    VARCHAR2(5) PRIMARY KEY,
    TENNV   NVARCHAR2(100),
    CHUCVU  NVARCHAR2(50),
    DIACHI  NVARCHAR2(500),    -- Tăng size để chứa encrypted data
    SDT     VARCHAR2(500),     -- Tăng size để chứa encrypted data
    EMAIL   VARCHAR2(100)
);

-- Bảng LOẠI SẢN PHẨM
CREATE TABLE LOAISP (
    MALOAISP VARCHAR2(6) PRIMARY KEY,
    TENLOAI  NVARCHAR2(50)
);

-- Bảng NHÀ CUNG CẤP
CREATE TABLE NCC (
    MANCC   VARCHAR2(6) PRIMARY KEY,
    TENNCC  NVARCHAR2(100),
    DIACHI  NVARCHAR2(200),
    SDT     VARCHAR2(15),
    EMAIL   VARCHAR2(100)
);

-- Bảng SẢN PHẨM
CREATE TABLE SANPHAM (
    MASP        VARCHAR2(5) PRIMARY KEY,
    TENSP       NVARCHAR2(100),
    GIANHAP     NUMBER,
    GIABAN      NUMBER,
    SOLUONGTON  INT,
    GHICHU      NVARCHAR2(200),
    NGAYTAO     DATE,
    MANCC       VARCHAR2(6),
    MALOAISP    VARCHAR2(6),
    CONSTRAINT FK_SP_NCC FOREIGN KEY (MANCC) REFERENCES NCC(MANCC),
    CONSTRAINT FK_SP_LOAISP FOREIGN KEY (MALOAISP) REFERENCES LOAISP(MALOAISP)
);

-- Bảng ĐƠN HÀNG
CREATE TABLE DONHANG (
    MADH      VARCHAR2(5) PRIMARY KEY,
    NGAYLAP   DATE,
    NGAYHT    DATE,
    TRANGTHAI NVARCHAR2(50),
    MAKH      VARCHAR2(5),
    MANV      VARCHAR2(5),
    CONSTRAINT FK_DH_KH FOREIGN KEY (MAKH) REFERENCES KHACHHANG(MAKH),
    CONSTRAINT FK_DH_NV FOREIGN KEY (MANV) REFERENCES NHANVIEN(MANV)
);

-- Bảng CHI TIẾT ĐƠN HÀNG
CREATE TABLE CHITIETDONHANG (
    MADH     VARCHAR2(5),
    MASP     VARCHAR2(5),
    SOLUONG  INT,
    DONGIA   NUMBER,
    CONSTRAINT PK_CTDH PRIMARY KEY (MADH, MASP),
    CONSTRAINT FK_CTDH_DH FOREIGN KEY (MADH) REFERENCES DONHANG(MADH),
    CONSTRAINT FK_CTDH_SP FOREIGN KEY (MASP) REFERENCES SANPHAM(MASP)
);

-- Bảng HÓA ĐƠN
CREATE TABLE HOADON (
    MAHD            VARCHAR2(5) PRIMARY KEY,
    MADH            VARCHAR2(5),
    NGAYLAP         DATE,
    PHUONGTHUCTT    NVARCHAR2(50),
    TONGTIEN        NUMBER,
    CONSTRAINT FK_HD_DH FOREIGN KEY (MADH) REFERENCES DONHANG(MADH)
);

-- Bảng TÀI KHOẢN (MATKHAU mã hóa AES-GCM ở C#)
CREATE TABLE TAIKHOAN (
    MANV    VARCHAR2(5) PRIMARY KEY,
    MATKHAU VARCHAR2(256),
    VAITRO  NVARCHAR2(50),
    CONSTRAINT FK_TK_NV FOREIGN KEY (MANV) REFERENCES NHANVIEN(MANV)
);

-- Bảng CRYPTO_KEYS - Lưu RSA key pairs
CREATE TABLE CRYPTO_KEYS (
    KEY_ID          NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    KEY_NAME        VARCHAR2(50) UNIQUE NOT NULL,
    PUBLIC_KEY      CLOB NOT NULL,
    PRIVATE_KEY     CLOB NOT NULL,
    KEY_TYPE        VARCHAR2(20) DEFAULT 'RSA-2048',
    CREATED_DATE    TIMESTAMP DEFAULT SYSTIMESTAMP,
    IS_ACTIVE       CHAR(1) DEFAULT 'Y'
);

-- Bảng AUDIT_LOG - Ghi lịch sử thay đổi
CREATE TABLE AUDIT_LOG (
    LOG_ID        NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    MANV          VARCHAR2(10),
    TABLE_NAME    VARCHAR2(50),
    ACTION_TYPE   VARCHAR2(10),
    RECORD_ID     VARCHAR2(50),
    OLD_VALUE     CLOB,
    NEW_VALUE     CLOB,
    ACTION_TIME   TIMESTAMP DEFAULT SYSTIMESTAMP,
    IP_ADDRESS    VARCHAR2(50),
    DESCRIPTION   NVARCHAR2(500)
);

-- ============================================================
-- PHẦN 3: DỮ LIỆU MẪU
-- ============================================================

-- LOAIKH
INSERT INTO LOAIKH VALUES ('LK001', N'Khách lẻ');
INSERT INTO LOAIKH VALUES ('LK002', N'Khách sỉ');

-- KHACHHANG (dữ liệu chưa mã hóa - sẽ mã hóa khi thêm mới qua app)
INSERT INTO KHACHHANG VALUES ('KH001', N'Nguyễn Văn A', N'123 Lê Lợi', '0901111111', 'a@gmail.com', 'LK001');
INSERT INTO KHACHHANG VALUES ('KH002', N'Trần Thị B', N'456 Hai Bà Trưng', '0902222222', 'b@gmail.com', 'LK002');
INSERT INTO KHACHHANG VALUES ('KH003', N'Lê Văn C', N'789 Nguyễn Văn Cừ', '0903333333', 'c@gmail.com', 'LK001');
INSERT INTO KHACHHANG VALUES ('KH004', N'Phạm Thu D', N'101 Võ Thị Sáu', '0904444444', 'd@gmail.com', 'LK002');
INSERT INTO KHACHHANG VALUES ('KH005', N'Nguyễn Hoàng E', N'112 Trần Hưng Đạo', '0905555555', 'e@gmail.com', 'LK002');

-- NHANVIEN (dữ liệu chưa mã hóa - sẽ mã hóa sau)
INSERT INTO NHANVIEN VALUES ('NV001', N'Lê Văn C', N'Quản lý', N'Q1 HCM', '0911111111', 'c@gmail.com');
INSERT INTO NHANVIEN VALUES ('NV002', N'Phạm Thị D', N'Bán hàng', N'Q3 HCM', '0922222222', 'd@gmail.com');
INSERT INTO NHANVIEN VALUES ('NV003', N'Nguyễn Văn A', N'Kho hàng', N'Q2 HCM', '0923243546', 'a@gmail.com');
INSERT INTO NHANVIEN VALUES ('NV004', N'Trần Văn B', N'Kho hàng', N'Q4 HCM', '0933333333', 'b@gmail.com');
INSERT INTO NHANVIEN VALUES ('NV005', N'Hoàng Thị E', N'Bán hàng', N'Q7 HCM', '0944444444', 'e@gmail.com');

-- LOAISP
INSERT INTO LOAISP VALUES ('LSP001', N'Điện tử');
INSERT INTO LOAISP VALUES ('LSP002', N'Phụ kiện');
INSERT INTO LOAISP VALUES ('LSP003', N'Đồ gia dụng');
INSERT INTO LOAISP VALUES ('LSP004', N'Thời trang');
INSERT INTO LOAISP VALUES ('LSP005', N'Thực phẩm');

-- NCC
INSERT INTO NCC VALUES ('NCC001', N'Công ty ABC', N'Hà Nội', '0931111111', 'abc@gmail.com');
INSERT INTO NCC VALUES ('NCC002', N'Công ty XYZ', N'HCM', '0932222222', 'xyz@gmail.com');
INSERT INTO NCC VALUES ('NCC003', N'Công ty MNO', N'Đà Nẵng', '0933333333', 'mno@gmail.com');
INSERT INTO NCC VALUES ('NCC004', N'Công ty PQR', N'Hải Phòng', '0934444444', 'pqr@gmail.com');
INSERT INTO NCC VALUES ('NCC005', N'Công ty DEF', N'Cần Thơ', '0935555555', 'def@gmail.com');

-- SANPHAM
INSERT INTO SANPHAM VALUES ('SP001', N'Laptop Dell', 20000000, 25000000, 10, N'Hàng mới', '10-11-2025', 'NCC001', 'LSP001');
INSERT INTO SANPHAM VALUES ('SP002', N'Chuột Logitech', 500000, 800000, 20, N'Bảo hành 12 tháng', '09-10-2025', 'NCC002', 'LSP002');
INSERT INTO SANPHAM VALUES ('SP003', N'Máy ảnh Canon EOS', 15000000, 18000000, 5, N'Bảo hành 24 tháng', '15-10-2025', 'NCC001', 'LSP001');
INSERT INTO SANPHAM VALUES ('SP004', N'Tủ lạnh Inverter', 8500000, 10000000, 15, N'Bảo hành 36 tháng', '20-10-2025', 'NCC003', 'LSP003');
INSERT INTO SANPHAM VALUES ('SP005', N'Áo khoác gió Nam', 400000, 650000, 50, N'Chống nước', '10-10-2025', 'NCC004', 'LSP004');

-- DONHANG
INSERT INTO DONHANG VALUES ('DH001', '05-10-2025', null, N'Đang xử lý', 'KH001', 'NV002');
INSERT INTO DONHANG VALUES ('DH002', '03-10-2025', '04-10-2025', N'Đã giao', 'KH002', 'NV002');
INSERT INTO DONHANG VALUES ('DH003', '05-10-2025', '06-10-2025', N'Đã hủy', 'KH002', 'NV005');
INSERT INTO DONHANG VALUES ('DH004', '07-10-2025', null, N'Đang xử lý', 'KH003', 'NV001');
INSERT INTO DONHANG VALUES ('DH005', '08-10-2025', '09-10-2025', N'Đã giao', 'KH004', 'NV005');

-- CHITIETDONHANG
INSERT INTO CHITIETDONHANG VALUES ('DH001', 'SP001', 1, 25000000);
INSERT INTO CHITIETDONHANG VALUES ('DH001', 'SP002', 1, 800000);
INSERT INTO CHITIETDONHANG VALUES ('DH002', 'SP002', 2, 800000);
INSERT INTO CHITIETDONHANG VALUES ('DH002', 'SP003', 1, 18000000);
INSERT INTO CHITIETDONHANG VALUES ('DH004', 'SP004', 1, 10000000);
INSERT INTO CHITIETDONHANG VALUES ('DH003', 'SP005', 3, 650000);
INSERT INTO CHITIETDONHANG VALUES ('DH005', 'SP002', 1, 800000);

-- HOADON
INSERT INTO HOADON VALUES ('HD001', 'DH001', '05-10-2025', N'Tiền mặt', 25800000);
INSERT INTO HOADON VALUES ('HD002', 'DH002', '03-10-2025', N'Chuyển khoản', 19600000);
INSERT INTO HOADON VALUES ('HD003', 'DH003', '05-10-2025', N'Chuyển khoản', 1950000);
INSERT INTO HOADON VALUES ('HD004', 'DH004', '07-10-2025', N'Tiền mặt', 10000000);
INSERT INTO HOADON VALUES ('HD005', 'DH005', '08-10-2025', N'Tiền mặt', 800000);

-- TAIKHOAN (Password đã mã hóa AES-256-GCM)
-- Plaintext: NV001='Admin123@', NV002-NV005='Nhanvien123@'
INSERT INTO TAIKHOAN VALUES ('NV001', 'q4zw5J567j/P1zCG8jnD2r670tLld3BfaFaHgX+OtbhLIRUsPQ==', N'Quản lý');
INSERT INTO TAIKHOAN VALUES ('NV002', 'SsOrbR7/5D39SALj87Z66tfL0eTYMwtil/mmE5AlFIRm3Qar9kocqg==', N'Bán hàng');
INSERT INTO TAIKHOAN VALUES ('NV003', 'xC0i0E0yLnWlZ+OmwwWSWoNPFD7Zj1sKIHC1TZ01DRJ+NOllBZjOyA==', N'Kho hàng');
INSERT INTO TAIKHOAN VALUES ('NV004', 'TbvyqYpPUsoMPAoKtZqlMYPccIGCmEwe2VEyqcxYhm39QXamO0/XoA==', N'Kho hàng');
INSERT INTO TAIKHOAN VALUES ('NV005', 'DTkZBWSrqTAegePU7xi61Qoby/RD7rIh/3z08mwmRae5RFK/+KqF2w==', N'Bán hàng');

-- ============================================================
-- PHẦN 4: TẠO INDEXES
-- ============================================================
CREATE INDEX IDX_DONHANG_MANV ON DONHANG(MANV);
CREATE INDEX IDX_DONHANG_TRANGTHAI ON DONHANG(TRANGTHAI);
CREATE INDEX IDX_DONHANG_MAKH ON DONHANG(MAKH);
CREATE INDEX IDX_KHACHHANG_MALOAIKH ON KHACHHANG(MALOAIKH);
CREATE INDEX IDX_SANPHAM_MANCC ON SANPHAM(MANCC);
CREATE INDEX IDX_SANPHAM_MALOAISP ON SANPHAM(MALOAISP);
CREATE INDEX IDX_AUDIT_LOG_MANV ON AUDIT_LOG(MANV);
CREATE INDEX IDX_AUDIT_LOG_TABLE ON AUDIT_LOG(TABLE_NAME);
CREATE INDEX IDX_AUDIT_LOG_TIME ON AUDIT_LOG(ACTION_TIME);

-- ============================================================
-- PHẦN 5: ENCRYPTION PACKAGE (Mã hóa NHANVIEN)
-- ============================================================

CREATE OR REPLACE PACKAGE PKG_ENCRYPTION AS
    -- AES-256 Key (PHẢI đúng 32 bytes = 32 ký tự ASCII)
    -- Key: QLDH2024AES256KEYSECRET123456789 (đúng 32 ký tự)
    g_aes_key RAW(32) := UTL_RAW.CAST_TO_RAW('QLDH2024AES256KEYSECRET123456789');
    
    -- Key riêng cho SDT (simulate RSA) - PHẢI đúng 32 bytes
    -- Key: QLDH2024SDTRSAKEYSECRET123456789 (đúng 32 ký tự)
    g_sdt_key RAW(32) := UTL_RAW.CAST_TO_RAW('QLDH2024SDTRSAKEYSECRET123456789');
    
    c_encrypt_type PLS_INTEGER := DBMS_CRYPTO.ENCRYPT_AES256 
                                + DBMS_CRYPTO.CHAIN_CBC 
                                + DBMS_CRYPTO.PAD_PKCS5;
    
    -- AES Functions
    FUNCTION ENCRYPT_AES(p_plaintext IN VARCHAR2) RETURN VARCHAR2;
    FUNCTION DECRYPT_AES(p_ciphertext IN VARCHAR2) RETURN VARCHAR2;
    
    -- RSA Functions (simulated with different key)
    FUNCTION ENCRYPT_RSA(p_plaintext IN VARCHAR2) RETURN VARCHAR2;
    FUNCTION DECRYPT_RSA(p_ciphertext IN VARCHAR2) RETURN VARCHAR2;
    
    -- Employee Data Encryption
    PROCEDURE ENCRYPT_NHANVIEN_DIACHI(p_manv IN VARCHAR2);
    PROCEDURE ENCRYPT_NHANVIEN_SDT(p_manv IN VARCHAR2);
    PROCEDURE ENCRYPT_NHANVIEN_DATA(p_manv IN VARCHAR2);
    
    FUNCTION DECRYPT_NHANVIEN_DIACHI(p_manv IN VARCHAR2) RETURN VARCHAR2;
    FUNCTION DECRYPT_NHANVIEN_SDT(p_manv IN VARCHAR2) RETURN VARCHAR2;
    
    -- Customer Data Encryption (KHACHHANG)
    -- TENKH: AES, DIACHI: RSA, SDT: Hybrid (dùng key riêng)
    g_hybrid_key RAW(32) := UTL_RAW.CAST_TO_RAW('QLDH2024HYBRIDKEYSECRET123456789');
    
    FUNCTION ENCRYPT_HYBRID(p_plaintext IN VARCHAR2) RETURN VARCHAR2;
    FUNCTION DECRYPT_HYBRID(p_ciphertext IN VARCHAR2) RETURN VARCHAR2;
    
    PROCEDURE ENCRYPT_KHACHHANG_DATA(p_makh IN VARCHAR2);
    FUNCTION DECRYPT_KHACHHANG_TENKH(p_makh IN VARCHAR2) RETURN VARCHAR2;
    FUNCTION DECRYPT_KHACHHANG_DIACHI(p_makh IN VARCHAR2) RETURN VARCHAR2;
    FUNCTION DECRYPT_KHACHHANG_SDT(p_makh IN VARCHAR2) RETURN VARCHAR2;
END PKG_ENCRYPTION;
/

CREATE OR REPLACE PACKAGE BODY PKG_ENCRYPTION AS
    
    FUNCTION ENCRYPT_AES(p_plaintext IN VARCHAR2) RETURN VARCHAR2 IS
        v_raw_data RAW(2000);
        v_encrypted RAW(2000);
        v_iv RAW(16);
    BEGIN
        IF p_plaintext IS NULL THEN RETURN NULL; END IF;
        v_iv := DBMS_CRYPTO.RANDOMBYTES(16);
        v_raw_data := UTL_I18N.STRING_TO_RAW(p_plaintext, 'AL32UTF8');
        v_encrypted := DBMS_CRYPTO.ENCRYPT(src => v_raw_data, typ => c_encrypt_type, key => g_aes_key, iv => v_iv);
        RETURN UTL_RAW.CAST_TO_VARCHAR2(UTL_ENCODE.BASE64_ENCODE(UTL_RAW.CONCAT(v_iv, v_encrypted)));
    EXCEPTION WHEN OTHERS THEN
        RAISE_APPLICATION_ERROR(-20001, 'AES Encrypt Error: ' || SQLERRM);
    END ENCRYPT_AES;
    
    FUNCTION DECRYPT_AES(p_ciphertext IN VARCHAR2) RETURN VARCHAR2 IS
        v_raw_data RAW(2000);
        v_decrypted RAW(2000);
        v_iv RAW(16);
        v_encrypted RAW(2000);
    BEGIN
        IF p_ciphertext IS NULL THEN RETURN NULL; END IF;
        v_raw_data := UTL_ENCODE.BASE64_DECODE(UTL_RAW.CAST_TO_RAW(p_ciphertext));
        v_iv := UTL_RAW.SUBSTR(v_raw_data, 1, 16);
        v_encrypted := UTL_RAW.SUBSTR(v_raw_data, 17);
        v_decrypted := DBMS_CRYPTO.DECRYPT(src => v_encrypted, typ => c_encrypt_type, key => g_aes_key, iv => v_iv);
        RETURN UTL_I18N.RAW_TO_CHAR(v_decrypted, 'AL32UTF8');
    EXCEPTION WHEN OTHERS THEN
        RAISE_APPLICATION_ERROR(-20002, 'AES Decrypt Error: ' || SQLERRM);
    END DECRYPT_AES;
    
    FUNCTION ENCRYPT_RSA(p_plaintext IN VARCHAR2) RETURN VARCHAR2 IS
        v_raw_data RAW(2000);
        v_encrypted RAW(2000);
        v_iv RAW(16);
    BEGIN
        IF p_plaintext IS NULL THEN RETURN NULL; END IF;
        v_iv := DBMS_CRYPTO.RANDOMBYTES(16);
        v_raw_data := UTL_I18N.STRING_TO_RAW(p_plaintext, 'AL32UTF8');
        v_encrypted := DBMS_CRYPTO.ENCRYPT(src => v_raw_data, typ => c_encrypt_type, key => g_sdt_key, iv => v_iv);
        RETURN UTL_RAW.CAST_TO_VARCHAR2(UTL_ENCODE.BASE64_ENCODE(UTL_RAW.CONCAT(v_iv, v_encrypted)));
    EXCEPTION WHEN OTHERS THEN
        RAISE_APPLICATION_ERROR(-20003, 'RSA Encrypt Error: ' || SQLERRM);
    END ENCRYPT_RSA;
    
    FUNCTION DECRYPT_RSA(p_ciphertext IN VARCHAR2) RETURN VARCHAR2 IS
        v_raw_data RAW(2000);
        v_decrypted RAW(2000);
        v_iv RAW(16);
        v_encrypted RAW(2000);
        v_cipher_clean VARCHAR2(4000);
    BEGIN
        IF p_ciphertext IS NULL THEN RETURN NULL; END IF;
        v_cipher_clean := REPLACE(p_ciphertext, 'RSA:', '');
        v_raw_data := UTL_ENCODE.BASE64_DECODE(UTL_RAW.CAST_TO_RAW(v_cipher_clean));
        v_iv := UTL_RAW.SUBSTR(v_raw_data, 1, 16);
        v_encrypted := UTL_RAW.SUBSTR(v_raw_data, 17);
        v_decrypted := DBMS_CRYPTO.DECRYPT(src => v_encrypted, typ => c_encrypt_type, key => g_sdt_key, iv => v_iv);
        RETURN UTL_I18N.RAW_TO_CHAR(v_decrypted, 'AL32UTF8');
    EXCEPTION WHEN OTHERS THEN
        RAISE_APPLICATION_ERROR(-20004, 'RSA Decrypt Error: ' || SQLERRM);
    END DECRYPT_RSA;
    
    PROCEDURE ENCRYPT_NHANVIEN_DIACHI(p_manv IN VARCHAR2) IS
        v_diachi NVARCHAR2(500);
        v_encrypted VARCHAR2(500);
    BEGIN
        SELECT DIACHI INTO v_diachi FROM NHANVIEN WHERE MANV = p_manv;
        IF v_diachi IS NOT NULL AND SUBSTR(v_diachi, 1, 4) != 'AES:' THEN
            v_encrypted := 'AES:' || ENCRYPT_AES(v_diachi);
            UPDATE NHANVIEN SET DIACHI = v_encrypted WHERE MANV = p_manv;
        END IF;
    END ENCRYPT_NHANVIEN_DIACHI;
    
    PROCEDURE ENCRYPT_NHANVIEN_SDT(p_manv IN VARCHAR2) IS
        v_sdt VARCHAR2(500);
        v_encrypted VARCHAR2(500);
    BEGIN
        SELECT SDT INTO v_sdt FROM NHANVIEN WHERE MANV = p_manv;
        IF v_sdt IS NOT NULL AND SUBSTR(v_sdt, 1, 4) != 'RSA:' THEN
            v_encrypted := 'RSA:' || ENCRYPT_RSA(v_sdt);
            UPDATE NHANVIEN SET SDT = v_encrypted WHERE MANV = p_manv;
        END IF;
    END ENCRYPT_NHANVIEN_SDT;
    
    PROCEDURE ENCRYPT_NHANVIEN_DATA(p_manv IN VARCHAR2) IS
    BEGIN
        ENCRYPT_NHANVIEN_DIACHI(p_manv);
        ENCRYPT_NHANVIEN_SDT(p_manv);
        COMMIT;
    END ENCRYPT_NHANVIEN_DATA;
    
    FUNCTION DECRYPT_NHANVIEN_DIACHI(p_manv IN VARCHAR2) RETURN VARCHAR2 IS
        v_diachi VARCHAR2(500);
    BEGIN
        SELECT DIACHI INTO v_diachi FROM NHANVIEN WHERE MANV = p_manv;
        IF v_diachi IS NULL THEN RETURN NULL; END IF;
        IF SUBSTR(v_diachi, 1, 4) = 'AES:' THEN
            RETURN DECRYPT_AES(SUBSTR(v_diachi, 5));
        ELSE
            RETURN v_diachi;
        END IF;
    END DECRYPT_NHANVIEN_DIACHI;
    
    FUNCTION DECRYPT_NHANVIEN_SDT(p_manv IN VARCHAR2) RETURN VARCHAR2 IS
        v_sdt VARCHAR2(500);
    BEGIN
        SELECT SDT INTO v_sdt FROM NHANVIEN WHERE MANV = p_manv;
        IF v_sdt IS NULL THEN RETURN NULL; END IF;
        IF SUBSTR(v_sdt, 1, 4) = 'RSA:' THEN
            RETURN DECRYPT_RSA(v_sdt);
        ELSE
            RETURN v_sdt;
        END IF;
    END DECRYPT_NHANVIEN_SDT;
    
    -- ========== KHACHHANG ENCRYPTION ==========
    
    -- Hybrid encryption (AES+RSA simulation với key riêng)
    FUNCTION ENCRYPT_HYBRID(p_plaintext IN VARCHAR2) RETURN VARCHAR2 IS
        v_raw_data RAW(2000);
        v_encrypted RAW(2000);
        v_iv RAW(16);
    BEGIN
        IF p_plaintext IS NULL THEN RETURN NULL; END IF;
        v_iv := DBMS_CRYPTO.RANDOMBYTES(16);
        v_raw_data := UTL_I18N.STRING_TO_RAW(p_plaintext, 'AL32UTF8');
        v_encrypted := DBMS_CRYPTO.ENCRYPT(src => v_raw_data, typ => c_encrypt_type, key => g_hybrid_key, iv => v_iv);
        RETURN UTL_RAW.CAST_TO_VARCHAR2(UTL_ENCODE.BASE64_ENCODE(UTL_RAW.CONCAT(v_iv, v_encrypted)));
    EXCEPTION WHEN OTHERS THEN
        RAISE_APPLICATION_ERROR(-20005, 'Hybrid Encrypt Error: ' || SQLERRM);
    END ENCRYPT_HYBRID;
    
    FUNCTION DECRYPT_HYBRID(p_ciphertext IN VARCHAR2) RETURN VARCHAR2 IS
        v_raw_data RAW(2000);
        v_decrypted RAW(2000);
        v_iv RAW(16);
        v_encrypted RAW(2000);
        v_cipher_clean VARCHAR2(4000);
    BEGIN
        IF p_ciphertext IS NULL THEN RETURN NULL; END IF;
        v_cipher_clean := REPLACE(p_ciphertext, 'HYBRID:', '');
        v_raw_data := UTL_ENCODE.BASE64_DECODE(UTL_RAW.CAST_TO_RAW(v_cipher_clean));
        v_iv := UTL_RAW.SUBSTR(v_raw_data, 1, 16);
        v_encrypted := UTL_RAW.SUBSTR(v_raw_data, 17);
        v_decrypted := DBMS_CRYPTO.DECRYPT(src => v_encrypted, typ => c_encrypt_type, key => g_hybrid_key, iv => v_iv);
        RETURN UTL_I18N.RAW_TO_CHAR(v_decrypted, 'AL32UTF8');
    EXCEPTION WHEN OTHERS THEN
        RAISE_APPLICATION_ERROR(-20006, 'Hybrid Decrypt Error: ' || SQLERRM);
    END DECRYPT_HYBRID;
    
    PROCEDURE ENCRYPT_KHACHHANG_DATA(p_makh IN VARCHAR2) IS
        v_tenkh NVARCHAR2(500);
        v_diachi NVARCHAR2(1000);
        v_sdt VARCHAR2(500);
    BEGIN
        SELECT TENKH, DIACHI, SDT INTO v_tenkh, v_diachi, v_sdt FROM KHACHHANG WHERE MAKH = p_makh;
        
        -- Mã hóa TENKH bằng AES
        IF v_tenkh IS NOT NULL AND SUBSTR(v_tenkh, 1, 4) != 'AES:' THEN
            UPDATE KHACHHANG SET TENKH = 'AES:' || ENCRYPT_AES(v_tenkh) WHERE MAKH = p_makh;
        END IF;
        
        -- Mã hóa DIACHI bằng RSA
        IF v_diachi IS NOT NULL AND SUBSTR(v_diachi, 1, 4) != 'RSA:' THEN
            UPDATE KHACHHANG SET DIACHI = 'RSA:' || ENCRYPT_RSA(v_diachi) WHERE MAKH = p_makh;
        END IF;
        
        -- Mã hóa SDT bằng Hybrid
        IF v_sdt IS NOT NULL AND SUBSTR(v_sdt, 1, 7) != 'HYBRID:' THEN
            UPDATE KHACHHANG SET SDT = 'HYBRID:' || ENCRYPT_HYBRID(v_sdt) WHERE MAKH = p_makh;
        END IF;
        
        COMMIT;
    END ENCRYPT_KHACHHANG_DATA;
    
    FUNCTION DECRYPT_KHACHHANG_TENKH(p_makh IN VARCHAR2) RETURN VARCHAR2 IS
        v_tenkh VARCHAR2(500);
    BEGIN
        SELECT TENKH INTO v_tenkh FROM KHACHHANG WHERE MAKH = p_makh;
        IF v_tenkh IS NULL THEN RETURN NULL; END IF;
        IF SUBSTR(v_tenkh, 1, 4) = 'AES:' THEN
            RETURN DECRYPT_AES(SUBSTR(v_tenkh, 5));
        ELSE
            RETURN v_tenkh;
        END IF;
    END DECRYPT_KHACHHANG_TENKH;
    
    FUNCTION DECRYPT_KHACHHANG_DIACHI(p_makh IN VARCHAR2) RETURN VARCHAR2 IS
        v_diachi VARCHAR2(1000);
    BEGIN
        SELECT DIACHI INTO v_diachi FROM KHACHHANG WHERE MAKH = p_makh;
        IF v_diachi IS NULL THEN RETURN NULL; END IF;
        IF SUBSTR(v_diachi, 1, 4) = 'RSA:' THEN
            RETURN DECRYPT_RSA(v_diachi);
        ELSE
            RETURN v_diachi;
        END IF;
    END DECRYPT_KHACHHANG_DIACHI;
    
    FUNCTION DECRYPT_KHACHHANG_SDT(p_makh IN VARCHAR2) RETURN VARCHAR2 IS
        v_sdt VARCHAR2(500);
    BEGIN
        SELECT SDT INTO v_sdt FROM KHACHHANG WHERE MAKH = p_makh;
        IF v_sdt IS NULL THEN RETURN NULL; END IF;
        IF SUBSTR(v_sdt, 1, 7) = 'HYBRID:' THEN
            RETURN DECRYPT_HYBRID(v_sdt);
        ELSE
            RETURN v_sdt;
        END IF;
    END DECRYPT_KHACHHANG_SDT;
    
END PKG_ENCRYPTION;
/

-- ============================================================
-- PHẦN 6: SECURITY PACKAGE (Hỗ trợ VPD)
-- ============================================================

CREATE OR REPLACE PACKAGE PKG_SECURITY AS
    PROCEDURE SET_USER_CONTEXT(p_manv VARCHAR2, p_vaitro VARCHAR2);
    FUNCTION GET_CURRENT_MANV RETURN VARCHAR2;
    FUNCTION GET_CURRENT_VAITRO RETURN VARCHAR2;
END PKG_SECURITY;
/

CREATE OR REPLACE PACKAGE BODY PKG_SECURITY AS
    PROCEDURE SET_USER_CONTEXT(p_manv VARCHAR2, p_vaitro VARCHAR2) IS
    BEGIN
        DBMS_SESSION.SET_CONTEXT('USER_CTX', 'MANV', p_manv);
        DBMS_SESSION.SET_CONTEXT('USER_CTX', 'VAITRO', p_vaitro);
    END SET_USER_CONTEXT;
    
    FUNCTION GET_CURRENT_MANV RETURN VARCHAR2 IS
    BEGIN
        RETURN SYS_CONTEXT('USER_CTX', 'MANV');
    END GET_CURRENT_MANV;
    
    FUNCTION GET_CURRENT_VAITRO RETURN VARCHAR2 IS
    BEGIN
        RETURN SYS_CONTEXT('USER_CTX', 'VAITRO');
    END GET_CURRENT_VAITRO;
END PKG_SECURITY;
/

-- ============================================================
-- PHẦN 7: VPD POLICY (Lọc DONHANG theo nhân viên)
-- ============================================================

CREATE OR REPLACE FUNCTION FN_FILTER_DONHANG (
    p_schema_name VARCHAR2,
    p_table_name VARCHAR2
) RETURN VARCHAR2 IS
    v_vaitro VARCHAR2(50);
    v_manv VARCHAR2(10);
BEGIN
    v_vaitro := SYS_CONTEXT('USER_CTX', 'VAITRO');
    v_manv := SYS_CONTEXT('USER_CTX', 'MANV');
    
    IF v_vaitro IS NULL OR v_vaitro = N'Quản lý' THEN
        RETURN NULL;
    END IF;
    
    IF v_vaitro = N'Bán hàng' THEN
        RETURN 'MANV = ''' || v_manv || '''';
    END IF;
    
    RETURN NULL;
END FN_FILTER_DONHANG;
/

-- Áp dụng VPD Policy
BEGIN
    BEGIN
        DBMS_RLS.DROP_POLICY(object_schema => 'QLDH', object_name => 'DONHANG', policy_name => 'POLICY_FILTER_DONHANG');
    EXCEPTION WHEN OTHERS THEN NULL;
    END;
    
    DBMS_RLS.ADD_POLICY(
        object_schema   => 'QLDH',
        object_name     => 'DONHANG',
        policy_name     => 'POLICY_FILTER_DONHANG',
        function_schema => 'QLDH',
        policy_function => 'FN_FILTER_DONHANG',
        statement_types => 'SELECT,UPDATE,DELETE',
        update_check    => TRUE,
        enable          => TRUE
    );
END;
/

-- ============================================================
-- PHẦN 8: FGA (Fine-Grained Auditing)
-- ============================================================

-- FGA cho NHANVIEN
BEGIN
    BEGIN DBMS_FGA.DROP_POLICY(object_schema => 'QLDH', object_name => 'NHANVIEN', policy_name => 'FGA_NHANVIEN');
    EXCEPTION WHEN OTHERS THEN NULL; END;
    
    DBMS_FGA.ADD_POLICY(
        object_schema   => 'QLDH',
        object_name     => 'NHANVIEN',
        policy_name     => 'FGA_NHANVIEN',
        audit_column    => 'DIACHI,SDT',
        statement_types => 'SELECT,INSERT,UPDATE,DELETE',
        audit_trail     => DBMS_FGA.DB_EXTENDED
    );
END;
/

-- FGA cho TAIKHOAN
BEGIN
    BEGIN DBMS_FGA.DROP_POLICY(object_schema => 'QLDH', object_name => 'TAIKHOAN', policy_name => 'FGA_TAIKHOAN');
    EXCEPTION WHEN OTHERS THEN NULL; END;
    
    DBMS_FGA.ADD_POLICY(
        object_schema   => 'QLDH',
        object_name     => 'TAIKHOAN',
        policy_name     => 'FGA_TAIKHOAN',
        audit_column    => 'MATKHAU,VAITRO',
        statement_types => 'SELECT,INSERT,UPDATE,DELETE',
        audit_trail     => DBMS_FGA.DB_EXTENDED
    );
END;
/

-- FGA cho KHACHHANG
BEGIN
    BEGIN DBMS_FGA.DROP_POLICY(object_schema => 'QLDH', object_name => 'KHACHHANG', policy_name => 'FGA_KHACHHANG');
    EXCEPTION WHEN OTHERS THEN NULL; END;
    
    DBMS_FGA.ADD_POLICY(
        object_schema   => 'QLDH',
        object_name     => 'KHACHHANG',
        policy_name     => 'FGA_KHACHHANG',
        audit_column    => 'TENKH,DIACHI,SDT',
        statement_types => 'SELECT,INSERT,UPDATE,DELETE',
        audit_trail     => DBMS_FGA.DB_EXTENDED
    );
END;
/

-- FGA cho DONHANG
BEGIN
    BEGIN DBMS_FGA.DROP_POLICY(object_schema => 'QLDH', object_name => 'DONHANG', policy_name => 'FGA_DONHANG');
    EXCEPTION WHEN OTHERS THEN NULL; END;
    
    DBMS_FGA.ADD_POLICY(
        object_schema   => 'QLDH',
        object_name     => 'DONHANG',
        policy_name     => 'FGA_DONHANG',
        audit_column    => 'TRANGTHAI',
        statement_types => 'UPDATE,DELETE',
        audit_trail     => DBMS_FGA.DB_EXTENDED
    );
END;
/

-- ============================================================
-- PHẦN 9: ENCRYPTION TRIGGERS (Tự động mã hóa khi INSERT/UPDATE)
-- ============================================================

-- Trigger TỰ ĐỘNG MÃ HÓA DIACHI và SDT khi thêm/sửa NHANVIEN
CREATE OR REPLACE TRIGGER TRG_ENCRYPT_NHANVIEN
BEFORE INSERT OR UPDATE ON NHANVIEN
FOR EACH ROW
DECLARE
    v_encrypted_diachi VARCHAR2(500);
    v_encrypted_sdt VARCHAR2(500);
BEGIN
    -- Mã hóa DIACHI bằng AES (nếu chưa mã hóa)
    IF :NEW.DIACHI IS NOT NULL AND SUBSTR(:NEW.DIACHI, 1, 4) != 'AES:' THEN
        v_encrypted_diachi := 'AES:' || PKG_ENCRYPTION.ENCRYPT_AES(:NEW.DIACHI);
        :NEW.DIACHI := v_encrypted_diachi;
    END IF;
    
    -- Mã hóa SDT bằng RSA (nếu chưa mã hóa)
    IF :NEW.SDT IS NOT NULL AND SUBSTR(:NEW.SDT, 1, 4) != 'RSA:' THEN
        v_encrypted_sdt := 'RSA:' || PKG_ENCRYPTION.ENCRYPT_RSA(:NEW.SDT);
        :NEW.SDT := v_encrypted_sdt;
    END IF;
END;
/

-- Trigger TỰ ĐỘNG MÃ HÓA TENKH, DIACHI, SDT khi thêm/sửa KHACHHANG
-- TENKH: AES, DIACHI: RSA, SDT: Hybrid (AES+RSA)
CREATE OR REPLACE TRIGGER TRG_ENCRYPT_KHACHHANG
BEFORE INSERT OR UPDATE ON KHACHHANG
FOR EACH ROW
DECLARE
    v_encrypted_tenkh VARCHAR2(500);
    v_encrypted_diachi VARCHAR2(1000);
    v_encrypted_sdt VARCHAR2(500);
BEGIN
    -- Mã hóa TENKH bằng AES (nếu chưa mã hóa)
    IF :NEW.TENKH IS NOT NULL AND SUBSTR(:NEW.TENKH, 1, 4) != 'AES:' THEN
        v_encrypted_tenkh := 'AES:' || PKG_ENCRYPTION.ENCRYPT_AES(:NEW.TENKH);
        :NEW.TENKH := v_encrypted_tenkh;
    END IF;
    
    -- Mã hóa DIACHI bằng RSA (nếu chưa mã hóa)
    IF :NEW.DIACHI IS NOT NULL AND SUBSTR(:NEW.DIACHI, 1, 4) != 'RSA:' THEN
        v_encrypted_diachi := 'RSA:' || PKG_ENCRYPTION.ENCRYPT_RSA(:NEW.DIACHI);
        :NEW.DIACHI := v_encrypted_diachi;
    END IF;
    
    -- Mã hóa SDT bằng Hybrid (nếu chưa mã hóa)
    IF :NEW.SDT IS NOT NULL AND SUBSTR(:NEW.SDT, 1, 7) != 'HYBRID:' THEN
        v_encrypted_sdt := 'HYBRID:' || PKG_ENCRYPTION.ENCRYPT_HYBRID(:NEW.SDT);
        :NEW.SDT := v_encrypted_sdt;
    END IF;
END;
/

-- ============================================================
-- PHẦN 10: AUDIT TRIGGERS
-- ============================================================

-- Trigger cho NHANVIEN (Audit Log)
CREATE OR REPLACE TRIGGER TRG_AUDIT_NHANVIEN
AFTER INSERT OR UPDATE OR DELETE ON NHANVIEN
FOR EACH ROW
DECLARE
    v_action VARCHAR2(10);
    v_old_value CLOB;
    v_new_value CLOB;
    v_manv VARCHAR2(10);
BEGIN
    v_manv := NVL(SYS_CONTEXT('USER_CTX', 'MANV'), 'SYSTEM');
    
    IF INSERTING THEN
        v_action := 'INSERT';
        v_new_value := '{"MANV":"' || :NEW.MANV || '","TENNV":"' || :NEW.TENNV || '"}';
        INSERT INTO AUDIT_LOG (MANV, TABLE_NAME, ACTION_TYPE, RECORD_ID, NEW_VALUE, DESCRIPTION)
        VALUES (v_manv, 'NHANVIEN', v_action, :NEW.MANV, v_new_value, N'Thêm nhân viên');
    ELSIF UPDATING THEN
        v_action := 'UPDATE';
        v_old_value := '{"TENNV":"' || :OLD.TENNV || '","DIACHI":"' || :OLD.DIACHI || '"}';
        v_new_value := '{"TENNV":"' || :NEW.TENNV || '","DIACHI":"' || :NEW.DIACHI || '"}';
        INSERT INTO AUDIT_LOG (MANV, TABLE_NAME, ACTION_TYPE, RECORD_ID, OLD_VALUE, NEW_VALUE, DESCRIPTION)
        VALUES (v_manv, 'NHANVIEN', v_action, :NEW.MANV, v_old_value, v_new_value, N'Cập nhật nhân viên');
    ELSIF DELETING THEN
        v_action := 'DELETE';
        v_old_value := '{"MANV":"' || :OLD.MANV || '","TENNV":"' || :OLD.TENNV || '"}';
        INSERT INTO AUDIT_LOG (MANV, TABLE_NAME, ACTION_TYPE, RECORD_ID, OLD_VALUE, DESCRIPTION)
        VALUES (v_manv, 'NHANVIEN', v_action, :OLD.MANV, v_old_value, N'Xóa nhân viên');
    END IF;
END;
/

-- Trigger cho KHACHHANG
CREATE OR REPLACE TRIGGER TRG_AUDIT_KHACHHANG
AFTER INSERT OR UPDATE OR DELETE ON KHACHHANG
FOR EACH ROW
DECLARE
    v_action VARCHAR2(10);
    v_old_value CLOB;
    v_new_value CLOB;
    v_manv VARCHAR2(10);
BEGIN
    v_manv := NVL(SYS_CONTEXT('USER_CTX', 'MANV'), 'SYSTEM');
    
    IF INSERTING THEN
        v_action := 'INSERT';
        v_new_value := '{"MAKH":"' || :NEW.MAKH || '"}';
        INSERT INTO AUDIT_LOG (MANV, TABLE_NAME, ACTION_TYPE, RECORD_ID, NEW_VALUE, DESCRIPTION)
        VALUES (v_manv, 'KHACHHANG', v_action, :NEW.MAKH, v_new_value, N'Thêm khách hàng');
    ELSIF UPDATING THEN
        v_action := 'UPDATE';
        INSERT INTO AUDIT_LOG (MANV, TABLE_NAME, ACTION_TYPE, RECORD_ID, DESCRIPTION)
        VALUES (v_manv, 'KHACHHANG', v_action, :NEW.MAKH, N'Cập nhật khách hàng');
    ELSIF DELETING THEN
        v_action := 'DELETE';
        INSERT INTO AUDIT_LOG (MANV, TABLE_NAME, ACTION_TYPE, RECORD_ID, DESCRIPTION)
        VALUES (v_manv, 'KHACHHANG', v_action, :OLD.MAKH, N'Xóa khách hàng');
    END IF;
END;
/

-- Trigger cho DONHANG
CREATE OR REPLACE TRIGGER TRG_AUDIT_DONHANG
AFTER INSERT OR UPDATE OR DELETE ON DONHANG
FOR EACH ROW
DECLARE
    v_action VARCHAR2(10);
    v_old_value CLOB;
    v_new_value CLOB;
    v_manv VARCHAR2(10);
BEGIN
    v_manv := NVL(SYS_CONTEXT('USER_CTX', 'MANV'), 'SYSTEM');
    
    IF INSERTING THEN
        v_action := 'INSERT';
        v_new_value := '{"MADH":"' || :NEW.MADH || '","TRANGTHAI":"' || :NEW.TRANGTHAI || '"}';
        INSERT INTO AUDIT_LOG (MANV, TABLE_NAME, ACTION_TYPE, RECORD_ID, NEW_VALUE, DESCRIPTION)
        VALUES (v_manv, 'DONHANG', v_action, :NEW.MADH, v_new_value, N'Tạo đơn hàng');
    ELSIF UPDATING THEN
        v_action := 'UPDATE';
        v_old_value := '{"TRANGTHAI":"' || :OLD.TRANGTHAI || '"}';
        v_new_value := '{"TRANGTHAI":"' || :NEW.TRANGTHAI || '"}';
        INSERT INTO AUDIT_LOG (MANV, TABLE_NAME, ACTION_TYPE, RECORD_ID, OLD_VALUE, NEW_VALUE, DESCRIPTION)
        VALUES (v_manv, 'DONHANG', v_action, :NEW.MADH, v_old_value, v_new_value, N'Cập nhật đơn hàng');
    ELSIF DELETING THEN
        v_action := 'DELETE';
        INSERT INTO AUDIT_LOG (MANV, TABLE_NAME, ACTION_TYPE, RECORD_ID, DESCRIPTION)
        VALUES (v_manv, 'DONHANG', v_action, :OLD.MADH, N'Xóa đơn hàng');
    END IF;
END;
/

-- Trigger cho SANPHAM
CREATE OR REPLACE TRIGGER TRG_AUDIT_SANPHAM
AFTER UPDATE OR DELETE ON SANPHAM
FOR EACH ROW
DECLARE
    v_action VARCHAR2(10);
    v_manv VARCHAR2(10);
BEGIN
    v_manv := NVL(SYS_CONTEXT('USER_CTX', 'MANV'), 'SYSTEM');
    
    IF UPDATING THEN
        INSERT INTO AUDIT_LOG (MANV, TABLE_NAME, ACTION_TYPE, RECORD_ID, DESCRIPTION)
        VALUES (v_manv, 'SANPHAM', 'UPDATE', :NEW.MASP, N'Cập nhật sản phẩm');
    ELSIF DELETING THEN
        INSERT INTO AUDIT_LOG (MANV, TABLE_NAME, ACTION_TYPE, RECORD_ID, DESCRIPTION)
        VALUES (v_manv, 'SANPHAM', 'DELETE', :OLD.MASP, N'Xóa sản phẩm');
    END IF;
END;
/

-- ============================================================
-- PHẦN 10: VIEW GIẢI MÃ NHÂN VIÊN
-- ============================================================

CREATE OR REPLACE VIEW V_NHANVIEN_DECRYPT AS
SELECT 
    MANV,
    TENNV,
    CHUCVU,
    PKG_ENCRYPTION.DECRYPT_NHANVIEN_DIACHI(MANV) AS DIACHI,
    PKG_ENCRYPTION.DECRYPT_NHANVIEN_SDT(MANV) AS SDT,
    EMAIL
FROM NHANVIEN;

-- View giải mã KHACHHANG
CREATE OR REPLACE VIEW V_KHACHHANG_DECRYPT AS
SELECT 
    MAKH,
    PKG_ENCRYPTION.DECRYPT_KHACHHANG_TENKH(MAKH) AS TENKH,
    PKG_ENCRYPTION.DECRYPT_KHACHHANG_DIACHI(MAKH) AS DIACHI,
    PKG_ENCRYPTION.DECRYPT_KHACHHANG_SDT(MAKH) AS SDT,
    EMAIL,
    MALOAIKH
FROM KHACHHANG;

-- ============================================================
-- PHẦN 11: MÃ HÓA DỮ LIỆU NHÂN VIÊN HIỆN TẠI
-- ============================================================

-- Mã hóa dữ liệu NHANVIEN hiện tại
DECLARE
    CURSOR c_nv IS SELECT MANV FROM NHANVIEN;
BEGIN
    FOR rec IN c_nv LOOP
        PKG_ENCRYPTION.ENCRYPT_NHANVIEN_DATA(rec.MANV);
    END LOOP;
    COMMIT;
    DBMS_OUTPUT.PUT_LINE('Đã mã hóa dữ liệu nhân viên');
END;
/

-- Mã hóa dữ liệu KHACHHANG hiện tại
DECLARE
    CURSOR c_kh IS SELECT MAKH FROM KHACHHANG;
BEGIN
    FOR rec IN c_kh LOOP
        PKG_ENCRYPTION.ENCRYPT_KHACHHANG_DATA(rec.MAKH);
    END LOOP;
    COMMIT;
    DBMS_OUTPUT.PUT_LINE('Đã mã hóa dữ liệu khách hàng');
END;
/

-- ============================================================
-- HOÀN TẤT!
-- ============================================================

COMMIT;

SELECT 'Database QLDH đã được tạo và cấu hình bảo mật thành công!' AS STATUS FROM DUAL;

-- Kiểm tra dữ liệu
SELECT * FROM LOAIKH;
SELECT * FROM NHANVIEN;
SELECT * FROM KHACHHANG;
SELECT * FROM TAIKHOAN;
SELECT * FROM V_NHANVIEN_DECRYPT;
SELECT * FROM V_KHACHHANG_DECRYPT;
