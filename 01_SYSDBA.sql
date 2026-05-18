-- ============================================================
-- FILE 1: SETUP SYSDBA (Chạy 1 lần duy nhất với quyền SYSDBA)
-- ============================================================

SET SERVEROUTPUT ON;

-- ============================================================
-- 0. KIỂM TRA VÀ CHUYỂN SANG PDB
-- ============================================================
-- Hiển thị container hiện tại
SHOW CON_NAME;

-- Nếu đang ở CDB$ROOT, chuyển sang PDB
ALTER SESSION SET CONTAINER = FREEPDB1;

-- Mở PDB nếu chưa mở
ALTER PLUGGABLE DATABASE FREEPDB1 OPEN;

-- Kiểm tra lại
SHOW CON_NAME;

-- ============================================================
-- 1. TẠO USER QLDH
-- ============================================================
DECLARE
    v_count NUMBER;
BEGIN
    SELECT COUNT(*) INTO v_count FROM dba_users WHERE username = 'QLDH';
    IF v_count > 0 THEN
        EXECUTE IMMEDIATE 'DROP USER QLDH CASCADE';
        DBMS_OUTPUT.PUT_LINE('Da xoa user QLDH cu');
    END IF;
END;
/

CREATE USER QLDH IDENTIFIED BY "123"
    DEFAULT TABLESPACE USERS
    TEMPORARY TABLESPACE TEMP
    QUOTA UNLIMITED ON USERS;

BEGIN
    DBMS_OUTPUT.PUT_LINE('Da tao user QLDH');
END;
/

-- ============================================================
-- 2. CẤP QUYỀN CƠ BẢN
-- ============================================================
GRANT CREATE SESSION TO QLDH;
GRANT CREATE TABLE TO QLDH;
GRANT CREATE PROCEDURE TO QLDH;
GRANT CREATE TRIGGER TO QLDH;
GRANT CREATE VIEW TO QLDH;
GRANT CREATE SEQUENCE TO QLDH;
GRANT CREATE ANY CONTEXT TO QLDH;

-- Quyền cho mã hóa và VPD
GRANT EXECUTE ON DBMS_CRYPTO TO QLDH;
GRANT EXECUTE ON DBMS_LOB TO QLDH;
GRANT EXECUTE ON UTL_RAW TO QLDH;
GRANT EXECUTE ON DBMS_RLS TO QLDH;
GRANT EXECUTE ON DBMS_FGA TO QLDH;
GRANT EXECUTE ON DBMS_SESSION TO QLDH;

-- Quyền Flashback cho phục hồi dữ liệu
GRANT FLASHBACK ANY TABLE TO QLDH;

-- ============================================================
-- 3. TẠO TABLESPACES
-- ============================================================

BEGIN
    EXECUTE IMMEDIATE 'CREATE TABLESPACE QLDH_DATA 
        DATAFILE ''qldh_data01.dbf'' SIZE 100M AUTOEXTEND ON NEXT 50M MAXSIZE 1G';
EXCEPTION WHEN OTHERS THEN
    IF SQLCODE = -1543 THEN NULL; ELSE RAISE; END IF;
END;
/

BEGIN
    EXECUTE IMMEDIATE 'CREATE TABLESPACE QLDH_INDEX 
        DATAFILE ''qldh_index01.dbf'' SIZE 50M AUTOEXTEND ON NEXT 25M MAXSIZE 500M';
EXCEPTION WHEN OTHERS THEN
    IF SQLCODE = -1543 THEN NULL; ELSE RAISE; END IF;
END;
/

ALTER USER QLDH DEFAULT TABLESPACE QLDH_DATA;
ALTER USER QLDH QUOTA UNLIMITED ON QLDH_DATA;
ALTER USER QLDH QUOTA UNLIMITED ON QLDH_INDEX;

-- ============================================================
-- 4. TẠO PROFILE 
-- ============================================================
-- Profile chỉ tạo được trong PDB, bỏ qua nếu lỗi
BEGIN
    EXECUTE IMMEDIATE 'DROP PROFILE QLDH_PROFILE CASCADE';
EXCEPTION WHEN OTHERS THEN NULL;
END;
/

BEGIN
    EXECUTE IMMEDIATE 'CREATE PROFILE QLDH_PROFILE LIMIT
        SESSIONS_PER_USER 5
        CONNECT_TIME 480
        IDLE_TIME 30
        FAILED_LOGIN_ATTEMPTS 5
        PASSWORD_LIFE_TIME 90
        PASSWORD_REUSE_TIME 365
        PASSWORD_LOCK_TIME 1/24
        PASSWORD_GRACE_TIME 7';
    DBMS_OUTPUT.PUT_LINE('Da tao QLDH_PROFILE');
EXCEPTION WHEN OTHERS THEN
    DBMS_OUTPUT.PUT_LINE('Profile da ton tai hoac loi: ' || SQLERRM);
END;
/

BEGIN
    EXECUTE IMMEDIATE 'ALTER USER QLDH PROFILE QLDH_PROFILE';
EXCEPTION WHEN OTHERS THEN NULL;
END;
/

-- ============================================================
-- 5. TẠO CONTEXT CHO VPD
-- ============================================================
CREATE OR REPLACE CONTEXT USER_CTX USING QLDH.PKG_SECURITY;

-- ============================================================
-- 6. TẠO ROLES (Optional - Roles phân quyền ở tầng Application)
-- Bỏ qua phần này vì phân quyền được quản lý ở C#
-- ============================================================
/*
BEGIN EXECUTE IMMEDIATE 'DROP ROLE ROLE_QUAN_LY'; EXCEPTION WHEN OTHERS THEN NULL; END;
/
BEGIN EXECUTE IMMEDIATE 'DROP ROLE ROLE_BAN_HANG'; EXCEPTION WHEN OTHERS THEN NULL; END;
/
BEGIN EXECUTE IMMEDIATE 'DROP ROLE ROLE_NHAN_VIEN_KHO'; EXCEPTION WHEN OTHERS THEN NULL; END;
/

CREATE ROLE ROLE_QUAN_LY;
CREATE ROLE ROLE_BAN_HANG;
CREATE ROLE ROLE_NHAN_VIEN_KHO;
*/

-- ============================================================
-- HOÀN TẤT!
-- ============================================================
SELECT 'SYSDBA Setup hoan tat! Chay tiep 02_QLDH.sql voi user QLDH' AS STATUS FROM DUAL;
