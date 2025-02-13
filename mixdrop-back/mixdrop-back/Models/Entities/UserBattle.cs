namespace mixdrop_back.Models.Entities;

public class UserBattle
{
    public int Id { get; set; }
    public int Punctuation { get; set; } = 0;
    public long TimePlayed { get; set; } = 0;
    public int UserId { get; set; }
    public int BattleId { get; set; }
    public int BattleResultId { get; set; }
    public bool Receiver { get; set; }
    public User User { get; set; }
    public Battle Battle { get; set; }
    public BattleResult BattleResult { get; set; } = null;
    public bool IsBot { get; set; } = false;

    public ICollection<Card> Cards { get; set; } = new List<Card>();
    public bool IsTheirTurn { get; set; } = false;
    public int ActionsLeft { get; set; } = 0;
}
