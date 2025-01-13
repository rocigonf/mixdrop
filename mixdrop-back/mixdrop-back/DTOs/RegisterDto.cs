
namespace mixdrop_back.DTOs;

// datos con los que el usuario se registra
public class RegisterDto
{
    public string Nickname { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public IFormFile Image { get; set; }

}
