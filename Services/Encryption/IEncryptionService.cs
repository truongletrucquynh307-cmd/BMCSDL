namespace QLDH.Services.Encryption
{
    /// <summary>
    /// Interface cho các dịch vụ mã hóa
    /// </summary>
    public interface IEncryptionService
    {
        /// <summary>
        /// Mã hóa chuỗi plaintext
        /// </summary>
        string Encrypt(string plainText);

        /// <summary>
        /// Giải mã chuỗi đã được mã hóa
        /// </summary>
        string Decrypt(string cipherText);

        /// <summary>
        /// Kiểm tra xem chuỗi có phải đã được mã hóa không
        /// </summary>
        bool IsEncrypted(string text);
    }
}
