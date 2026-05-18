// Tool để generate encrypted passwords
// Không biên dịch cùng project - chỉ để tham khảo
// Có thể chạy bằng LINQPad hoặc .NET Interactive

#if STANDALONE_TOOL
using System;
using System.Security.Cryptography;
using System.Text;

namespace QLDH.Tools
{
    /// <summary>
    /// Tool sinh mật khẩu mã hóa AES-256-GCM cho QLDH.sql
    /// </summary>
    public static class PasswordGenerator
    {
        // Key từ appsettings.json
        static readonly byte[] Key = Convert.FromBase64String("LZ1Z8w2E8VMT6OYu3YcR6o7SkZb+Kx/ivB3pY7e1T5g=");

        public static void Generate()
        {
            Console.WriteLine("=== ENCRYPTED PASSWORDS FOR QLDH.sql ===\n");
            
            var passwords = new[] {
                ("NV001", "Admin123@"),
                ("NV002", "Nhanvien123@"),
                ("NV003", "Nhanvien123@"),
                ("NV004", "Nhanvien123@"),
                ("NV005", "Nhanvien123@")
            };

            foreach (var (manv, pass) in passwords)
            {
                string encrypted = Encrypt(pass);
                Console.WriteLine($"-- {manv}: {pass}");
                Console.WriteLine($"INSERT INTO TAIKHOAN VALUES ('{manv}', '{encrypted}', N'...');");
                Console.WriteLine();
            }
        }

        public static string Encrypt(string plainText)
        {
            byte[] nonce = new byte[12];
            RandomNumberGenerator.Fill(nonce);

            byte[] plaintextBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] ciphertext = new byte[plaintextBytes.Length];
            byte[] tag = new byte[16];

            using (var aes = new AesGcm(Key, 16))
            {
                aes.Encrypt(nonce, plaintextBytes, ciphertext, tag, null);
            }

            byte[] combined = new byte[nonce.Length + tag.Length + ciphertext.Length];
            Buffer.BlockCopy(nonce, 0, combined, 0, nonce.Length);
            Buffer.BlockCopy(tag, 0, combined, nonce.Length, tag.Length);
            Buffer.BlockCopy(ciphertext, 0, combined, nonce.Length + tag.Length, ciphertext.Length);

            return Convert.ToBase64String(combined);
        }
    }
}
#endif
