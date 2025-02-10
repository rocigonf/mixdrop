namespace mixdrop_back.Models.Entities;

public class Battle
{
    public int Id { get; set; }
    public bool IsAgainstBot { get; set; } = false;
    public BattleState BattleState { get; set; }
    public int BattleStateId { get; set; }
    public ICollection<UserBattle> BattleUsers { get; set; } = new List<UserBattle>();
    public ICollection<BattleCard> BattleCarts { get; set; } = new List<BattleCard>();
}
