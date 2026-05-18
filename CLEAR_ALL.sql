-- ============================================================
-- SCRIPT CLEAR ALL OBJECTS - QLDH USER
-- Chạy với user QLDH (không cần SYSDBA)
-- ============================================================

-- Tắt constraint checking tạm thời
SET SERVEROUTPUT ON;

-- ============================================================
-- 1. XÓA TẤT CẢ TRIGGERS
-- ============================================================
BEGIN
    FOR t IN (SELECT trigger_name FROM user_triggers) LOOP
        EXECUTE IMMEDIATE 'DROP TRIGGER ' || t.trigger_name;
        DBMS_OUTPUT.PUT_LINE('Dropped trigger: ' || t.trigger_name);
    END LOOP;
END;
/

-- ============================================================
-- 2. XÓA TẤT CẢ PACKAGES
-- ============================================================
BEGIN
    FOR p IN (SELECT object_name FROM user_objects WHERE object_type = 'PACKAGE') LOOP
        EXECUTE IMMEDIATE 'DROP PACKAGE ' || p.object_name;
        DBMS_OUTPUT.PUT_LINE('Dropped package: ' || p.object_name);
    END LOOP;
END;
/

-- ============================================================
-- 3. XÓA TẤT CẢ FUNCTIONS
-- ============================================================
BEGIN
    FOR f IN (SELECT object_name FROM user_objects WHERE object_type = 'FUNCTION') LOOP
        EXECUTE IMMEDIATE 'DROP FUNCTION ' || f.object_name;
        DBMS_OUTPUT.PUT_LINE('Dropped function: ' || f.object_name);
    END LOOP;
END;
/

-- ============================================================
-- 4. XÓA TẤT CẢ PROCEDURES
-- ============================================================
BEGIN
    FOR p IN (SELECT object_name FROM user_objects WHERE object_type = 'PROCEDURE') LOOP
        EXECUTE IMMEDIATE 'DROP PROCEDURE ' || p.object_name;
        DBMS_OUTPUT.PUT_LINE('Dropped procedure: ' || p.object_name);
    END LOOP;
END;
/

-- ============================================================
-- 5. XÓA TẤT CẢ VIEWS
-- ============================================================
BEGIN
    FOR v IN (SELECT view_name FROM user_views) LOOP
        EXECUTE IMMEDIATE 'DROP VIEW ' || v.view_name;
        DBMS_OUTPUT.PUT_LINE('Dropped view: ' || v.view_name);
    END LOOP;
END;
/

-- ============================================================
-- 6. XÓA TẤT CẢ TABLES (theo thứ tự để tránh FK constraint)
-- ============================================================
-- Xóa theo thứ tự: các bảng con trước, bảng cha sau

-- Cách 1: Xóa thủ công theo thứ tự đúng
DROP TABLE HOADON CASCADE CONSTRAINTS;
DROP TABLE CHITIETDONHANG CASCADE CONSTRAINTS;
DROP TABLE DONHANG CASCADE CONSTRAINTS;
DROP TABLE SANPHAM CASCADE CONSTRAINTS;
DROP TABLE LOAISP CASCADE CONSTRAINTS;
DROP TABLE NCC CASCADE CONSTRAINTS;
DROP TABLE KHACHHANG CASCADE CONSTRAINTS;
DROP TABLE LOAIKH CASCADE CONSTRAINTS;
DROP TABLE TAIKHOAN CASCADE CONSTRAINTS;
DROP TABLE NHANVIEN CASCADE CONSTRAINTS;
DROP TABLE AUDIT_LOG CASCADE CONSTRAINTS;
DROP TABLE CRYPTO_KEYS CASCADE CONSTRAINTS;

-- Cách 2: Xóa tự động tất cả bảng còn lại (nếu có)
BEGIN
    FOR t IN (SELECT table_name FROM user_tables) LOOP
        BEGIN
            EXECUTE IMMEDIATE 'DROP TABLE ' || t.table_name || ' CASCADE CONSTRAINTS';
            DBMS_OUTPUT.PUT_LINE('Dropped table: ' || t.table_name);
        EXCEPTION
            WHEN OTHERS THEN
                DBMS_OUTPUT.PUT_LINE('Error dropping table ' || t.table_name || ': ' || SQLERRM);
        END;
    END LOOP;
END;
/

-- ============================================================
-- 7. XÓA TẤT CẢ SEQUENCES
-- ============================================================
-- Cách 1: Xóa thủ công các sequence đã biết
DROP SEQUENCE SEQ_AUDIT_LOG;
DROP SEQUENCE SEQ_NHANVIEN;
DROP SEQUENCE SEQ_KHACHHANG;
DROP SEQUENCE SEQ_SANPHAM;
DROP SEQUENCE SEQ_DONHANG;
DROP SEQUENCE SEQ_CHITIETDONHANG;
DROP SEQUENCE SEQ_HOADON;
DROP SEQUENCE SEQ_NCC;
DROP SEQUENCE SEQ_LOAISP;
DROP SEQUENCE SEQ_LOAIKH;
DROP SEQUENCE SEQ_TAIKHOAN;
DROP SEQUENCE SEQ_CRYPTO_KEYS;

-- Cách 2: Xóa tự động tất cả sequence còn lại (nếu có)
BEGIN
    FOR s IN (SELECT sequence_name FROM user_sequences) LOOP
        BEGIN
            EXECUTE IMMEDIATE 'DROP SEQUENCE ' || s.sequence_name;
            DBMS_OUTPUT.PUT_LINE('Dropped sequence: ' || s.sequence_name);
        EXCEPTION
            WHEN OTHERS THEN
                DBMS_OUTPUT.PUT_LINE('Error dropping sequence ' || s.sequence_name || ': ' || SQLERRM);
        END;
    END LOOP;
END;
/

-- ============================================================
-- 8. XÓA TẤT CẢ INDEXES (nếu còn)
-- ============================================================
BEGIN
    FOR i IN (SELECT index_name FROM user_indexes WHERE index_type != 'LOB') LOOP
        BEGIN
            EXECUTE IMMEDIATE 'DROP INDEX ' || i.index_name;
            DBMS_OUTPUT.PUT_LINE('Dropped index: ' || i.index_name);
        EXCEPTION
            WHEN OTHERS THEN
                NULL; -- Bỏ qua lỗi nếu index đã bị xóa cùng table
        END;
    END LOOP;
END;
/

-- ============================================================
-- 9. KIỂM TRA KẾT QUẢ
-- ============================================================
SELECT 'Tables: ' || COUNT(*) FROM user_tables;
SELECT 'Triggers: ' || COUNT(*) FROM user_triggers;
SELECT 'Packages: ' || COUNT(*) FROM user_objects WHERE object_type = 'PACKAGE';
SELECT 'Functions: ' || COUNT(*) FROM user_objects WHERE object_type = 'FUNCTION';
SELECT 'Procedures: ' || COUNT(*) FROM user_objects WHERE object_type = 'PROCEDURE';
SELECT 'Views: ' || COUNT(*) FROM user_views;
SELECT 'Sequences: ' || COUNT(*) FROM user_sequences;
SELECT 'Indexes: ' || COUNT(*) FROM user_indexes;

COMMIT;

-- ============================================================
-- KẾT THÚC - Database đã được clear
-- Bây giờ có thể chạy lại QLDH.sql để tạo mới
-- ============================================================
