using mixdrop_back.Models.Entities;

namespace mixdrop_back.Sockets;
// SLAY QUEEN 💅✨
public class GayHandler // GameHandler :3
{
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
    private readonly ICollection<UserBattle> _participants = new List<UserBattle>();
    public int _timmyBattleId = 0;
    private static ICollection<Card> _cards = new List<Card>();

    /// <summary>
    /// Método que agrega participantes a la batalla
    /// </summary>
    /// <returns>Nada (por ahora)</returns>
    public async Task<ICollection<Card>> AddParticipant(Battle battle, int userId, UnitOfWork unitOfWork) //💀💀💀💀
    {

        await _semaphore.WaitAsync();

        UserBattle player = battle.BattleUsers.FirstOrDefault(user => user.UserId == userId);
        if (_participants.Contains(player))
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

        _participants.Append(player);

        _semaphore.Release();
        return player.Cards;
    }
}
