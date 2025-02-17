using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using mixdrop_back.Models.Entities;
using mixdrop_back.Services;
using mixdrop_back.Sockets;
using System.Net.WebSockets;

namespace mixdrop_back.Controllers;

[Route("socket")]
[ApiController]
public class WebSocketController : ControllerBase
{
    private readonly WebSocketHandler _webSocketHandler;
    private readonly UserService _userService;

    public WebSocketController(WebSocketHandler webSocketHandler, UserService userService)
    { 
        _webSocketHandler = webSocketHandler;
        _userService = userService;
    }

    [Authorize]
    [Route("{jwt}")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task ConnectAsync()
    {
        // Si la petición es de tipo websocket la aceptamos
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            // Aceptamos la solicitud
            WebSocket webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();

            User user = await GetAuthorizedUser();

            if (user == null)
            {
                return;
            }

            user.StateId = 2;
            await _userService.ConnectUser(user);

            // Manejamos la solicitud.
            await _webSocketHandler.HandleWebsocketAsync(webSocket, user);
        }
        // En caso contrario la rechazamos
        else
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        }

        // Cuando este método finalice, se cerrará automáticamente la conexión con el websocket
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
