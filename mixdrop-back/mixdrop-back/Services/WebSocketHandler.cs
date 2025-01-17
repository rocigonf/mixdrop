using mixdrop_back.Models.Entities;
using mixdrop_back.Models.Sockets;
using System.Net.WebSockets;
using System.Text.Json;

namespace mixdrop_back.Services;

public class WebSocketHandler
{
    private static readonly List<UserSocket> USER_SOCKETS = new List<UserSocket>();
    public static int Total { get; set; } = 0;

    public async Task HandleWebsocketAsync(WebSocket webSocket, int userId)
    {
        // TODO: Cuando asigno un nuevo socket a la lista, envío a todos los usuarios que se ha cambiado
        var socket = USER_SOCKETS.FirstOrDefault(userSocket => userSocket.UserId == userId);
        if (socket == null)
        {
            socket = new UserSocket()
            {
                UserId = userId,
                Socket = webSocket
            };
            USER_SOCKETS.Add(socket);
            Total += 1;
        }
        else
        {
            // Necesario porque parece que toma el socket como cerrado, no sé por qué
            socket.Socket = webSocket;
        }

        await NotifyUsers();
        await socket.ProcessWebSocket();
    }

    private static async Task NotifyUsers()
    {
        Dictionary<object, object> dict = new Dictionary<object, object>
        {
            { "messageType", MessageType.Stats },
            { "total", Total }
        };

        string jsonToSend = JsonSerializer.Serialize(dict);

        foreach (var userSocket in USER_SOCKETS)
        {
            if(userSocket.Socket.State == WebSocketState.Open) await userSocket.SendAsync(jsonToSend);
        }
    }
}
