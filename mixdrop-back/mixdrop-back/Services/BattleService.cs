using mixdrop_back.Models.DTOs;
using mixdrop_back.Models.Entities;
using mixdrop_back.Models.Mappers;
using mixdrop_back.Sockets;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace mixdrop_back.Services;

public class BattleService
{
    private readonly UnitOfWork _unitOfWork;
    private readonly Dictionary<object, object> dict = new Dictionary<object, object>
    {
        { "messageType", MessageType.AskForBattle }
    };
    //private SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

    public BattleService(UnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task CreateBattle(User user1, int user2Id = 0, bool isRandom = false)
    {
        //await _semaphore.WaitAsync();

        BattleState battleState = await _unitOfWork.BattleStateRepository.GetByIdAsync(1);

        Battle battle = new Battle() { CreatedAt = DateTime.UtcNow };
        if (user2Id != 0)
        {
            // Si es random significa que deben pelear directamente
            if (isRandom)
            {
                dict["messageType"] = MessageType.StartBattle;
                battle.BattleStateId = 3;
                battleState = await _unitOfWork.BattleStateRepository.GetByIdAsync(3);
            }

            // q pueda ser nulo el user 2 porque puede ser el bot
            User existingUser = await _unitOfWork.UserRepository.GetByIdAsync(user2Id);

            if (existingUser == null)
            {
                Console.WriteLine("Este usuario 2 no existe.");
                return;
            }
        }
        else
        {
            /*var existingBattle = _unitOfWork.BattleRepository.GetByIdAsync(user1.Id);

            if(existingBattle != null)
            {
                Console.WriteLine("Ya existe una batalla");
                return;
            }*/

            await Task.Delay(1000);
            // Si es contra un bot, se acepta y se pone como jugando
            dict["messageType"] = MessageType.StartBattle;
            battle.BattleStateId = 3;
            battle.IsAgainstBot = true;
            battleState = await _unitOfWork.BattleStateRepository.GetByIdAsync(3);
        }

        if (user2Id != 0)
        {
            // q si estan en batalla los jugadores, no pueda estar en otra
            Battle existingBattle = await _unitOfWork.BattleRepository.GetBattleByUsersAsync(user1.Id, user2Id);
            if (existingBattle != null)
            {
                Console.WriteLine("Ya hay una batalla entre ambos usuarios");
                return;
            }
        }


        // adaptar websocket a esto tamb

        battle.BattleState = battleState;

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


        if (user2Id != 0)
        {
            UserBattle newUserBattle2 = new UserBattle
            {
                BattleId = newBattle.Id,
                UserId = user2Id,
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
        await WebSocketHandler.NotifyOneUser(JsonSerializer.Serialize(dict, options), user1.Id);
        if (user2Id != 0)
        {
            await WebSocketHandler.NotifyOneUser(JsonSerializer.Serialize(dict, options), user2Id);
        }

        //_semaphore.Release();
    }


    // Método solicitud de batalla
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

        // Porque este método sirve también para mostrar batallas que, si bien estaban aceptadas, no han comenzado
        if (existingBattle.BattleStateId == 1)
        {
            existingBattle.BattleStateId = 2;
            _unitOfWork.BattleRepository.Update(existingBattle);
            await _unitOfWork.SaveAsync();
        }

        JsonSerializerOptions options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.ReferenceHandler = ReferenceHandler.IgnoreCycles;

        UserBattle sender = existingBattle.BattleUsers.FirstOrDefault(user => user.Receiver == false);

        dict.Add("battleId", existingBattle.Id);

        dict["messageType"] = MessageType.Play;
        await WebSocketHandler.NotifyOneUser(JsonSerializer.Serialize(dict, options), receiverUser.UserId);

        dict.Add("battle", existingBattle);
        await WebSocketHandler.NotifyOneUser(JsonSerializer.Serialize(dict, options), sender.UserId);

    }

    public async Task StartBattle(int battleId, int userId)
    {
        Battle existingBattle = await _unitOfWork.BattleRepository.GetCompleteBattleAsync(battleId);
        if (existingBattle == null)
        {
            Console.WriteLine("Esta solicitud no existe");
            return;
        }

        UserBattle receiverUser = existingBattle.BattleUsers.FirstOrDefault(user => user.Receiver == false);
        if (receiverUser.UserId != userId)
        {
            Console.WriteLine("Este usuario no es el anfitrión");
            return;
        }

        existingBattle.BattleStateId = 3;

        _unitOfWork.BattleRepository.Update(existingBattle);
        await _unitOfWork.SaveAsync();

        JsonSerializerOptions options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.ReferenceHandler = ReferenceHandler.IgnoreCycles;

        UserBattle sender = existingBattle.BattleUsers.FirstOrDefault(user => user.Receiver == true);

        dict["messageType"] = MessageType.StartBattle;
        await WebSocketHandler.NotifyOneUser(JsonSerializer.Serialize(dict, options), receiverUser.UserId);
        await WebSocketHandler.NotifyOneUser(JsonSerializer.Serialize(dict, options), sender.UserId);

    }

    // Método borrar batalla o rechazar solicitud de batalla
    public async Task DeleteBattleById(int battleId, int userId, bool notify)
    {
        // Comprobamos que la batalla existe
        Battle existingBattle = await _unitOfWork.BattleRepository.GetCompleteBattleAsync(battleId);
        if (existingBattle == null)
        {
            Console.WriteLine("Esta batalla no existe :(");
            return;
        }

        await DeleteBattleByObject(existingBattle, userId, notify);
    }

    public async Task DeleteBattleByObject(Battle existingBattle, int userId, bool notify = false, bool check = true)
    {
        if (check)
        {
            // Comprobamos que el usuario es parte de la batalla
            UserBattle userBattle = existingBattle.BattleUsers.FirstOrDefault(user => user.UserId == userId);
            if (userBattle == null)
            {
                Console.WriteLine("Este usuario no forma parte de esta batalla");
                return;
            }
        }

        GayHandler handler = GayNetwork._handlers.FirstOrDefault(h => h._participants.Any(u => u.UserId == userId));
        GayNetwork._handlers.Remove(handler);

        _unitOfWork.BattleRepository.Delete(existingBattle);
        await _unitOfWork.SaveAsync();

        JsonSerializerOptions options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.ReferenceHandler = ReferenceHandler.IgnoreCycles;

        UserBattle otherUser = existingBattle.BattleUsers.FirstOrDefault(user => user.UserId != userId);

        // Por si es un bot
        if (otherUser != null)
        {
            if(notify)
            {
                dict["messageType"] = MessageType.DisconnectedFromBattle;
            }

            // Notifico a ambos usuarios a que vuelvan a solicitar las peticiones de batallas
            await Task.WhenAll(
                WebSocketHandler.NotifyOneUser(JsonSerializer.Serialize(dict, options), otherUser.UserId),
                WebSocketHandler.NotifyOneUser(JsonSerializer.Serialize(dict, options), userId));
        }
    }

    public async Task DeleteBattleAgainstBot(User user)
    {
        // Comprobamos que la batalla existe
        Battle existingBattle = await _unitOfWork.BattleRepository.GetBattleWithBotByUserAsync(user.Id);
        if (existingBattle == null)
        {
            Console.WriteLine("Esta batalla no existe :(");
            return;
        }

        await ChangeUserStateAsync(user, 2);
        await DeleteBattleByObject(existingBattle, user.Id, false, false);
    }

    public async Task ForfeitBattle(User user)
    {
        ICollection<Battle> battles = await _unitOfWork.BattleRepository.GetCurrentBattleByUserWithoutThem(user.Id);
        Battle battle = battles.FirstOrDefault();

        if(battle == null || battle.IsAgainstBot)
        {
            return;
        }

        UserBattle winner = battle.BattleUsers.FirstOrDefault(u => u.UserId != user.Id);
        UserBattle loser = battle.BattleUsers.FirstOrDefault(u => u.UserId == user.Id);

        await ChangeUserStateAsync(user, 2);
        await EndBattle(battle, winner, loser);
    }

    // Esto se podría reutilizar en el timer y en el GayHandler
    public async Task EndBattle(Battle battle, UserBattle winner, UserBattle loser)
    {
        try
        {
            if (battle.BattleStateId == 3)
            {
                //BattleState battleState = await _unitOfWork.BattleStateRepository.GetByIdAsync(4);
                //ICollection<BattleResult> results = await _unitOfWork.BattleResultRepository.GetAllAsync();

                //battle.BattleState = battleState;
                battle.BattleStateId = 4;
                battle.FinishedAt = DateTime.UtcNow;

                //BattleResult victory = results.FirstOrDefault(b => b.Name == "Victoria");
                //BattleResult defeat = results.FirstOrDefault(b => b.Name == "Derrota");

                battle.BattleUsers = [];

                if (!winner.IsBot)
                {
                    //winner.BattleResult = victory;
                    winner.BattleResultId = 1;
                    winner.Cards = new List<Card>();
                    battle.BattleUsers.Add(winner);

                    User userWinner = await _unitOfWork.UserRepository.GetByIdAsync(winner.UserId);
                    await ChangeUserStateAsync(userWinner, 2);
                }

                if (!loser.IsBot)
                {
                    //loser.BattleResult = defeat;
                    loser.BattleResultId = 2;
                    loser.Cards = new List<Card>();
                    battle.BattleUsers.Add(loser);
                }

                _unitOfWork.BattleRepository.Update(battle);
                await _unitOfWork.SaveAsync();

                GayHandler handler = GayNetwork._handlers.FirstOrDefault(h => h._participants.Any(u => u.UserId == winner.UserId));
                GayNetwork._handlers.Remove(handler);

                Dictionary<object, object> dict = new Dictionary<object, object>
                {
                    { "messageType", MessageType.DisconnectedFromBattle },
                    { "reported", false }
                };

                JsonSerializerOptions options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
                options.ReferenceHandler = ReferenceHandler.IgnoreCycles;

                await WebSocketHandler.NotifyOneUser(JsonSerializer.Serialize(dict, options), winner.UserId);
            }
        }
        catch(Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private async Task ChangeUserStateAsync(User user, int stateId)
    {
        // estado de conectado
        var state = await _unitOfWork.StateRepositoty.GetByIdAsync(stateId);

        user.StateId = state.Id; // conectado
        user.State = state;

        _unitOfWork.UserRepository.Update(user);
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
            await CreateBattle(user, userInQueue.Id, true);
            JsonSerializerOptions options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
            options.ReferenceHandler = ReferenceHandler.IgnoreCycles;

            //WebSocketHandler.TotalPlayers++;

            /*dict["messageType"] = MessageType.Play;
            await WebSocketHandler.NotifyOneUser(JsonSerializer.Serialize(dict, options), user.Id);
            await WebSocketHandler.NotifyOneUser(JsonSerializer.Serialize(dict, options), userInQueue.Id);*/
            Console.WriteLine("Partida encontrada B)");

        }
    }

    public async Task DeleteFromQueue(User user)
    {
        user.IsInQueue = false;
        _unitOfWork.UserRepository.Update(user);
        await _unitOfWork.SaveAsync();
    }
    public async Task<Battle> GetCurrentBattleByUserAsync(int userId)
    {
        ICollection<Battle> currentBattles = await _unitOfWork.BattleRepository.GetCurrentBattleByUser(userId);
        if (currentBattles.Count() > 1)
        {
            throw new Exception("Cagaste chiquiluqui");
        }
        else if (currentBattles.Count() == 0)
        {
            Console.WriteLine("Empanada con queso");
            return null;
        }
        return currentBattles.First();
    }

    public async Task<ICollection<Battle>> GetPendingBattlesByUserIdAsync(int userId)
    {
        ICollection<Battle> battles = await _unitOfWork.BattleRepository.GetPendingBattlesByUserIdAsync(userId);
        return battles;
    }

    public async Task<List<BattleDto>> GetBattleByIdAsync(int id)
    {
        Battle battle = await _unitOfWork.BattleRepository.GetCompleteBattleWithUsersAsync(id);

        if(battle == null)
        {
            return null;
        }
        
        BattleMapper battleMapper = new BattleMapper();

        List<BattleDto> battles = battleMapper.ToDtoWithAllInfo(new List<Battle> { battle });

        return battles;
    }
}
