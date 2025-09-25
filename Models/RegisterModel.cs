using System.ComponentModel.DataAnnotations;

public class AccountRegisterModel
{
    public string FirstName { get; set; }   // optional
    public string LastName { get; set; }    // optional
    [Required, EmailAddress]
    public string Email { get; set; }
    [Required, MinLength(12)]
    public string Password { get; set; }
    [Compare("Password")]
    public string ConfirmPassword { get; set; }
}
