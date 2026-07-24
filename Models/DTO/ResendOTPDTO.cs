using System.ComponentModel.DataAnnotations;

public class ResendOTPDTO
{
    [EmailAddress]
    public string email { get; set; }
}