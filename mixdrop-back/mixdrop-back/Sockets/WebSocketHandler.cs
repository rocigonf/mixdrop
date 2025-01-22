using System.Net.WebSockets;
using System.Text.Json;

namespace mixdrop_back.Sockets;

public class WebSocketHandler
{
    private static readonly List<UserSocket> USER_SOCKETS = new List<UserSocket>();
    public static int Total { get; set; } = 0;

    public async Task HandleWebsocketAsync(WebSocket webSocket, int userId)
    {
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
            if (socket.Socket == null || socket.Socket.State == WebSocketState.Closed)
            {
                socket.Socket = webSocket;
            }
        }

        Dictionary<object, object> dict = new Dictionary<object, object>
        {
            { "messageType", MessageType.Stats },
            { "total", Total },
        };

        await SendStatsMessage();
        await socket.ProcessWebSocket();
    }

    public static async Task RemoveSocket(int userId)
    {
        var userSocket = USER_SOCKETS.FirstOrDefault(userSocket => userSocket.UserId == userId);
        if (userSocket != null)
        {
            USER_SOCKETS.Remove(userSocket);
            Total -= 1;
            userSocket.Socket.Dispose(); // Para cerrar el websocket

            await SendStatsMessage();
        }
    }

    private static async Task NotifyUsers(string jsonToSend)
    {
        foreach (var userSocket in USER_SOCKETS)
        {
            if (userSocket.Socket.State == WebSocketState.Open) await userSocket.SendAsync(jsonToSend);
        }
    }

    // Esto habrá que ponerlo en otro archivo
    public static async Task SendStatsMessage()
    {
        Dictionary<object, object> dict = new Dictionary<object, object>
        {
            { "messageType", MessageType.Stats },
            { "total", Total },
        };

        await NotifyUsers(JsonSerializer.Serialize(dict));
    }
}
