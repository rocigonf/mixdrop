using mixdrop_back.Models.Entities;
using mixdrop_back.Models.Mappers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace mixdrop_back.Sockets;
// SLAY QUEEN 💅✨
public class GayHandler // GameHandler :3
{
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
    private readonly ICollection<UserBattle> _participants = new List<UserBattle>();
    public int _timmyBattleId = 0;
    private static ICollection<Card> _cards = new List<Card>();

    private Dictionary<object, object> dict = new Dictionary<object, object>
    {
        { "messageType", MessageType.AskForBattle }
    };

    /// <summary>
    /// Método que agrega participantes a la batalla
    /// </summary>
    /// <returns>Nada (por ahora)</returns>
    public async Task<UserBattleDto> AddParticipant(Battle battle, int userId, UnitOfWork unitOfWork) //💀💀💀💀
    {

        await _semaphore.WaitAsync();

        UserBattle player = battle.BattleUsers.FirstOrDefault(user => user.UserId == userId);
        if (_participants.Contains(player) || _participants.Count == 2)
        {
            return null;
        }

        if (_cards.Count == 0)
        {
            _cards = await unitOfWork.CardRepository.GetAllCardsAsync();
        }

        Random rand = new Random(); // Obtiene 5 cartas aleatorias
        UserBattleMapper mapper = new UserBattleMapper();

        for (int i = 0; i < 5; i++)
        {
            Card card = _cards.ElementAt(rand.Next(0, _cards.Count));
            player.Cards.Add(card);
        }

        // asigna turno
        if (_participants.Count == 0)
        {
            player.IsTheirTurn = false;
        }
        if (_participants.Count == 1)
        {
            player.IsTheirTurn = true;
        }

        _participants.Add(player);

        _semaphore.Release();
        return mapper.ToDto(player);
    }

    public async Task<UserBattleDto> TurnPlayed(Battle battle, int userId) //💀💀💀💀
    {

        UserBattle player = battle.BattleUsers.FirstOrDefault(user => user.UserId == userId);

        UserBattleMapper mapper = new UserBattleMapper();

        if (player.IsTheirTurn)
        {
            player.IsTheirTurn = false;
        }
        else
        {
            player.IsTheirTurn = true;
        }


        // mira a ver quien es su compañero para notificarle y dale el turno
        var players = battle.BattleUsers.ToList();


        var player2id = 0;
        UserBattle player2 = null;

        if (player.Id == players[0].Id)
        {
            player2 = battle.BattleUsers.FirstOrDefault(user => user.UserId == players[1].UserId);
            players[1].IsTheirTurn = !players[1].IsTheirTurn;
            player2id = players[1].UserId;
        }
        else if (player.Id == players[1].Id)
        {
            player2 = battle.BattleUsers.FirstOrDefault(user => user.UserId == players[0].UserId);
            players[0].IsTheirTurn = !players[0].IsTheirTurn;
            player2id = players[0].UserId;
        }

        if (player2id != 0)
        {
            // notifica al otro jugador
            JsonSerializerOptions options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
            options.ReferenceHandler = ReferenceHandler.IgnoreCycles;

            dict["messageType"] = MessageType.TurnPlayed;
            dict.Add("turn", player2);
            await WebSocketHandler.NotifyOneUser(JsonSerializer.Serialize(dict, options), player2id);
        }



        return mapper.ToDto(player);

    }
}
