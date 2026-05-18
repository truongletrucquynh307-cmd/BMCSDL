using System;
using System.Security.Cryptography;
using System.Text;

namespace QLDH.Services
{
    /// <summary>
    /// Service giải mã dữ liệu NHANVIEN đã được mã hóa ở Oracle
    /// Dùng cùng key với PKG_ENCRYPTION trong Oracle
    /// </summary>
    public class NhanVienDecryptionService
    {
        // Key phải giống với Oracle PKG_ENCRYPTION (32 bytes)
        private readonly byte[] _aesKey;
        private readonly byte[] _sdtKey;

        public NhanVienDecryptionService()
        {
            // Key đồng bộ với Oracle: QLDH2024AES256KEYSECRET123456789
            _aesKey = Encoding.ASCII.GetBytes("QLDH2024AES256KEYSECRET123456789");
            // Key SDT: QLDH2024SDTRSAKEYSECRET123456789
            _sdtKey = Encoding.ASCII.GetBytes("QLDH2024SDTRSAKEYSECRET123456789");
        }

        /// <summary>
        /// Giải mã DIACHI (AES-256-CBC, Oracle format)
        /// Format: AES: + Base64(IV + Encrypted)
        /// </summary>
        public string DecryptDiaChi(string encryptedDiaChi)
        {
            if (string.IsNullOrEmpty(encryptedDiaChi))
                return encryptedDiaChi;

            // Nếu không có prefix AES: thì trả về nguyên gốc
            if (!encryptedDiaChi.StartsWith("AES:"))
                return encryptedDiaChi;

            try
            {
                // Bỏ prefix "AES:"
                string base64Data = encryptedDiaChi.Substring(4);
                byte[] combined = Convert.FromBase64String(base64Data);

                // Oracle format: IV (16 bytes) + Encrypted data
                byte[] iv = new byte[16];
                byte[] encrypted = new byte[combined.Length - 16];
                Buffer.BlockCopy(combined, 0, iv, 0, 16);
                Buffer.BlockCopy(combined, 16, encrypted, 0, encrypted.Length);

                using var aes = Aes.Create();
                aes.Key = _aesKey;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using var decryptor = aes.CreateDecryptor();
                byte[] decrypted = decryptor.TransformFinalBlock(encrypted, 0, encrypted.Length);

                return Encoding.UTF8.GetString(decrypted);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Decrypt DIACHI error: {ex.Message}");
                return encryptedDiaChi; // Trả về nguyên gốc nếu lỗi
            }
        }

        /// <summary>
        /// Giải mã SDT (RSA simulated = AES-256-CBC với key khác)
        /// Format: RSA: + Base64(IV + Encrypted)
        /// </summary>
        public string DecryptSDT(string encryptedSDT)
        {
            if (string.IsNullOrEmpty(encryptedSDT))
                return encryptedSDT;

            // Nếu không có prefix RSA: thì trả về nguyên gốc
            if (!encryptedSDT.StartsWith("RSA:"))
                return encryptedSDT;

            try
            {
                // Bỏ prefix "RSA:"
                string base64Data = encryptedSDT.Substring(4);
                byte[] combined = Convert.FromBase64String(base64Data);

                // Oracle format: IV (16 bytes) + Encrypted data
                byte[] iv = new byte[16];
                byte[] encrypted = new byte[combined.Length - 16];
                Buffer.BlockCopy(combined, 0, iv, 0, 16);
                Buffer.BlockCopy(combined, 16, encrypted, 0, encrypted.Length);

                using var aes = Aes.Create();
                aes.Key = _sdtKey;
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                using var decryptor = aes.CreateDecryptor();
                byte[] decrypted = decryptor.TransformFinalBlock(encrypted, 0, encrypted.Length);

                return Encoding.UTF8.GetString(decrypted);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Decrypt SDT error: {ex.Message}");
                return encryptedSDT; // Trả về nguyên gốc nếu lỗi
            }
        }
    }
}
