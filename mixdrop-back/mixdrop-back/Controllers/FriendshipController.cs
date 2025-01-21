using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using mixdrop_back.Models.Entities;
using mixdrop_back.Services;

namespace mixdrop_back.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FriendshipController : ControllerBase
    {
        private readonly FriendshipService _friendshipService;
        private readonly UserService _userService;

        public FriendshipController(FriendshipService friendshipService, UserService userService)
        {
            _friendshipService = friendshipService;
            _userService = userService;
        }

        // Enviar solicitud de amistad
        [Authorize]
        [HttpPost]
        public async Task AddFriend([FromBody] User user2)
        {
            User user1 = await GetAuthorizedUser();
            await _friendshipService.AddFriend(user1, user2);
        }

        // Aceptar solicitud de amistad
        [Authorize]
        [HttpPut]
        public async Task AcceptFriend([FromQuery] int friendshipId)
        {
            User user = await GetAuthorizedUser();
            await _friendshipService.AcceptFriend(friendshipId, user);
        }

        // Borrar amigo o rechazar solicitud de amistad
        [Authorize]
        [HttpDelete]
        public async Task DeleteFriend([FromQuery] int friendshipId)
        {
            User user = await GetAuthorizedUser();
            await _friendshipService.DeleteFriend(friendshipId, user);
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
}
