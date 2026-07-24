public class SessionDataDTO
{
    public string SessionId { get; set; } = null!;
    public DateOnly SessionDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly? EndTime { get; set; }
    public string? Duration { get; set; } = null!;   // "HH:mm:ss" formatted string
    public int? RoundedHours { get; set; }
}