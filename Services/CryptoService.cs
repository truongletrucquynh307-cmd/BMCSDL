using System;
using Microsoft.Extensions.Configuration;

namespace QLDH.Services
{
    public class CryptoService
    {
        private readonly byte[] _key;

        public CryptoService(IConfiguration config)
        {
            var keyBase64 = config["Crypto:AesKeyBase64"];
            if (string.IsNullOrEmpty(keyBase64))
                throw new InvalidOperationException("Missing Crypto:AesKeyBase64 in configuration.");

            _key = Convert.FromBase64String(keyBase64);
            if (_key.Length != 32 && _key.Length != 16)
                throw new InvalidOperationException("Aes key must be 16 or 32 bytes (base64).");
        }

        public string Encrypt(string plain) => AesGcmHelper.Encrypt(plain, _key);
        public string Decrypt(string cipher) => AesGcmHelper.Decrypt(cipher, _key);
    }
}
