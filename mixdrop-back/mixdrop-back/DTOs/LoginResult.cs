using mixdrop_back.DTOs;

namespace Ecommerce.Models.Dtos;

// se envia al iniciar sesion
public class LoginResult
{
    public string AccessToken { get; set; }

    public UserDto User { get; set; }   // Para que el front reciba datos del usuario que ha iniciado sesión
}
