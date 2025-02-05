using mixdrop_back.Models.Entities;


namespace mixdrop_back.Sockets;

public class GayNetwork // GameNetwork :3
{
    private static readonly ICollection<GayHandler> _handlers = new List<GayHandler>();
    
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



}
