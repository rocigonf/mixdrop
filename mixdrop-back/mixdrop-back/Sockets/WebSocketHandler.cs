using mixdrop_back.Models.Entities;
using mixdrop_back.Services;
using System.Net.WebSockets;
using System.Text.Json;

namespace mixdrop_back.Sockets;

public class WebSocketHandler
{
    private static IServiceProvider _serviceProvider;
    public static readonly List<UserSocket> USER_SOCKETS = new List<UserSocket>();

    public static int Total { get; set; } = 0;
    //public static int TotalBattles { get; set; } = 0; // las batallas q se estan jugando
    //public static int TotalPlayers { get; set; } = 0; // personas jugando

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

        await SendStatsMessage();

        await handler.ProcessWebSocket();
    }

    private async Task<UserSocket> AddWebsocketAsync(WebSocket webSocket, User user)
    {
        // Esperamos a que haya un hueco disponible
        await _semaphore.WaitAsync();

        // Sección crítica

        UserSocket existingSocket = USER_SOCKETS.FirstOrDefault(u => u.User.Id == user.Id);
        if (existingSocket != null)
        {
            USER_SOCKETS.Remove(existingSocket);
            Total--;
        }

        UserSocket handler = new UserSocket(_serviceProvider, webSocket, user);
        handler.Disconnected += OnDisconnectedAsync;
        USER_SOCKETS.Add(handler);
        Total++;

        using var scope = _serviceProvider.CreateScope();
        UnitOfWork unitOfWork = scope.ServiceProvider.GetService<UnitOfWork>();

        // estado de conectado
        var estadoConectado = await unitOfWork.StateRepositoty.GetByIdAsync(2);

        user.StateId = estadoConectado.Id; // conectado
        user.State = estadoConectado;

        unitOfWork.UserRepository.Update(user);
        await unitOfWork.SaveAsync();

        scope.Dispose();

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

        ICollection<Battle> battles = await unitOfWork.BattleRepository.GetCurrentBattleByUserWithoutThem(disconnectedHandler.User.Id);
        bool sendNotif = false;
        
        if(battles.Count > 1)
        {
            /*foreach(Battle battle in battles)
            {
                unitOfWork.BattleRepository.Delete(battle);
            }*/

            Console.WriteLine("El usuario está en más de una batalla al mismo tiempo :(");
        }
        else
        {
            // Borro al usuario de la batalla
            if (battles.Count == 1)
            {
                sendNotif = true;
            }
        }

        var state = await unitOfWork.StateRepositoty.GetByIdAsync(1);

        disconnectedHandler.User.StateId = state.Id;
        disconnectedHandler.User.State = state;
        disconnectedHandler.User.IsInQueue = false;
        unitOfWork.UserRepository.Update(disconnectedHandler.User);
        await unitOfWork.SaveAsync();

        // Guardamos una copia de los WebSocketHandler para evitar problemas de concurrencia
        UserSocket[] handlers = USER_SOCKETS.ToArray();

        if (sendNotif)
        {
            UserBattle user = battles.First().BattleUsers.FirstOrDefault(u => u.UserId == disconnectedHandler.User.Id);
            if (!battles.First().IsAgainstBot)
            {
                UserBattle otherUser = battles.First().BattleUsers.FirstOrDefault(u => u.UserId != disconnectedHandler.User.Id);
                await battleService.EndBattle(battles.First(), otherUser, user);
            }
            else
            {
                await battleService.DeleteBattleByObject(battles.First(), user.UserId, false, false);
            }
            GayNetwork.DeleteHandler(disconnectedHandler.User.Id);
            await SendStatsMessage();
        }

        await SendStatsMessage();
        
        scope.Dispose();

        // Liberamos el semáforo
        _semaphore.Release();
    }

    public static async Task NotifyOneUser(string jsonToSend, int userId)
    {
        var userSocket = USER_SOCKETS.FirstOrDefault(userSocket => userSocket.User.Id == userId);
        if (userSocket != null && userSocket.Socket.State == WebSocketState.Open)
        {
            await userSocket.SendAsync(jsonToSend);
        }
    }

    public static async Task NotifyOneUserBlob(byte[] message, int userId)
    {
        var userSocket = USER_SOCKETS.FirstOrDefault(userSocket => userSocket.User.Id == userId);
        if (userSocket != null && userSocket.Socket.State == WebSocketState.Open)
        {
            await userSocket.SendBlobAsync(message);
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
        int totalBattles = GayNetwork._handlers.Count;
        int totalBattlesAgainstBots = GayNetwork._handlers.Where(h => h.Battle.IsAgainstBot).Count();
        int totalBattlesWithoutBots = GayNetwork._handlers.Where(h => h.Battle.IsAgainstBot == false).Count();

        int totalPlayers = totalBattlesWithoutBots * 2;
        totalPlayers += totalBattlesAgainstBots;

        Dictionary<object, object> dict = new Dictionary<object, object>
        {
            { "messageType", MessageType.Stats },
            { "total", Total },
            { "totalBattles" , totalBattles },
            { "totalPlayers" , totalPlayers}
        };

        await NotifyUsers(JsonSerializer.Serialize(dict));
    }
}
