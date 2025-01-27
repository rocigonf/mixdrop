using System.Net.WebSockets;
using System.Text.Json;

namespace mixdrop_back.Sockets;

public class WebSocketHandler
{
    private static IServiceProvider _serviceProvider;
    private static readonly List<UserSocket> USER_SOCKETS = new List<UserSocket>();
    public static int Total { get; set; } = 0;

    // Semáforo para controlar el acceso a la lista de WebSocketHandler
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

    public WebSocketHandler(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task HandleWebsocketAsync(WebSocket webSocket, int userId)
    {
        // Creamos un nuevo WebSocketHandler a partir del WebSocket recibido y lo añadimos a la lista
        UserSocket handler = await AddWebsocketAsync(webSocket, userId);

        Dictionary<object, object> dict = new Dictionary<object, object>
        {
            { "messageType", MessageType.Stats },
            { "total", Total },
        };

        await SendStatsMessage();
        await handler.ProcessWebSocket();
    }

    private async Task<UserSocket> AddWebsocketAsync(WebSocket webSocket, int userId)
    {
        // Esperamos a que haya un hueco disponible
        await _semaphore.WaitAsync();

        // Sección crítica

        UserSocket handler = new UserSocket(_serviceProvider, webSocket, userId);
        handler.Disconnected += OnDisconnectedAsync;
        USER_SOCKETS.Add(handler);
        Total++;

        /*var existingSocket = USER_SOCKETS.FirstOrDefault(u => u.UserId == userId);

        if (existingSocket == null || existingSocket.Socket == null || existingSocket.Socket.State == WebSocketState.Closed)
        {
            if(existingSocket != null)
            {
                USER_SOCKETS.Remove(existingSocket);
                Total--;
            }

            existingSocket = new UserSocket(webSocket, userId);
            existingSocket.Disconnected += OnDisconnectedAsync;
            USER_SOCKETS.Add(existingSocket);
            Total++;
        }*/

        // Liberamos el semáforo
        _semaphore.Release();

        return handler;
    }

    private async Task OnDisconnectedAsync(UserSocket disconnectedHandler)
    {
        // Esperamos a que haya un hueco disponible
        await _semaphore.WaitAsync();

        // Sección crítica
        // Nos desuscribimos de los eventos y eliminamos el WebSocketHandler de la lista
        disconnectedHandler.Disconnected -= OnDisconnectedAsync;
        USER_SOCKETS.Remove(disconnectedHandler);
        Total--;

        // Liberamos el semáforo
        _semaphore.Release();

        // Lista donde guardar las tareas de envío de mensajes
        List<Task> tasks = new List<Task>();
        // Guardamos una copia de los WebSocketHandler para evitar problemas de concurrencia
        UserSocket[] handlers = USER_SOCKETS.ToArray();

        // Esperamos a que todas las tareas de envío de mensajes se completen
        await Task.WhenAll(tasks);
    }

    /*public static async Task RemoveSocket(int userId)
    {
        var userSocket = USER_SOCKETS.FirstOrDefault(userSocket => userSocket.UserId == userId);
        if (userSocket != null)
        {
            USER_SOCKETS.Remove(userSocket);
            Total -= 1;
            userSocket.Socket = null;
            userSocket.Socket.Dispose(); // Para cerrar el websocket
            userSocket = null;

            await SendStatsMessage();
        }
    }*/

    public static async Task NotifyOneUser(string jsonToSend, int userId)
    {
        var userSocket = USER_SOCKETS.FirstOrDefault(userSocket => userSocket.UserId == userId);
        if (userSocket != null && userSocket.Socket.State == WebSocketState.Open)
        {
            await userSocket.SendAsync(jsonToSend);
        }
    }

    public static async Task NotifyUsers(string jsonToSend)
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
