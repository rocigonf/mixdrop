using mixdrop_back.Models.Entities;
using mixdrop_back.Models.Mappers;

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
    private int _totalTurns { get; set; }

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

        if (_participants.Count + 1 == 2)
        {
            player.IsTheirTurn = true;
        }

        _participants.Append(player);

        _semaphore.Release();
        return mapper.ToDto(player);
    }
}
