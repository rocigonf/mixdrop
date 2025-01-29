using mixdrop_back.Models.Entities;
using mixdrop_back.Services;
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

    public async Task HandleWebsocketAsync(WebSocket webSocket, User user)
    {
        // Creamos un nuevo WebSocketHandler a partir del WebSocket recibido y lo añadimos a la lista
        UserSocket handler = await AddWebsocketAsync(webSocket, user);

        Dictionary<object, object> dict = new Dictionary<object, object>
        {
            { "messageType", MessageType.Stats },
            { "total", Total },
        };

        await SendStatsMessage();

        await handler.ProcessWebSocket();
    }

    private async Task<UserSocket> AddWebsocketAsync(WebSocket webSocket, User user)
    {
        // Esperamos a que haya un hueco disponible
        await _semaphore.WaitAsync();

        // Sección crítica

        UserSocket handler = new UserSocket(_serviceProvider, webSocket, user);
        handler.Disconnected += OnDisconnectedAsync;
        USER_SOCKETS.Add(handler);
        Total++;

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

        using IServiceScope scope = _serviceProvider.CreateScope();

        var unitOfWork = scope.ServiceProvider.GetRequiredService<UnitOfWork>();
        var battleService = scope.ServiceProvider.GetRequiredService<BattleService>();

        ICollection<Battle> battles = await unitOfWork.BattleRepository.GetCurrentBattleByUser(disconnectedHandler.User.Id);
        if(battles.Count > 1)
        {
            throw new Exception("El usuario está en más de una batalla al mismo tiempo :(");
        }
        else
        {
            // Borro al usuario de la batalla
            if(battles.Count == 1)
            {
                await battleService.DeleteBattleByObject(battles.First(), disconnectedHandler.User.Id, true);
            }
        }

        disconnectedHandler.User.StateId = 1;
        disconnectedHandler.User.IsInQueue = false;
        unitOfWork.UserRepository.Update(disconnectedHandler.User);
        await unitOfWork.SaveAsync();

        //await SendStatsMessage();

        scope.Dispose();

        //await SendStatsMessage();

        // Liberamos el semáforo
        _semaphore.Release();

        // Lista donde guardar las tareas de envío de mensajes
        List<Task> tasks = new List<Task>();
        // Guardamos una copia de los WebSocketHandler para evitar problemas de concurrencia
        UserSocket[] handlers = USER_SOCKETS.ToArray();

        tasks.Add(SendStatsMessage());

        // Esperamos a que todas las tareas de envío de mensajes se completen
        await Task.WhenAll(tasks);
    }

    public static async Task NotifyOneUser(string jsonToSend, int userId)
    {
        var userSocket = USER_SOCKETS.FirstOrDefault(userSocket => userSocket.User.Id == userId);
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
