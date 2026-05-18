using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace QLDH.Services.Encryption
{
    /// <summary>
    /// Dịch vụ mã hóa RSA-2048 cho dữ liệu khách hàng (DIACHI)
    /// RSA chỉ mã hóa được dữ liệu nhỏ (< 245 bytes với RSA-2048)
    /// </summary>
    public class RsaEncryptionService : IEncryptionService
    {
        private readonly RSA _rsa;
        private readonly ILogger<RsaEncryptionService>? _logger;
        private const string ENCRYPTED_PREFIX = "RSA:";
        private const int KEY_SIZE = 2048;

        // Constructor với key từ configuration
        public RsaEncryptionService(IConfiguration config, ILogger<RsaEncryptionService>? logger = null)
        {
            _logger = logger;
            _rsa = RSA.Create(KEY_SIZE);

            // Nếu có key trong config thì load, không thì tạo mới
            var privateKeyBase64 = config["Crypto:RsaPrivateKey"];
            if (!string.IsNullOrEmpty(privateKeyBase64))
            {
                try
                {
                    byte[] privateKeyBytes = Convert.FromBase64String(privateKeyBase64);
                    _rsa.ImportRSAPrivateKey(privateKeyBytes, out _);
                    _logger?.LogInformation("RSA key loaded from configuration");
                }
                catch (Exception ex)
                {
                    _logger?.LogWarning(ex, "Failed to load RSA key from config, generating new key pair");
                    GenerateAndLogKeys();
                }
            }
            else
            {
                _logger?.LogInformation("No RSA key in config, generating new key pair");
                GenerateAndLogKeys();
            }
        }

        // Constructor với RSA key pair đã có
        public RsaEncryptionService(RSA rsa)
        {
            _rsa = rsa ?? throw new ArgumentNullException(nameof(rsa));
        }

        // Constructor với private key bytes
        public RsaEncryptionService(byte[] privateKey)
        {
            _rsa = RSA.Create();
            _rsa.ImportRSAPrivateKey(privateKey, out _);
        }

        private void GenerateAndLogKeys()
        {
            // Log keys để có thể lưu vào config sau
            var privateKey = Convert.ToBase64String(_rsa.ExportRSAPrivateKey());
            var publicKey = Convert.ToBase64String(_rsa.ExportRSAPublicKey());

            _logger?.LogWarning("Generated new RSA key pair. Add to appsettings.json:");
            _logger?.LogWarning("\"RsaPrivateKey\": \"{PrivateKey}\"", privateKey);
            _logger?.LogWarning("\"RsaPublicKey\": \"{PublicKey}\"", publicKey);
        }

        public string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return plainText;

            if (IsEncrypted(plainText))
                return plainText;

            byte[] dataBytes = Encoding.UTF8.GetBytes(plainText);

            // RSA-2048 với OAEP SHA256 chỉ mã hóa được tối đa 190 bytes
            int maxSize = (_rsa.KeySize / 8) - 66; // OAEP SHA256 overhead
            if (dataBytes.Length > maxSize)
            {
                throw new ArgumentException(
                    $"Data too large for RSA encryption. Max: {maxSize} bytes, Got: {dataBytes.Length} bytes. Use HybridEncryptionService instead.",
                    nameof(plainText));
            }

            byte[] encryptedBytes = _rsa.Encrypt(dataBytes, RSAEncryptionPadding.OaepSHA256);
            return ENCRYPTED_PREFIX + Convert.ToBase64String(encryptedBytes);
        }

        public string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
                return cipherText;

            if (!IsEncrypted(cipherText))
                return cipherText;

            string base64Data = cipherText.Substring(ENCRYPTED_PREFIX.Length);
            byte[] encryptedBytes = Convert.FromBase64String(base64Data);
            byte[] decryptedBytes = _rsa.Decrypt(encryptedBytes, RSAEncryptionPadding.OaepSHA256);

            return Encoding.UTF8.GetString(decryptedBytes);
        }

        public bool IsEncrypted(string text)
        {
            return !string.IsNullOrEmpty(text) && text.StartsWith(ENCRYPTED_PREFIX);
        }

        /// <summary>
        /// Lấy public key để lưu vào database hoặc chia sẻ
        /// </summary>
        public string GetPublicKeyBase64()
        {
            return Convert.ToBase64String(_rsa.ExportRSAPublicKey());
        }

        /// <summary>
        /// Lấy private key để backup
        /// </summary>
        public string GetPrivateKeyBase64()
        {
            return Convert.ToBase64String(_rsa.ExportRSAPrivateKey());
        }

        /// <summary>
        /// Tạo RSA key pair mới và trả về dạng Base64
        /// </summary>
        public static (string PublicKey, string PrivateKey) GenerateKeyPair()
        {
            using var rsa = RSA.Create(KEY_SIZE);
            return (
                Convert.ToBase64String(rsa.ExportRSAPublicKey()),
                Convert.ToBase64String(rsa.ExportRSAPrivateKey())
            );
        }

        public void Dispose()
        {
            _rsa?.Dispose();
        }
    }
}
