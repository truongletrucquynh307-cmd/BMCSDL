using Oracle.ManagedDataAccess.Client;
using Microsoft.Extensions.Configuration;

namespace QLDH.Services
{
    /// <summary>
    /// Service gọi các Oracle procedures cho VPD context và mã hóa Database-level
    /// </summary>
    public class OracleSecurityService
    {
        private readonly string _connectionString;

        public OracleSecurityService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("OracleDb")
                ?? throw new ArgumentNullException("OracleDb connection string not found");
        }

        /// <summary>
        /// Set VPD context khi user login (cho Oracle VPD policy)
        /// </summary>
        public void SetUserContext(string manv, string vaitro)
        {
            try
            {
                using var conn = new OracleConnection(_connectionString);
                conn.Open();

                using var cmd = new OracleCommand
                {
                    Connection = conn,
                    CommandText = "PKG_SECURITY.SET_USER_CONTEXT",
                    CommandType = System.Data.CommandType.StoredProcedure
                };

                cmd.Parameters.Add("p_manv", OracleDbType.Varchar2).Value = manv;
                cmd.Parameters.Add("p_vaitro", OracleDbType.NVarchar2).Value = vaitro;

                cmd.ExecuteNonQuery();
            }
            catch (OracleException ex)
            {
                // Log error but don't throw - VPD context is optional
                Console.WriteLine($"Warning: Could not set VPD context: {ex.Message}");
            }
        }

        /// <summary>
        /// Mã hóa DIACHI và SDT của nhân viên (gọi Oracle procedure)
        /// </summary>
        public void EncryptNhanVienData(string manv)
        {
            try
            {
                using var conn = new OracleConnection(_connectionString);
                conn.Open();

                using var cmd = new OracleCommand
                {
                    Connection = conn,
                    CommandText = "PKG_ENCRYPTION.ENCRYPT_NHANVIEN_DATA",
                    CommandType = System.Data.CommandType.StoredProcedure
                };

                cmd.Parameters.Add("p_manv", OracleDbType.Varchar2).Value = manv;
                cmd.ExecuteNonQuery();
            }
            catch (OracleException ex)
            {
                Console.WriteLine($"Warning: Could not encrypt employee data: {ex.Message}");
                // Don't throw - encryption is optional if Oracle package not installed
            }
        }

        /// <summary>
        /// Giải mã DIACHI của nhân viên
        /// </summary>
        public string? DecryptNhanVienDiaChi(string manv)
        {
            try
            {
                using var conn = new OracleConnection(_connectionString);
                conn.Open();

                using var cmd = new OracleCommand
                {
                    Connection = conn,
                    CommandText = "SELECT PKG_ENCRYPTION.DECRYPT_NHANVIEN_DIACHI(:manv) FROM DUAL",
                    CommandType = System.Data.CommandType.Text
                };

                cmd.Parameters.Add("manv", OracleDbType.Varchar2).Value = manv;

                var result = cmd.ExecuteScalar();
                return result?.ToString();
            }
            catch (OracleException ex)
            {
                Console.WriteLine($"Warning: Could not decrypt DIACHI: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Giải mã SDT của nhân viên
        /// </summary>
        public string? DecryptNhanVienSdt(string manv)
        {
            try
            {
                using var conn = new OracleConnection(_connectionString);
                conn.Open();

                using var cmd = new OracleCommand
                {
                    Connection = conn,
                    CommandText = "SELECT PKG_ENCRYPTION.DECRYPT_NHANVIEN_SDT(:manv) FROM DUAL",
                    CommandType = System.Data.CommandType.Text
                };

                cmd.Parameters.Add("manv", OracleDbType.Varchar2).Value = manv;

                var result = cmd.ExecuteScalar();
                return result?.ToString();
            }
            catch (OracleException ex)
            {
                Console.WriteLine($"Warning: Could not decrypt SDT: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Xem dữ liệu flashback (phục hồi dữ liệu)
        /// </summary>
        public List<Dictionary<string, object>> GetFlashbackData(string tableName, int minutesAgo = 30)
        {
            var result = new List<Dictionary<string, object>>();

            try
            {
                using var conn = new OracleConnection(_connectionString);
                conn.Open();

                var sql = $"SELECT * FROM {tableName} AS OF TIMESTAMP (SYSTIMESTAMP - INTERVAL '{minutesAgo}' MINUTE)";

                using var cmd = new OracleCommand(sql, conn);
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    var row = new Dictionary<string, object>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        row[reader.GetName(i)] = reader.IsDBNull(i) ? null! : reader.GetValue(i);
                    }
                    result.Add(row);
                }
            }
            catch (OracleException ex)
            {
                Console.WriteLine($"Flashback error: {ex.Message}");
            }

            return result;
        }

        /// <summary>
        /// Xem FGA audit logs
        /// </summary>
        public List<Dictionary<string, object>> GetFgaAuditLogs(string? policyName = null, int maxRows = 100)
        {
            var result = new List<Dictionary<string, object>>();

            try
            {
                using var conn = new OracleConnection(_connectionString);
                conn.Open();

                var sql = @"SELECT TIMESTAMP, DB_USER, OBJECT_SCHEMA, OBJECT_NAME, 
                                   POLICY_NAME, SQL_TEXT, SQL_BIND 
                            FROM DBA_FGA_AUDIT_TRAIL 
                            WHERE (:policy IS NULL OR POLICY_NAME = :policy)
                            ORDER BY TIMESTAMP DESC 
                            FETCH FIRST :maxRows ROWS ONLY";

                using var cmd = new OracleCommand(sql, conn);
                cmd.Parameters.Add("policy", OracleDbType.Varchar2).Value = 
                    string.IsNullOrEmpty(policyName) ? DBNull.Value : policyName;
                cmd.Parameters.Add("maxRows", OracleDbType.Int32).Value = maxRows;

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var row = new Dictionary<string, object>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        row[reader.GetName(i)] = reader.IsDBNull(i) ? null! : reader.GetValue(i);
                    }
                    result.Add(row);
                }
            }
            catch (OracleException ex)
            {
                Console.WriteLine($"FGA Audit error: {ex.Message}");
            }

            return result;
        }
    }
}
