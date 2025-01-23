namespace mixdrop_back.Models.Entities;

public class UserBattle
{
    public int Id { get; set; }
    public int Punctuation { get; set; }
    public long TimePlayed { get; set; }
    public int UserId { get; set; }
    public int BattleId { get; set; }
    public int BattleRoleId { get; set; }
    public int BattleResultId { get; set; }
    public bool Receiver { get; set; }
    public User User { get; set; }
    public Battle Battle { get; set; }
    public BattleRole BattleRole { get; set; }
    public BattleResult BattleResult { get; set; }
}
