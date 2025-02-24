using mixdrop_back.Models.DTOs;
using mixdrop_back.Models.Entities;
using mixdrop_back.Models.Mappers;
using mixdrop_back.Services;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace mixdrop_back.Sockets;

public class UserSocket
{
    private static IServiceProvider _serviceProvider;

    public WebSocket Socket;
    public User User { get; set; }

    public event Func<UserSocket, Task> Disconnected;

    public UserSocket(IServiceProvider serviceProvider, WebSocket socket, User user)
    {
        _serviceProvider = serviceProvider;
        Socket = socket;
        User = user;
    }

    public async Task ProcessWebSocket()
    {
        // Mientras que el websocket del cliente esté conectado
        while (Socket.State == WebSocketState.Open)
        {
            try
            {
                string message = await ReadAsync();
                Dictionary<object, object> dictInput = GetActionMessage(message);


                // Si ha recibido algo
                if (!string.IsNullOrWhiteSpace(message))
                {
                    if (dictInput == null)
                    {
                        Console.WriteLine("El diccionario no es válido");
                        continue;
                    }

                    // AQUÍ TRASLADO EL MENSAJE AL ENUM Y HAGO SWITCH (POR AHORA) (ahora el messageType está en el catch de GetActionMessage()
                    JsonSerializerOptions options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
                    options.ReferenceHandler = ReferenceHandler.IgnoreCycles;

                    using IServiceScope scope = _serviceProvider.CreateScope();

                    // Obtención de los servicios necesarios
                    BattleService battleService = scope.ServiceProvider.GetRequiredService<BattleService>();
                    BattleMapper battleMapper = new BattleMapper();
                    UnitOfWork unitOfWork = scope.ServiceProvider.GetRequiredService<UnitOfWork>();

                    dictInput.TryGetValue("messageType", out object messageTypeRaw);

                    MessageType messageType;

                    bool couldParse = int.TryParse(messageTypeRaw.ToString(), out int messageTypeInt);
                    if (couldParse)
                    {
                        messageType = (MessageType)messageTypeInt;
                    }
                    else
                    {
                        messageType = (MessageType)messageTypeRaw;
                    }


                    Dictionary<object, object> dict = new Dictionary<object, object>
                    {
                        { "messageType", messageType }
                    };

                    // AQUÍ SE LLAMARÍA A LA CLASE PARA PROCESAR LOS DATOS
                    // En función del switch, obtengo unos datos u otros, y los envío en JSON
                    switch (messageType)
                    {
                        case MessageType.Friend:
                            var friendlist = await GetFriendList(scope);
                            dict.Add("friends", friendlist);
                            break;
                        case MessageType.Stats:
                            await WebSocketHandler.SendStatsMessage();
                            break;
                        case MessageType.PendingBattle:
                            var battleList = await battleService.GetPendingBattlesByUserIdAsync(User.Id);
                            var battleListDto = battleMapper.ToDto(battleList);
                            dict.Add("battles", battleListDto);
                            break;
                        case MessageType.ShuffleDeckStart:
                            // Cambio el estado a jugando
                            var state = await unitOfWork.StateRepositoty.GetByIdAsync(3);

                            User.StateId = state.Id;
                            User.State = state;

                            unitOfWork.UserRepository.Update(User);
                            await unitOfWork.SaveAsync();

                            Battle currentBattle = await battleService.GetCurrentBattleByUserAsync(User.Id);
                            if (currentBattle == null)
                            {
                                Console.WriteLine("Si es nulo a tomar por culo");
                                continue;
                            }

                            var valorant = await GayNetwork.StartGame(currentBattle, User, unitOfWork, _serviceProvider);
                            Console.WriteLine("¿Qué es VALORANT? 😨");
                            dict.Add("userBattleDto", valorant);
                            dict.Add("currentBattle", currentBattle);
                            break;
                        case MessageType.PlayCard:
                            Models.DTOs.Action action = dictInput["action"] as Models.DTOs.Action;
                            await GayNetwork.PlayCard(action, User.Id, unitOfWork, _serviceProvider);
                            break;
                        case MessageType.Chat:
                            var messageReceived = dictInput["messageChat"] as string;
                            Battle currentChatBattle = await battleService.GetCurrentBattleByUserAsync(User.Id);
                            if (!currentChatBattle.IsAgainstBot)
                            {
                                UserBattle enemyUser = currentChatBattle.BattleUsers.First(u => User.Id != u.UserId);
                                UserBattle chatSender = currentChatBattle.BattleUsers.First(u => User.Id == u.UserId);
                                dict.Add("messageChat", messageReceived);
                                dict.Add("who", chatSender.User.Nickname);
                                    await WebSocketHandler.NotifyOneUser(JsonSerializer.Serialize(dict, options), enemyUser.UserId);
                                Console.WriteLine($"Enviando mensaje a {enemyUser.User.Nickname}: {messageReceived}");
                            }
                            break;
                    }

                    if (dict.Values.Count > 1)
                    {

                        string outMessage = System.Text.Json.JsonSerializer.Serialize(dict, options);
                        // Procesamos el mensaje
                        //string outMessage = $"[{string.Join(", ", message as IEnumerable<char>)}]";

                        // Enviamos respuesta al cliente
                        await SendAsync(outMessage);
                    }

                    scope.Dispose();
                }
            }
            catch (Exception e)
            {
                if (e is System.FormatException)
                {
                    continue;
                }
                Console.WriteLine($"Error: {e}");
            }
            // Leemos el mensaje

        }

        // Si hay suscriptores al evento Disconnected, gestionamos el evento
        if (Disconnected != null)
        {
            await Disconnected.Invoke(this);
        }
    }

