using mixdrop_back.Models.Entities;
using mixdrop_back.Models.Sockets;
using System.Net.WebSockets;

namespace mixdrop_back.Services;

public class WebSocketHandler
{
    private List<UserSocket> userSockets;

    public async Task HandleWebsocketAsync(WebSocket webSocket, int userId)
    {
        // TODO: Cuando asigno un nuevo socket a la lista, envío a todos los usuarios que se ha cambiado
        var socket = userSockets.FirstOrDefault(userSocket => userSocket.UserId == userId);
        if (socket == null)
        {
            socket = new UserSocket()
            {
                UserId = userId,
                Socket = webSocket
            };
            userSockets.Add(socket);
        }

        await NotifyUsers();
        await socket.ProcessWebSocket();
    }

    private async Task NotifyUsers()
    {
        foreach (var userSocket in userSockets)
        {
            await userSocket.SendAsync("Se conectó un usuario");
        }
    }
}
