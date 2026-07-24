using System.ComponentModel.DataAnnotations;

public class RegisterAccountDTO
{
    [Required]
    public string Username { get; set; }

    [Required]
    [EmailAddress]
    public string email { get; set; }

    [Required]
    public string password { get; set; }

    [Required]
    public IFormFile Image { get; set; }
}