using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using mixdrop_back.Models.DTOs;
using mixdrop_back.Models.Entities;
using mixdrop_back.Services;
using mixdrop_back.Sockets;

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

    [Authorize]
    [HttpGet("{id}")]
    public async Task<List<BattleDto>> GetBattleById(int id)
    {
        User user1 = await GetAuthorizedUser();

        if (user1 == null)
        {
            return null;
        }

        return await _battleService.GetBattleByIdAsync(id);
    }

    // Crear batalla
    [Authorize]
    [HttpPost]
    public async Task AddBattle([FromBody] BattleRequest request)
    {
        bool isRandom = request.IsRandom;

        User user1 = await GetAuthorizedUser();

        if (user1 == null)
        {
            return;
        }

        await _battleService.CreateBattle(user1, request.User2Id, isRandom);
    }


    [Authorize]
    [HttpDelete("ragequit")]
    public async Task ForfeitBattle()
    {
        User user1 = await GetAuthorizedUser();

        if (user1 == null)
        {
            return;
        }

        await _battleService.ForfeitBattle(user1);
        await WebSocketHandler.SendStatsMessage();
    }


    // Aceptar solicitud de batalla
    [Authorize]
    [HttpPut("{id}")]
    public async Task AcceptBattle(int id)
    {
        User user = await GetAuthorizedUser();

        if (user == null)
        {
            return;
        }

        await _battleService.AcceptBattle(id, user.Id);
    }

    // Aceptar solicitud de batalla
    [Authorize]
    [HttpPut("start/{id}")]
    public async Task StartBattle(int id)
    {
        User user = await GetAuthorizedUser();

        if (user == null)
        {
            return;
        }

        await _battleService.StartBattle(id, user.Id);

        // modo muy cutre 
        /*WebSocketHandler.TotalBattles++;
        WebSocketHandler.TotalPlayers += 2;*/
        await WebSocketHandler.SendStatsMessage();
    }

    // Rechazar solicitud de batalla
    [Authorize]
    [HttpDelete("{id}/{notify}")]
    public async Task DeleteBattle(int id, bool notify)
    {
        User user = await GetAuthorizedUser();

        if (user == null)
        {
            return;
        }

        await _battleService.DeleteBattleById(id, user.Id, notify);

        // modo muy cutre 
        /*WebSocketHandler.TotalBattles--;
        WebSocketHandler.TotalPlayers -= 2;*/
        await WebSocketHandler.SendStatsMessage();
    }

    [Authorize]
    [HttpDelete("bot")]
    public async Task DeleteBattleAgainstBot()
    {
        User user = await GetAuthorizedUser();

        if (user == null)
        {
            return;
        }

        await _battleService.DeleteBattleAgainstBot(user);
        await WebSocketHandler.SendStatsMessage();
    }

    // Emparejamiento aleatorio
    [Authorize]
    [HttpPost("Matchmaking")]
    public async Task RandomBattle()
    {
        User user = await GetAuthorizedUser();

        if (user == null)
        {
            return;
        }

        await _battleService.RandomBattle(user);

        // modo muy cutre 
        /*WebSocketHandler.TotalBattles++;
        WebSocketHandler.TotalPlayers += 2;*/
        await WebSocketHandler.SendStatsMessage();
    }

    [Authorize]
    [HttpDelete("Matchmaking/delete")]
    public async Task DeleteRandomBattle()
    {
        User user = await GetAuthorizedUser();

        if (user == null)
        {
            return;
        }

        await _battleService.DeleteFromQueue(user);
    }

    private async Task<User> GetAuthorizedUser()
    {
        System.Security.Claims.ClaimsPrincipal currentUser = this.User;
        string firstClaim = currentUser.Claims.First().ToString();
        string idString = firstClaim.Substring(firstClaim.IndexOf("nameidentifier:") + "nameIdentifier".Length + 2);

        // Pilla el usuario de la base de datos
        User user = await _userService.GetFullUserByIdAsync(int.Parse(idString));
        
        if (user.Banned)
        {
            Console.WriteLine("Usuario baneado");
            return null;
        }

        return user;
    }
}
