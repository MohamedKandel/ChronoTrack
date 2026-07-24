using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("Accounts");

        builder.HasKey(a => a.AccountID);

        builder.Property(a => a.AccountID)
            .HasColumnName("AccountID")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(a => a.Username)
            .HasColumnName("username")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(a => a.Email)
            .HasColumnName("email")
            .HasMaxLength(70)
            .IsRequired();

        builder.Property(a => a.Hashed)
            .HasColumnName("passwordHashed")
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(a => a.Salt)
            .HasColumnName("passwordSalt")
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(a => a.Image)
            .HasColumnName("image")
            .HasColumnType("varbinary(max)")
            .IsRequired(false);

        builder.Property(a => a.ContentType)
            .HasColumnName("ImgContentType")
            .HasMaxLength(20)
            .IsRequired(false);

        builder.Property(a => a.OTP)
            .HasColumnName("OTP")
            .HasMaxLength(6)
            .IsRequired(false);

        builder.Property(a => a.ExpiryAt)
            .HasColumnName("expiryAt")
            .IsRequired(false);

        builder.Property(a => a.LastTimeRequested)
            .HasColumnName("LastTimeRequested")
            .IsRequired(false);

        builder.Property(a => a.IsVerified)
            .HasColumnName("isVerified")
            .IsRequired();

        builder.Property(a => a.CreatedAt)
            .HasColumnName("createdAt")
            .IsRequired();
    }
}