using Oracle.ManagedDataAccess.Client;
using Microsoft.Extensions.Configuration;
using QLDH.Models;

namespace QLDH.Services
{
    public class OracleAuthService
    {
        private readonly IConfiguration _config;
        private readonly string _connStr;

        public OracleAuthService(IConfiguration config)
        {
            _config = config;
            _connStr = _config.GetConnectionString("OracleDb");
        }

        public TaiKhoan GetTaiKhoanByMaNV(string maNV)
        {
            using var conn = new OracleConnection(_connStr);
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT MANV, MATKHAU, VAITRO FROM TAIKHOAN WHERE MANV = :manv";
            cmd.Parameters.Add(new OracleParameter("manv", maNV));

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new TaiKhoan
                {
                    MANV = reader.GetString(0),
                    MATKHAU = reader.IsDBNull(1) ? null : reader.GetString(1),
                    VAITRO = reader.IsDBNull(2) ? null : reader.GetString(2)
                };
            }
            return null;
        }

        // ✅ Cập nhật mật khẩu
        public void UpdatePassword(string manv, string newPassword)
        {
            using var conn = new OracleConnection(_connStr);
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "UPDATE TAIKHOAN SET MATKHAU = :newPassword WHERE MANV = :manv";
            cmd.Parameters.Add(new OracleParameter("newPassword", newPassword));
            cmd.Parameters.Add(new OracleParameter("manv", manv));
            cmd.ExecuteNonQuery();
        }

        //Đăng ký -Register
        public void CreateNhanVien(string manv, string tennv, string chucvu, string diachi, string sdt, string email)
        {
            using var conn = new OracleConnection(_connStr);
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"INSERT INTO NHANVIEN (MANV, TENNV, CHUCVU, DIACHI, SDT, EMAIL) 
                        VALUES (:manv, :tennv, :chucvu, :diachi, :sdt, :email)";
            cmd.Parameters.Add(new OracleParameter("manv", manv));
            cmd.Parameters.Add(new OracleParameter("tennv", tennv));
            cmd.Parameters.Add(new OracleParameter("chucvu", chucvu));
            cmd.Parameters.Add(new OracleParameter("diachi", diachi));
            cmd.Parameters.Add(new OracleParameter("sdt", sdt));
            cmd.Parameters.Add(new OracleParameter("email", email));
            cmd.ExecuteNonQuery();
        }

        public void CreateTaiKhoan(string manv, string password, string vaitro)
        {
            using var conn = new OracleConnection(_connStr);
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "INSERT INTO TAIKHOAN (MANV, MATKHAU, VAITRO) VALUES (:manv, :matkhau, :vaitro)";
            cmd.Parameters.Add(new OracleParameter("manv", manv));
            cmd.Parameters.Add(new OracleParameter("matkhau", password));
            cmd.Parameters.Add(new OracleParameter("vaitro", vaitro));

            cmd.ExecuteNonQuery();
        }

        // ✅ Lấy tên nhân viên theo mã
        public string GetTenNVByMaNV(string maNV)
        {
            using var conn = new OracleConnection(_connStr);
            conn.Open();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT TENNV FROM NHANVIEN WHERE MANV = :manv";
            cmd.Parameters.Add(new OracleParameter("manv", maNV));

            var result = cmd.ExecuteScalar();
            return result?.ToString() ?? "";
        }

    }
}
