using System.ComponentModel.DataAnnotations;

public class ResetPasswordDTO
{
    [EmailAddress]
    public string email { get; set; }
    public string newPassword { get; set; }
}