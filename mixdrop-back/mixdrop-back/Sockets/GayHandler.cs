using System.Text.Json.Serialization;
using System.Text.Json;
using mixdrop_back.Models.DTOs;
using mixdrop_back.Models.Entities;
using mixdrop_back.Models.Mappers;
using Action = mixdrop_back.Models.DTOs.Action;

namespace mixdrop_back.Sockets;
// SLAY QUEEN 💅✨
public class GayHandler // GameHandler :3
{
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
    private readonly ICollection<UserBattle> _participants = new List<UserBattle>();
    public int _timmyBattleId = 0;

    // Lista obtenida de la base de datos
    private static ICollection<Card> _cards = new List<Card>();

    private readonly Board _board = new Board();

    private int _totalActions { get; set; } = 0;
    private int _totalTurns { get; set; } = 0;

    private UserBattleMapper _mapper = new UserBattleMapper();

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

        for (int i = 0; i < 5; i++)
        {
            Card card = _cards.ElementAt(rand.Next(0, _cards.Count));
            player.Cards.Add(card);
        }

        if (_participants.Count + 1 == 2)
        {
            player.IsTheirTurn = true;
        }

        _participants.Append(player);

        _semaphore.Release();
        return _mapper.ToDto(player);
    }

    // TODO: Mezcla de audio
    public async Task PlayCard(Action action, int userId)
    {
        await _semaphore.WaitAsync();

        int total = 0;
        bool wasEmpty = false;

        UserBattle playerInTurn = _participants.FirstOrDefault(u => u.UserId == userId && u.IsTheirTurn);

        if(playerInTurn == null)
        {
            Console.WriteLine("No le toca a este jugador");
            return;
        }

        for (int i = 0; i < action.Cards.Length; i++)
        {
            CardToPlay card = action.Cards[i];

            bool exists = _cards.Contains(card.Card);
            if(!exists)
            {
                Console.WriteLine("La carta no existe");
                return;
            }

            if(!playerInTurn.Cards.Contains(card.Card))
            {
                Console.WriteLine("El usuario no tiene la carta");
                return;
            }

            Slot slut = _board.Slots.ElementAt(card.Position);
            if (slut == null)
            {
                wasEmpty = true;
            }
            else
            {
                if (slut.Card.Level > card.Card.Level)
                {
                    Console.WriteLine("El nivel de la carta jugada es inferior");
                    return;
                }
            }

            bool isCorrectType = true;
            string partName = card.Card.Track.Part.Name;

            switch (card.Position)
            {
                case 0:
                    isCorrectType = CheckCardType([ "Voz", "Piano" ], partName);
                    break;
                case 1:
                    isCorrectType = CheckCardType(["Piano"], partName);
                    break;
                case 2:
                    isCorrectType = CheckCardType(["Guitarra", "Batería" ], partName);
                    break;
                case 3:
                    isCorrectType = CheckCardType(["Batería"], partName);
                    break;
                default:
                    Console.WriteLine("La posición no es correcta");
                    return;
            }

            if (!isCorrectType)
            {
                Console.WriteLine("El tipo de la carta no es el correcto");
                return;
            }

            // Si todo está correcto, establezco la nueva carta y la borro del mazo
            slut.Card = card.Card;
            playerInTurn.Cards.Remove(card.Card);

            total++;

            if(total == 2)
            {
                break;
            }
        }

        if(total < 2)
        {
            // TODO: Implementar acciones
            for (int i = 0; i < action.ActionsType.Length; i++)
            {
                ActionType actionType = action.ActionsType[i];
                switch (actionType.Name)
                {
                }
            }

        }

        _totalActions++;
        if(_totalActions % 2 == 0)
        {
            _totalTurns++;
            if(wasEmpty) { playerInTurn.Punctuation++; }
        }

        playerInTurn.Punctuation += 1;

        Dictionary<object, object> dict = new Dictionary<object, object>
        {
            { "messageType", MessageType.TurnResult },
            { "board", _board },
            { "player", _mapper.ToDto(playerInTurn) }
        };

        UserBattle otherUser = _participants.FirstOrDefault(u => u.UserId != userId);
        playerInTurn.IsTheirTurn = false;
        otherUser.IsTheirTurn = true;

        JsonSerializerOptions options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.ReferenceHandler = ReferenceHandler.IgnoreCycles;

        // TODO: Terminar batalla
        if(playerInTurn.Punctuation == 21)
        {

        }

        await WebSocketHandler.NotifyOneUser(JsonSerializer.Serialize(dict, options), playerInTurn.UserId);

        dict["player"] = otherUser;
        await WebSocketHandler.NotifyOneUser(JsonSerializer.Serialize(dict, options), otherUser.UserId);

        _semaphore.Release();
    }

    public bool CheckCardType(List<string> possibleTypes, string actualType)
    {
        return possibleTypes.Contains(actualType);
    }
}
