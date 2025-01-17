using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using mixdrop_back.Models.Entities;
using mixdrop_back.Services;
using System.Net.WebSockets;

namespace mixdrop_back.Controllers;

[Route("socket")]
[ApiController]
public class WebSocketController : ControllerBase
{
    private readonly WebSocketHandler _webSocketHandler;
    public WebSocketController(WebSocketHandler webSocketHandler)
    { 
        _webSocketHandler = webSocketHandler;
    }

    [Authorize]
    [HttpGet("{jwt}")]
    public async Task ConnectAsync()
    {
        // Si la petición es de tipo websocket la aceptamos
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            // Aceptamos la solicitud
            WebSocket webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();

            // Manejamos la solicitud.
            await _webSocketHandler.HandleWebsocketAsync(webSocket, GetCurrentUserId());
        }
        // En caso contrario la rechazamos
        else
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        }

        // Cuando este método finalice, se cerrará automáticamente la conexión con el websocket
    }


    private int GetCurrentUserId()
    {
        // Pilla el usuario autenticado según ASP
        System.Security.Claims.ClaimsPrincipal currentUser = this.User;
        string firstClaim = currentUser.Claims.First().ToString();

        // Un poco hardcodeado por mi parte la verdad xD
        string idString = firstClaim.Substring(firstClaim.IndexOf("nameidentifier:") + "nameIdentifier".Length + 2);

        return int.Parse(idString);
    }

}
