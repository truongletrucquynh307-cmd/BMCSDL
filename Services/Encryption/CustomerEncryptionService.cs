using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace QLDH.Services.Encryption
{
    /// <summary>
    /// Dịch vụ tổng hợp mã hóa cho Khách hàng
    /// - TENKH: AES-256-GCM (đối xứng)
    /// - DIACHI: RSA-2048 (bất đối xứng)
    /// - SDT: Hybrid AES+RSA
    /// </summary>
    public class CustomerEncryptionService
    {
        private readonly AesEncryptionService _aesService;
        private readonly RsaEncryptionService _rsaService;
        private readonly HybridEncryptionService _hybridService;

        public CustomerEncryptionService(IConfiguration config, ILogger<CustomerEncryptionService>? logger = null)
        {
            _aesService = new AesEncryptionService(config);
            _rsaService = new RsaEncryptionService(config, null);
            _hybridService = new HybridEncryptionService(_rsaService, null);
        }

        public CustomerEncryptionService(
            AesEncryptionService aesService,
            RsaEncryptionService rsaService,
            HybridEncryptionService hybridService)
        {
            _aesService = aesService;
            _rsaService = rsaService;
            _hybridService = hybridService;
        }

        #region TENKH - AES Encryption

        public string EncryptTenKH(string tenKH) => _aesService.Encrypt(tenKH);
        public string DecryptTenKH(string encryptedTenKH) => _aesService.Decrypt(encryptedTenKH);
        public bool IsTenKHEncrypted(string tenKH) => _aesService.IsEncrypted(tenKH);

        #endregion

        #region DIACHI - RSA Encryption

        public string EncryptDiaChi(string diaChi) => _rsaService.Encrypt(diaChi);
        public string DecryptDiaChi(string encryptedDiaChi) => _rsaService.Decrypt(encryptedDiaChi);
        public bool IsDiaChiEncrypted(string diaChi) => _rsaService.IsEncrypted(diaChi);

        #endregion

        #region SDT - Hybrid Encryption

        public string EncryptSDT(string sdt) => _hybridService.Encrypt(sdt);
        public string DecryptSDT(string encryptedSDT) => _hybridService.Decrypt(encryptedSDT);
        public bool IsSDTEncrypted(string sdt) => _hybridService.IsEncrypted(sdt);

        #endregion

        #region Encrypt/Decrypt All Fields

        /// <summary>
        /// Mã hóa tất cả các trường nhạy cảm của khách hàng
        /// </summary>
        public (string TenKH, string DiaChi, string SDT) EncryptCustomerData(
            string tenKH, string diaChi, string sdt)
        {
            return (
                EncryptTenKH(tenKH),
                EncryptDiaChi(diaChi),
                EncryptSDT(sdt)
            );
        }

        /// <summary>
        /// Giải mã tất cả các trường nhạy cảm của khách hàng
        /// </summary>
        public (string TenKH, string DiaChi, string SDT) DecryptCustomerData(
            string encryptedTenKH, string encryptedDiaChi, string encryptedSDT)
        {
            return (
                DecryptTenKH(encryptedTenKH),
                DecryptDiaChi(encryptedDiaChi),
                DecryptSDT(encryptedSDT)
            );
        }

        #endregion

        #region Key Management

        public string GetRsaPublicKey() => _rsaService.GetPublicKeyBase64();
        public string GetRsaPrivateKey() => _rsaService.GetPrivateKeyBase64();

        #endregion
    }
}
