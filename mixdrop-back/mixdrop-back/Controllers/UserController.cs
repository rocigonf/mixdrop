using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using mixdrop_back.Models.DTOs;
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

}
