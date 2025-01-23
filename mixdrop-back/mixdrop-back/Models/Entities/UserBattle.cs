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
}
