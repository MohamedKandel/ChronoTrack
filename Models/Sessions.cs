using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class Sessions
{
    [Key]
    [Column("SessionID")]
    public string SessionId { get; set; } = null!;

    [Column("AccountID")]
    public string AccountId { get; set; } = null!;

    public DateOnly SessionDate { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly? EndTime { get; set; }

    // Navigation property: each session belongs to one account
    [ForeignKey(nameof(AccountId))]
    public Account Account { get; set; } = null!;
}