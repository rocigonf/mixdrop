using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using mixdrop_back.Models.DTOs;
using mixdrop_back.Models.Entities;
using mixdrop_back.Services;

namespace mixdrop_back.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly UserService _userService;

    public UserController(UserService userService)
    {
        _userService = userService;
    }

    [Authorize]
    [HttpGet("{id}")]
    public async Task<UserDto> GetUserById(int id)
    {
        // Puede ser que haya que optimizar la petición para que no se traiga los amigos, por ejemplo
        UserDto userDto = await _userService.GetUserByIdAsync(id);
        return userDto;
    }

    [Authorize]
    [HttpPut]
    public async Task<IActionResult> UpdateUser([FromForm] RegisterDto user)
    {
        try
        {
            User currentUser = await GetAuthorizedUser();

            // Si no es admin y está intentando modificar a otro usuario
            if(!currentUser.Role.Equals("Admin") && user.Id != currentUser.Id)
            {
                return Unauthorized();
            }

            string role = "User";
            if(currentUser.Role.Equals("Admin"))
            {
                role = user.Role;
            }

            if (currentUser.Id == user.Id)
            {
                await _userService.UpdateUser(user, currentUser, role);
            }
            else
            {
                // Para los admin
                User oldUser = await _userService.GetBasicUserByIdAsync(user.Id);
                await _userService.UpdateUser(user, oldUser, role);
            }

            return Ok();
        }
        catch
        {
            return BadRequest();
        }
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchUser([FromQuery] string query)
    {
        if (query == null)
        {
            return BadRequest("Busqueda fallida.");
        }

        var result = await _userService.SearchUser(query);

        if (result.Count == 0)
        {
            return Ok(new { users = new List<UserDto>()});
        }

        return Ok(new { users = result });

    }

    private async Task<User> GetAuthorizedUser()
    {
        System.Security.Claims.ClaimsPrincipal currentUser = this.User;
        string firstClaim = currentUser.Claims.First().ToString();
        string idString = firstClaim.Substring(firstClaim.IndexOf("nameidentifier:") + "nameIdentifier".Length + 2);

        // Pilla el usuario de la base de datos
        return await _userService.GetBasicUserByIdAsync(int.Parse(idString));
    }

}
