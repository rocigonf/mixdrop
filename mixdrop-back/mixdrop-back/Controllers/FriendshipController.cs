using Microsoft.AspNetCore.Authorization;
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

        // TODO: Pasar a DTO para no enviar los datos del usuario
        /*[Authorize]
        [HttpGet("{id}")]
        public async Task<Friendship> GetFriendshipById(int id)
        {
            User user = await GetAuthorizedUser();
            Friendship friendship = await _friendshipService.GetFriendshipByIdAsync(id);

            UserFriend userInFriendship = friendship.UserFriends.FirstOrDefault(user => user.Id == id);
            if (userInFriendship == null)
            {
                Console.WriteLine("El usuario no está en la amistad");
                return null;
            }

            return friendship;
        }*/

        // Enviar solicitud de amistad
        [Authorize]
        [HttpPost]
        public async Task AddFriend([FromBody] User user2)
        {
            User user1 = await GetAuthorizedUser();

            if (user1 == null)
            {
                return;
            }

            await _friendshipService.AddFriend(user1, user2);
        }

        // Aceptar solicitud de amistad
        [Authorize]
        [HttpPut("{id}")]
        public async Task AcceptFriend(int id)
        {
            User user = await GetAuthorizedUser();

            if (user == null)
            {
                return;
            }

            await _friendshipService.AcceptFriend(id, user.Id);
        }

        // Borrar amigo o rechazar solicitud de amistad
        [Authorize]
        [HttpDelete("{id}")]
        public async Task DeleteFriend(int id)
        {
            User user = await GetAuthorizedUser();

            if (user == null)
            {
                return;
            }

            await _friendshipService.DeleteFriend(id, user.Id);
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
}
