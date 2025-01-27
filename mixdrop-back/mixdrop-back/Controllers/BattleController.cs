using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using mixdrop_back.Models.Entities;
using mixdrop_back.Services;

namespace mixdrop_back.Controllers;

[Route("api/[controller]")]
[ApiController]
public class BattleController : ControllerBase
{
    private readonly BattleService _battleService;
    private readonly UserService _userService;

    public BattleController(BattleService friendshipService, UserService userService)
    {
        _battleService = friendshipService;
        _userService = userService;
    }

    // TODO: Pasar a DTO para no enviar los datos del usuario
    /*[Authorize]
    [HttpGet("{id}")]
    public async Task<Friendship> GetFriendshipById(int id)
    {
        User user = await GetAuthorizedUser();
        Friendship friendship = await _battleService.GetFriendshipByIdAsync(id);

        UserFriend userInFriendship = friendship.UserFriends.FirstOrDefault(user => user.Id == id);
        if (userInFriendship == null)
        {
            Console.WriteLine("El usuario no está en la amistad");
            return null;
        }

        return friendship;
    }*/

    // Enviar solicitud de batalla
    [Authorize]
    [HttpPost]
    public async Task AddBattle([FromBody] User user2)
    {
        User user1 = await GetAuthorizedUser();
        await _battleService.CreateBattle(user1, user2);
    }

    // Aceptar solicitud de batalla
    [Authorize]
    [HttpPut("{id}")]
    public async Task AcceptBattle(int id)
    {
        int userId = GetAuthorizedId();
        await _battleService.AcceptBattle(id, userId);
    }

    // Rechazar solicitud de batalla
    [Authorize]
    [HttpDelete("{id}")]
    public async Task DeleteBattle(int id)
    {
        int userId = GetAuthorizedId();
        await _battleService.DeleteBattle(id, userId);
    }

    private async Task<User> GetAuthorizedUser()
    {
        System.Security.Claims.ClaimsPrincipal currentUser = this.User;
        string firstClaim = currentUser.Claims.First().ToString();
        string idString = firstClaim.Substring(firstClaim.IndexOf("nameidentifier:") + "nameIdentifier".Length + 2);

        // Pilla el usuario de la base de datos
        return await _userService.GetFullUserByIdAsync(int.Parse(idString));
    }

    private int GetAuthorizedId()
    {
        System.Security.Claims.ClaimsPrincipal currentUser = this.User;
        string firstClaim = currentUser.Claims.First().ToString();
        string idString = firstClaim.Substring(firstClaim.IndexOf("nameidentifier:") + "nameIdentifier".Length + 2);

        // Pilla el usuario de la base de datos
        return int.Parse(idString);
    }
}
