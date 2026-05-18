using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace QLDH.Services
{
    public static class AesGcmHelper
    {
        // Encrypt: trả về Base64 của [nonce(12) | tag(16) | ciphertext]
        public static string Encrypt(string plainText, byte[] key)
        {
            if (plainText == null) throw new ArgumentNullException(nameof(plainText));
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (key.Length != 32 && key.Length != 16) // khuyến nghị 32 (256-bit)
                throw new ArgumentException("Key must be 16 or 32 bytes", nameof(key));

            byte[] nonce = new byte[12]; // 96-bit nonce recommended
            RandomNumberGenerator.Fill(nonce);

            byte[] plaintextBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] ciphertext = new byte[plaintextBytes.Length];
            byte[] tag = new byte[16];

            using (var aes = new AesGcm(key))
            {
                // no associatedData (null) here; add if needed
                aes.Encrypt(nonce, plaintextBytes, ciphertext, tag, null);
            }

            byte[] combined = new byte[nonce.Length + tag.Length + ciphertext.Length];
            Buffer.BlockCopy(nonce, 0, combined, 0, nonce.Length);
            Buffer.BlockCopy(tag, 0, combined, nonce.Length, tag.Length);
            Buffer.BlockCopy(ciphertext, 0, combined, nonce.Length + tag.Length, ciphertext.Length);

            return Convert.ToBase64String(combined);
        }

        // Decrypt: input là Base64 được tạo bởi Encrypt
        public static string Decrypt(string base64Input, byte[] key)
        {
            if (base64Input == null) throw new ArgumentNullException(nameof(base64Input));
            if (key == null) throw new ArgumentNullException(nameof(key));

            byte[] combined = Convert.FromBase64String(base64Input);

            if (combined.Length < 12 + 16) throw new ArgumentException("Invalid cipher text", nameof(base64Input));

            byte[] nonce = new byte[12];
            byte[] tag = new byte[16];
            byte[] ciphertext = new byte[combined.Length - nonce.Length - tag.Length];

            Buffer.BlockCopy(combined, 0, nonce, 0, nonce.Length);
            Buffer.BlockCopy(combined, nonce.Length, tag, 0, tag.Length);
            Buffer.BlockCopy(combined, nonce.Length + tag.Length, ciphertext, 0, ciphertext.Length);

            byte[] plaintext = new byte[ciphertext.Length];

            try
            {
                using (var aes = new AesGcm(key))
                {
                    aes.Decrypt(nonce, ciphertext, tag, plaintext, null);
                }
                return Encoding.UTF8.GetString(plaintext);
            }
            catch (CryptographicException)
            {
                // tag verification failed -> tampered or wrong key
                throw;
            }
        }
    }
}
