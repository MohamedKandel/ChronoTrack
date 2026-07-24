using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class SessionsViewConfiguration : IEntityTypeConfiguration<SessionsView>
{
    public void Configure(EntityTypeBuilder<SessionsView> builder)
    {
        builder.ToView("sessionsView", "dbo");

        // SessionID is unique per row since it maps 1:1 to Sessions.SessionID
        builder.HasKey(s => s.SessionId);

        builder.Property(s => s.SessionId)
            .HasColumnName("SessionID")
            .HasMaxLength(50);

        builder.Property(s => s.SessionDate)
            .HasColumnName("SessionDate");

        builder.Property(s => s.StartTime)
            .HasColumnName("StartTime");

        builder.Property(s => s.EndTime)
            .HasColumnName("EndTime");

        builder.Property(s => s.Duration)
            .HasColumnName("Duration")
            .HasMaxLength(8);

        builder.Property(s => s.RoundedHours)
            .HasColumnName("RoundedHours");

        builder.Property(s => s.AccountID)
            .HasColumnName("AccountID")
            .HasMaxLength(50);

        // Views are effectively read-only — prevent accidental writes
        builder.ToView("sessionsView", "dbo");
    }
}