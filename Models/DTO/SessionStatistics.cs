public class SessionStatistics
{
    public int HoursThisMonth { get; set; }
    public int HoursLastMonth { get; set; }
    public string Percent { get; set; }
    public string? ActiveSessionID { get; set; }
    public SessionsView LastSession { get; set; }
}