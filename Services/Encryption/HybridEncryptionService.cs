using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;

namespace QLDH.Services.Encryption
{
    /// <summary>
    /// Dịch vụ mã hóa Hybrid (AES + RSA) cho dữ liệu khách hàng (SDT)
    /// Flow: 
    /// 1. Generate random AES key (256-bit)
    /// 2. Encrypt data using AES-GCM
    /// 3. Encrypt AES key using RSA public key
    /// 4. Store: HYB:base64(RSA_encrypted_AES_key)|base64(AES_encrypted_data)
    /// </summary>
    public class HybridEncryptionService : IEncryptionService
    {
        private readonly RsaEncryptionService _rsaService;
        private readonly ILogger<HybridEncryptionService>? _logger;
        private const string ENCRYPTED_PREFIX = "HYB:";
        private const string SEPARATOR = "|";
        private const int AES_KEY_SIZE = 32; // 256-bit

        public HybridEncryptionService(RsaEncryptionService rsaService, ILogger<HybridEncryptionService>? logger = null)
        {
            _rsaService = rsaService ?? throw new ArgumentNullException(nameof(rsaService));
            _logger = logger;
        }

        public string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return plainText;

            if (IsEncrypted(plainText))
                return plainText;

            try
            {
                // Step 1: Generate random AES key
                byte[] aesKey = new byte[AES_KEY_SIZE];
                RandomNumberGenerator.Fill(aesKey);

                // Step 2: Encrypt data using AES-GCM
                string aesEncryptedData = EncryptWithAes(plainText, aesKey);

                // Step 3: Encrypt AES key using RSA (qua service)
                // Chuyển AES key thành string base64 để encrypt
                string aesKeyBase64 = Convert.ToBase64String(aesKey);
                string encryptedAesKey = _rsaService.Encrypt(aesKeyBase64);
                
                // Loại bỏ prefix RSA: nếu có
                if (encryptedAesKey.StartsWith("RSA:"))
                    encryptedAesKey = encryptedAesKey.Substring(4);

                // Step 4: Combine: HYB:encrypted_aes_key|aes_encrypted_data
                return $"{ENCRYPTED_PREFIX}{encryptedAesKey}{SEPARATOR}{aesEncryptedData}";
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Hybrid encryption failed");
                throw;
            }
        }

        public string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
                return cipherText;

            if (!IsEncrypted(cipherText))
                return cipherText;

            try
            {
                // Remove prefix
                string data = cipherText.Substring(ENCRYPTED_PREFIX.Length);

                // Split by separator
                int separatorIndex = data.IndexOf(SEPARATOR);
                if (separatorIndex < 0)
                    throw new ArgumentException("Invalid hybrid encrypted data format", nameof(cipherText));

                string encryptedAesKey = data.Substring(0, separatorIndex);
                string aesEncryptedData = data.Substring(separatorIndex + 1);

                // Step 1: Decrypt AES key using RSA
                // Thêm prefix RSA: để decrypt
                string decryptedAesKeyBase64 = _rsaService.Decrypt("RSA:" + encryptedAesKey);
                byte[] aesKey = Convert.FromBase64String(decryptedAesKeyBase64);

                // Step 2: Decrypt data using AES
                return DecryptWithAes(aesEncryptedData, aesKey);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Hybrid decryption failed");
                throw;
            }
        }

        public bool IsEncrypted(string text)
        {
            return !string.IsNullOrEmpty(text) && text.StartsWith(ENCRYPTED_PREFIX);
        }

        private static string EncryptWithAes(string plainText, byte[] key)
        {
            byte[] nonce = new byte[12];
            RandomNumberGenerator.Fill(nonce);

            byte[] plaintextBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] ciphertext = new byte[plaintextBytes.Length];
            byte[] tag = new byte[16];

            using (var aes = new AesGcm(key, 16))
            {
                aes.Encrypt(nonce, plaintextBytes, ciphertext, tag, null);
            }

            // Combine: nonce(12) + tag(16) + ciphertext
            byte[] combined = new byte[nonce.Length + tag.Length + ciphertext.Length];
            Buffer.BlockCopy(nonce, 0, combined, 0, nonce.Length);
            Buffer.BlockCopy(tag, 0, combined, nonce.Length, tag.Length);
            Buffer.BlockCopy(ciphertext, 0, combined, nonce.Length + tag.Length, ciphertext.Length);

            return Convert.ToBase64String(combined);
        }

        private static string DecryptWithAes(string base64Data, byte[] key)
        {
            byte[] combined = Convert.FromBase64String(base64Data);

            if (combined.Length < 28)
                throw new ArgumentException("Invalid AES encrypted data", nameof(base64Data));

            byte[] nonce = new byte[12];
            byte[] tag = new byte[16];
            byte[] ciphertextBytes = new byte[combined.Length - 28];

            Buffer.BlockCopy(combined, 0, nonce, 0, 12);
            Buffer.BlockCopy(combined, 12, tag, 0, 16);
            Buffer.BlockCopy(combined, 28, ciphertextBytes, 0, ciphertextBytes.Length);

            byte[] plaintext = new byte[ciphertextBytes.Length];

            using (var aes = new AesGcm(key, 16))
            {
                aes.Decrypt(nonce, ciphertextBytes, tag, plaintext, null);
            }

            return Encoding.UTF8.GetString(plaintext);
        }
    }
}
