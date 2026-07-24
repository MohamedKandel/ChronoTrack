using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class SessionConfiguration : IEntityTypeConfiguration<Sessions>
{
    public void Configure(EntityTypeBuilder<Sessions> builder)
    {
        builder.ToTable("Sessions");

        builder.HasKey(s => s.SessionId);

        builder.Property(s => s.SessionId)
            .HasColumnName("SessionID")
            .HasMaxLength(50);

        builder.Property(s => s.AccountId)
            .HasColumnName("AccountID")
            .HasMaxLength(50);

        builder.Property(s => s.SessionDate)
            .HasColumnName("SessionDate");

        builder.Property(s => s.StartTime)
            .HasColumnName("StartTime");

        builder.Property(s => s.EndTime)
            .HasColumnName("EndTime");

        builder.HasOne(s => s.Account)
            .WithMany(a => a.Sessions)
            .HasForeignKey(s => s.AccountId)
            .HasConstraintName("fk_Session_Account")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(s => s.SessionDate)
            .HasDatabaseName("SessionDateIdx");
    }
}