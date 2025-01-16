namespace mixdrop_back.Models.DTOs;

// datos con los que el usuario inicia sesion
public class LoginRequest
{
    public string EmailOrNickname { get; set; } = null!;

    public string Password { get; set; } = null!;

}