    public Dictionary<object, object> GetActionMessage(string message)
    {
        Dictionary<object, object> dict = new Dictionary<object, object>
        {
            { "messageType", -1 }
        };

        try
        {
            JsonSerializerOptions options = new JsonSerializerOptions(JsonSerializerDefaults.Web);

            JsonDocument dxoc = JsonDocument.Parse(message);
            JsonElement elem = dxoc.RootElement;

            MessageType messageType = (MessageType)elem.GetProperty("messageType").GetInt32();
            dict["messageType"] = messageType;

            if (elem.TryGetProperty("action", out JsonElement actionElement))
            {
                actionElement = elem.GetProperty("action");

                CardToPlay cardToPlay = actionElement.GetProperty("card").Deserialize<CardToPlay>();
                ActionType actionType = actionElement.GetProperty("actionType").Deserialize<ActionType>();

                Models.DTOs.Action action = actionElement.Deserialize<Models.DTOs.Action>();
                action.Card = cardToPlay;
                action.ActionType = actionType;

                dict.Add("action", action);
            }
            else
            {
                JsonElement chatElement = elem.GetProperty("messageChat");

                string messageChat = chatElement.GetString();
                dict.Add("messageChat", messageChat);
            }

        }
        catch
        {
            int messageTypeInt = int.Parse(message);
            MessageType messageType = (MessageType)messageTypeInt;
            dict["messageType"] = messageType;
        }

        return dict;

    }

    public async Task<ICollection<Friendship>> GetFriendList(IServiceScope scope)
    {
        FriendshipService friendshipService = scope.ServiceProvider.GetRequiredService<FriendshipService>();
        var friendList = await friendshipService.GetFriendList(User.Id);
        return friendList;
    }

    // TANTO READ COMO SEND SON COMUNES, Y SIEMPRE ENVÍAN Y RECIBEN STRINGS EN FORMATO JSON
    // READ RECIBIRÍA EL TIPO DEL MENSAJE (por ejemplo, que quiero info de las partidas), Y HABRÍA UNA O VARIAS CLASES QUE OBTENGA LOS DATOS QUE QUIERE Y ENVÍE LO NECESARIO
    private async Task<string> ReadAsync(CancellationToken cancellation = default)
    {
        // Creo un buffer para almacenar temporalmente los bytes del contenido del mensaje
        byte[] buffer = new byte[4096];
        // Creo un StringBuilder para poder ir creando poco a poco el mensaje en formato texto
        StringBuilder stringBuilder = new StringBuilder();
        // Creo un booleano para saber cuándo termino de leer el mensaje
        bool endOfMessage = false;

        do
        {
            // Recibo el mensaje pasándole el buffer como parámetro
            WebSocketReceiveResult result = await Socket.ReceiveAsync(buffer, cancellation);

            // Si el resultado que se ha recibido es de tipo texto lo decodifico y lo meto en el StringBuilder
            if (result.MessageType == WebSocketMessageType.Text)
            {
                // Decodifico el contenido recibido
                string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                // Lo añado al StringBuilder
                stringBuilder.Append(message);
            }
            // Si el resultado que se ha recibido entonces cerramos la conexión
            else if (result.CloseStatus.HasValue)
            {
                // Cerramos la conexión
                await Socket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, cancellation);
            }

            // Guardamos en nuestro booleano si hemos recibido el final del mensaje
            endOfMessage = result.EndOfMessage;
        }
        // Repetiremos iteración si el socket permanece abierto y no se ha recibido todavía el final del mensaje
        while (Socket.State == WebSocketState.Open && !endOfMessage);

        return stringBuilder.ToString();

        // Finalmente devolvemos el contenido del StringBuilder
        //return stringBuilder.ToString();
    }

    public Task SendAsync(string message, CancellationToken cancellation = default)
    {
        // Codificamos a bytes el contenido del mensaje
        byte[] bytes = Encoding.UTF8.GetBytes(message);

        // Enviamos los bytes al cliente marcando que el mensaje es un texto
        return Socket.SendAsync(bytes, WebSocketMessageType.Text, true, cancellation);
    }

    public Task SendBlobAsync(byte[] message, CancellationToken cancellation = default)
    {
        // Enviamos los bytes al cliente marcando que el mensaje es un texto
        return Socket.SendAsync(message, WebSocketMessageType.Binary, true, cancellation);
    }
}
