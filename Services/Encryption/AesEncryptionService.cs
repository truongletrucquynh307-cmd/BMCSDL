using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace QLDH.Services.Encryption
{
    /// <summary>
    /// Dịch vụ mã hóa AES-256-GCM cho dữ liệu khách hàng (TENKH)
    /// </summary>
    public class AesEncryptionService : IEncryptionService
    {
        private readonly byte[] _key;
        private const string ENCRYPTED_PREFIX = "AES:";

        public AesEncryptionService(IConfiguration config)
        {
            var keyBase64 = config["Crypto:AesKeyBase64"];
            if (string.IsNullOrEmpty(keyBase64))
                throw new InvalidOperationException("Missing Crypto:AesKeyBase64 in configuration.");

            _key = Convert.FromBase64String(keyBase64);
            if (_key.Length != 32)
                throw new InvalidOperationException("AES key must be 32 bytes (256-bit).");
        }

        public AesEncryptionService(byte[] key)
        {
            if (key == null || key.Length != 32)
                throw new ArgumentException("AES key must be 32 bytes (256-bit).", nameof(key));
            _key = key;
        }

        public string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return plainText;

            // Nếu đã được mã hóa rồi thì không mã hóa lại
            if (IsEncrypted(plainText))
                return plainText;

            byte[] nonce = new byte[12]; // 96-bit nonce
            RandomNumberGenerator.Fill(nonce);

            byte[] plaintextBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] ciphertext = new byte[plaintextBytes.Length];
            byte[] tag = new byte[16]; // 128-bit tag

            using (var aes = new AesGcm(_key, 16))
            {
                aes.Encrypt(nonce, plaintextBytes, ciphertext, tag, null);
            }

            // Combine: nonce(12) + tag(16) + ciphertext
            byte[] combined = new byte[nonce.Length + tag.Length + ciphertext.Length];
            Buffer.BlockCopy(nonce, 0, combined, 0, nonce.Length);
            Buffer.BlockCopy(tag, 0, combined, nonce.Length, tag.Length);
            Buffer.BlockCopy(ciphertext, 0, combined, nonce.Length + tag.Length, ciphertext.Length);

            return ENCRYPTED_PREFIX + Convert.ToBase64String(combined);
        }

        public string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
                return cipherText;

            // Nếu không có prefix thì trả về nguyên gốc (chưa được mã hóa)
            if (!IsEncrypted(cipherText))
                return cipherText;

            string base64Data = cipherText.Substring(ENCRYPTED_PREFIX.Length);
            byte[] combined = Convert.FromBase64String(base64Data);

            if (combined.Length < 28) // 12 + 16 minimum
                throw new ArgumentException("Invalid cipher text", nameof(cipherText));

            byte[] nonce = new byte[12];
            byte[] tag = new byte[16];
            byte[] ciphertextBytes = new byte[combined.Length - 28];

            Buffer.BlockCopy(combined, 0, nonce, 0, 12);
            Buffer.BlockCopy(combined, 12, tag, 0, 16);
            Buffer.BlockCopy(combined, 28, ciphertextBytes, 0, ciphertextBytes.Length);

            byte[] plaintext = new byte[ciphertextBytes.Length];

            using (var aes = new AesGcm(_key, 16))
            {
                aes.Decrypt(nonce, ciphertextBytes, tag, plaintext, null);
            }

            return Encoding.UTF8.GetString(plaintext);
        }

        public bool IsEncrypted(string text)
        {
            return !string.IsNullOrEmpty(text) && text.StartsWith(ENCRYPTED_PREFIX);
        }
    }
}
