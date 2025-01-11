namespace mixdrop_back.Models;

public class Battle
{
    public int Id { get; set; }
    public ICollection<UserBattle> BattleUsers { get; set; } = new List<UserBattle>();
    public ICollection<BattleCart> BattleCarts { get; set; } = new List<BattleCart>();
}
