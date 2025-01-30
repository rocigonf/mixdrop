using mixdrop_back.Models.Entities;
using mixdrop_back.Sockets;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace mixdrop_back.Services;

public class BattleService
{
    private readonly UnitOfWork _unitOfWork;
    private Dictionary<object, object> dict = new Dictionary<object, object>
    {
        { "messageType", MessageType.AskForBattle }
    };

    public BattleService(UnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task CreateBattle(User user1, User user2 = null, bool isRandom = false)
    {
        Battle battle = new Battle();
        if (user2 != null)
        {
            // Si es random significa que deben pelear directamente
            if (isRandom)
            {
                dict["messageType"] = MessageType.Play;
                battle.BattleStateId = 3;
            }

            // q pueda ser nulo el user 2 porque puede ser el bot
            User existingUser = await _unitOfWork.UserRepository.GetByNicknameAsync(user2.Nickname);

            if (existingUser == null)
            {
                Console.WriteLine("Este usuario 2 no existe.");
                return;
            }
        }
        else
        {
            // Si es contra un bot, se acepta y se pone como jugando
            battle.BattleStateId = 3;
        }

        if (user2 != null)
        {
            // q si estan en batalla los jugadores, no pueda estar en otra
            Battle existingBattle = await _unitOfWork.BattleRepository.GetBattleByUsersAsync(user1.Id, user2.Id);
            if (existingBattle != null)
            {
                Console.WriteLine("Ya hay una batalla entre ambos usuarios");
                return;
            }
        }


        // adaptar websocket a esto tamb

        Battle newBattle = await _unitOfWork.BattleRepository.InsertAsync(battle);

        UserBattle newUserBattle1 = new UserBattle
        {
            BattleId = newBattle.Id,
            UserId = user1.Id,
            Receiver = false,
            BattleResultId = 1,
            Battle = newBattle,
            BattleResult = new BattleResult()
        };


        if (user2 != null)
        {
            UserBattle newUserBattle2 = new UserBattle
            {
                BattleId = newBattle.Id,
                UserId = user2.Id,
                Receiver = true,
                BattleResultId = 1,
                Battle = newBattle,
                BattleResult = new BattleResult()
            };

            await _unitOfWork.UserBattleRepository.InsertAsync(newUserBattle2);
        }

        await _unitOfWork.UserBattleRepository.InsertAsync(newUserBattle1);
        await _unitOfWork.SaveAsync();

        JsonSerializerOptions options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.ReferenceHandler = ReferenceHandler.IgnoreCycles;

        // Si el usuario 2 no existe, significa que va a pelear contra un bot, por lo que simplemente se le dirigiría desde el front a la batalla
        if (user2 != null)
        {
            await WebSocketHandler.NotifyOneUser(JsonSerializer.Serialize(dict, options), user1.Id);
            await WebSocketHandler.NotifyOneUser(JsonSerializer.Serialize(dict, options), user2.Id);
        }
    }


    // Método solicitud de batalla
    // TODO: Verificar que el usuario no esté ya en una batalla
    public async Task AcceptBattle(int battleId, int userId)
    {
        Battle existingBattle = await _unitOfWork.BattleRepository.GetCompleteBattleAsync(battleId);
        if (existingBattle == null)
        {
            Console.WriteLine("Esta solicitud no existe");
            return;
        }

        UserBattle receiverUser = existingBattle.BattleUsers.FirstOrDefault(user => user.Receiver == true);
        if (receiverUser.UserId != userId)
        {
            Console.WriteLine("Este usuario no es recibidor");
            return;
        }

        existingBattle.BattleStateId = 2;

        _unitOfWork.BattleRepository.Update(existingBattle);
        await _unitOfWork.SaveAsync();

        JsonSerializerOptions options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.ReferenceHandler = ReferenceHandler.IgnoreCycles;

        UserBattle sender = existingBattle.BattleUsers.FirstOrDefault(user => user.Receiver == false);

        dict["messageType"] = MessageType.Play;
        await WebSocketHandler.NotifyOneUser(JsonSerializer.Serialize(dict, options), sender.UserId);
        await WebSocketHandler.NotifyOneUser(JsonSerializer.Serialize(dict, options), receiverUser.UserId);
    }

    // Método borrar amigo o rechazar solicitud de batalla
    public async Task DeleteBattleById(int battleId, int userId, bool wasAlreadyInRoom = false)
    {
        // Comprobamos que la batalla existe
        Battle existingBattle = await _unitOfWork.BattleRepository.GetCompleteBattleAsync(battleId);
        if (existingBattle == null)
        {
            Console.WriteLine("Esta batalla no existe :(");
            return;
        }

        await DeleteBattleByObject(existingBattle, userId, wasAlreadyInRoom);
    }

    public async Task DeleteBattleByObject(Battle existingBattle, int userId, bool wasAlreadyInRoom)
    {
        // Comprobamos que el usuario es parte de la batalla
        UserBattle userBattle = existingBattle.BattleUsers.FirstOrDefault(user => user.UserId == userId);
        if (userBattle == null)
        {
            Console.WriteLine("Este usuario no forma parte de esta batalla");
            return;
        }

        _unitOfWork.BattleRepository.Delete(existingBattle);
        await _unitOfWork.SaveAsync();

        JsonSerializerOptions options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.ReferenceHandler = ReferenceHandler.IgnoreCycles;

        UserBattle otherUser = existingBattle.BattleUsers.FirstOrDefault(user => user.UserId != userId);

        // Si ya estaba en la sala, le digo al otro jugador que se ha desconectado
        if (wasAlreadyInRoom)
        {
            dict["messageType"] = MessageType.DisconnectedFromBattle;
        }

        // Notifico a ambos usuarios a que vuelvan a solicitar las peticiones de batallas
        await WebSocketHandler.NotifyOneUser(JsonSerializer.Serialize(dict, options), otherUser.UserId);
        await WebSocketHandler.NotifyOneUser(JsonSerializer.Serialize(dict, options), userId);
    }


    // Emparejamiento aleatorio
    public async Task RandomBattle(User user)
    {
        User userInQueue = await _unitOfWork.UserRepository.GetUserInQueueAsync();
        if (userInQueue == null)
        {
            user.IsInQueue = true;
            _unitOfWork.UserRepository.Update(user);
            await _unitOfWork.SaveAsync();
        }
        else
        {
            userInQueue.IsInQueue = false;
            _unitOfWork.UserRepository.Update(userInQueue);
            await CreateBattle(user, userInQueue, true);
            JsonSerializerOptions options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
            options.ReferenceHandler = ReferenceHandler.IgnoreCycles;

            dict["messageType"] = MessageType.Play;
            await WebSocketHandler.NotifyOneUser(JsonSerializer.Serialize(dict, options), user.Id);
            await WebSocketHandler.NotifyOneUser(JsonSerializer.Serialize(dict, options), userInQueue.Id);
            Console.WriteLine("Partida encontrada B)");
        }
    }

    public async Task<ICollection<Battle>> GetPendingBattlesByUserIdAsync(int userId)
    {
        ICollection<Battle> battles = await _unitOfWork.BattleRepository.GetPendingBattlesByUserIdAsync(userId);
        return battles;
    }
}
