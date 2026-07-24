using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Account
{
    [Key]
    [Column("AccountID")]
    public string AccountID { get; set; } = null!;
    [Column("username")]
    public string Username { get; set; } = null!;
    [Column("email")]
    [EmailAddress]
    public string Email { get; set; } = null!;
    [Column("passwordHashed")]
    public byte[] Hashed { get; set; } = null!;
    [Column("passwordSalt")]
    public byte[] Salt { get; set; } = null!;
    [Column("image")]
    public byte[] Image { get; set; } = null!;

    [Column("ImgContentType")]
    public string ContentType { get; set; } = null!;

    [Column("OTP")]
    public string? OTP { get; set; } = null!;
    [Column("expiryAt")]
    public DateTime? ExpiryAt { get; set; }
    [Column("LastTimeRequested")]
    public DateTime? LastTimeRequested { get; set; }
    [Column("isVerified")]
    public bool IsVerified { get; set; }
    [Column("createdAt")]
    public DateTime CreatedAt { get; set; }

    // Navigation property: one account has many sessions
    public ICollection<Sessions> Sessions { get; set; } = new List<Sessions>();
}