using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLDH.Models
{
    /// <summary>
    /// Bảng lưu trữ RSA key pairs cho mã hóa
    /// </summary>
    [Table("CRYPTO_KEYS")]
    public class CryptoKey
    {
        [Key]
        [Column("KEY_ID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int KeyId { get; set; }

        [Column("KEY_NAME")]
        [Required]
        [MaxLength(50)]
        public string KeyName { get; set; } = string.Empty;

        [Column("PUBLIC_KEY")]
        [Required]
        public string PublicKey { get; set; } = string.Empty;

        [Column("PRIVATE_KEY")]
        [Required]
        public string PrivateKey { get; set; } = string.Empty;

        [Column("KEY_TYPE")]
        [MaxLength(20)]
        public string KeyType { get; set; } = "RSA-2048";

        [Column("CREATED_DATE")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [Column("IS_ACTIVE")]
        [MaxLength(1)]
        public string IsActive { get; set; } = "Y";
    }
}
