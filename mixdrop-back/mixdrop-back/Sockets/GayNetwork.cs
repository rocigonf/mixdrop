using mixdrop_back.Models.DTOs;
using mixdrop_back.Models.Entities;

namespace mixdrop_back.Sockets;

public class GayNetwork // GameNetwork :3
{

  private static readonly ICollection<GayHandler> _handlers = new List<GayHandler>();
    
    public static readonly ICollection<GayHandler> _handlers = new List<GayHandler>();
    private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

    public static async Task<UserBattleDto> StartGame(Battle battle, User user, UnitOfWork unitOfWork) // 🦙📲🦙🔥
    {
        GayHandler handler = _handlers.FirstOrDefault(warden => warden._timmyBattleId == battle.Id);
        if (handler == null)
        {
            handler = new GayHandler();
            handler._timmyBattleId = battle.Id;
            _handlers.Add(handler);
        }
        
        return await handler.AddParticipant(battle, user.Id, unitOfWork);
    }

    // jugar un turno (se acaba su turno)
    public static async Task<UserBattleDto> PlayTurn(Battle battle, User user){

        GayHandler handler = _handlers.FirstOrDefault(warden => warden._timmyBattleId == battle.Id);
        if (handler == null)
        {
            Console.WriteLine("no hay batalla loco, con quien juegas");
            return null;
        }       

        return await handler.TurnPlayed(battle, user.Id);   

    }
  

    public static async Task<UserBattleDto> StartGame(Battle battle, User user, UnitOfWork unitOfWork) // 🦙📲🦙🔥
    {
        await _semaphore.WaitAsync();

        GayHandler handler = _handlers.FirstOrDefault(warden => warden.Battle.Id == battle.Id);
        if (handler == null)
        {
            handler = new GayHandler();
            handler.Battle = battle;
            _handlers.Add(handler);
        }

        _semaphore.Release();

        return await handler.AddParticipant(battle, user.Id, unitOfWork);
    }

    public static async Task PlayCard(Models.DTOs.Action action, int userId, UnitOfWork unitOfWork)
    {
        await _semaphore.WaitAsync();

        GayHandler handler = _handlers
            .Where(handler => handler._participants.Any(u => u.UserId == userId))
            .FirstOrDefault();

        if (handler == null)
        {
            Console.WriteLine("No existe un handler para este usuario");
            _semaphore.Release();
            return;
        }
        else
        {
            await handler.PlayCard(action, userId, unitOfWork);
        }

        _semaphore.Release();
    }

}
