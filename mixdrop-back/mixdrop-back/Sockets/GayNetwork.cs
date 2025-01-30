using mixdrop_back.Models.Entities;

namespace mixdrop_back.Sockets
{
    public class GayNetwork // GameNetwork :3
    {
        private static readonly ICollection<GayHandler> _handlers = new List<GayHandler>();
        
        public static GayHandler StartGame(Battle battle, User user)
        {
            // TODO: Agregar verificación de que no existe un gayhandler
            GayHandler gayHandler = new GayHandler();
            gayHandler._participants.Add("user1", battle.BattleUsers.First());
            gayHandler._participants.Add("user2", battle.BattleUsers.Last());

            gayHandler._timmyTurnerId = user.Id;

            _handlers.Add(gayHandler);

            // TODO: Repartir cartas

            return gayHandler;
        }
    }
}
