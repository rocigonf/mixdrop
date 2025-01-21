namespace mixdrop_back.Models.DTOs;

// datos con los que el usuario se registra
public class RegisterDto
{
    public int Id { get; set; } = 0;
    public string Nickname { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public IFormFile Image { get; set; }
    public string Role { get; set; } = "User";
    public string ChangeImage { get; set; } = "true";
}
