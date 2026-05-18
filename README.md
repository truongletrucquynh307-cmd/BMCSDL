### **BƯỚC 1: Tạo User QLDH**

1. **Mở SQL Developer**
2. **Kết nối với DETAI13** (connection đã có - username: `sys as sysdba`, password: `sys`)
3. **Mở file:** `setup_user_qldh.sql` (trong thư mục dự án)
4. **Nhấn F5** để chạy script
5. **Kiểm tra kết quả:** Phải thấy user `QLDH` với status `OPEN`

---

### **BƯỚC 2: Tạo Connection mới cho QLDH**

Trong SQL Developer, tạo connection mới:

```
Name:              QLDH_Connection
Database Type:     Oracle
Username:          QLDH
Password:          123
Connection Type:   Basic
Hostname:          localhost
Port:              1521
○ Service name:    freepdb1   ← Quan trọng!
```

**Click Test** → Phải thấy "Status: Success" ✅

**Click Save** → **Click Connect**

---

### **BƯỚC 3: Chạy Script Database**

1. **Kết nối với QLDH_Connection** (vừa tạo ở bước 2)
2. **Mở file:** `QLDH.sql`
3. **XÓA 9 DÒNG ĐẦU** (từ dòng 1 đến dòng 9):
   ```sql
   -- Tạo user
   CREATE USER QLDH IDENTIFIED BY 123
   GRANT CREATE SESSION TO QLDH
   GRANT CREATE TABLE TO QLDH
   ALTER USER QLDH QUOTA 100M ON USERS;
   ```
   **Lý do:** User đã tạo rồi ở Bước 1

4. **Copy từ dòng 11 trở đi** (bắt đầu từ `-- Xóa bảng nếu đã tồn tại`)
5. **Paste vào SQL Worksheet**
6. **Nhấn F5** (Run Script) - QUAN TRỌNG: Phải dùng F5, không dùng Ctrl+Enter!

---

### **BƯỚC 4: Verify Database**

Chạy các query sau để kiểm tra:

```sql
-- 1. Kiểm tra tables (phải có 10 tables)
SELECT table_name FROM user_tables ORDER BY table_name;

-- 2. Kiểm tra dữ liệu mẫu
SELECT COUNT(*) AS so_nhan_vien FROM NHANVIEN;     -- Phải có ít nhất 3
SELECT COUNT(*) AS so_khach_hang FROM KHACHHANG;   -- Phải có ít nhất 5
SELECT COUNT(*) AS so_san_pham FROM SANPHAM;       -- Phải có ít nhất 10
SELECT COUNT(*) AS so_don_hang FROM DONHANG;       -- Phải có ít nhất 5

-- 3. Kiểm tra tài khoản
SELECT MANV, VAITRO FROM TAIKHOAN;
```

**Kết quả mong đợi:**
- 10 tables: CHITIETDONHANG, DONHANG, HOADON, KHACHHANG, LOAIKH, LOAISP, NCC, NHANVIEN, SANPHAM, TAIKHOAN
- Có dữ liệu mẫu trong các bảng

---

## 🎯 BƯỚC 5: Chạy Ứng dụng

### **Cách 1: Dùng Visual Studio 2022**

1. Mở file `QLDH.sln`
2. Nhấn **F5** (hoặc Ctrl + F5)
3. Trình duyệt sẽ tự động mở: `https://localhost:7212`

### **Cách 2: Dùng Command Line**

```cmd
cd "d:\Quynh\Quản lý đơn hàng\QLDH"
dotnet run --launch-profile https
```

---

## 🔐 BƯỚC 6: Test Đăng ký và Login
### **Đăng ký tài khoản mới:**

1. Mở: `https://localhost:7212/Account/Register`
2. Điền thông tin:
   - **Mã NV:** `NV001` (hoặc NV002, NV003 - phải có trong DB)
   - **Tên:** Nguyen Van A
   - **Chức vụ:** Quản lý
   - **Địa chỉ:** Hà Nội
   - **SĐT:** 0123456789
   - **Email:** nva@email.com
   - **Mật khẩu:** 123456
   - **Nhập lại mật khẩu:** 123456
3. Click **Đăng ký**

### **Đăng nhập:**

1. Mở: `https://localhost:7212/Account/Login`
2. Nhập:
   - **Mã NV:** NV001
   - **Mật khẩu:** 123456
3. Click **Đăng nhập**

---

## 🎉 HOÀN TẤT!

Nếu thấy dashboard sau khi login → **Dự án đã chạy thành công!** 🚀

---

## 📊 Các URL có thể truy cập:

| URL | Chức năng |
|-----|-----------|
| `/` | Trang chủ |
| `/Account/Login` | Đăng nhập |
| `/Account/Register` | Đăng ký |
| `/Account/Dashboard` | Dashboard |
| `/Account/ChangePass` | Đổi mật khẩu |
| `/KhachHang/IndexKH` | Quản lý khách hàng |
| `/SanPham/IndexSP` | Quản lý sản phẩm |
| `/LoaiKhachHang/IndexLoaiKH` | Loại khách hàng |
| `/LoaiSanPham/IndexLSP` | Loại sản phẩm |
| `/NhaCungCap/IndexNCC` | Nhà cung cấp |
| `/NhanVien/IndexNV` | Nhân viên |
| `/DonHang/IndexDH` | Đơn hàng |
| `/HoaDon/IndexHD` | Hóa đơn |

---

## 🐛 Xử lý Lỗi

### **Lỗi: ORA-12514**
→ Service name sai. Kiểm tra lại `lsnrctl status` và sửa `appsettings.json`

### **Lỗi: ORA-01017 (Invalid username/password)**
→ User QLDH chưa được tạo. Quay lại Bước 1.

### **Lỗi: ORA-00942 (Table not found)**
→ Tables chưa được tạo. Quay lại Bước 3.

### **Lỗi: Connection refused**
→ Oracle service chưa chạy. Check `services.msc` → Tìm OracleService → Start
