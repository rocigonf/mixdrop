namespace mixdrop_back.Models.Entities;

public class Battle
{
    public int Id { get; set; }
    public bool Accepted { get; set; } = false;
    public bool IsPlaying { get; set; } = false;

    public ICollection<UserBattle> BattleUsers { get; set; } = new List<UserBattle>();
    public ICollection<BattleCard> BattleCarts { get; set; } = new List<BattleCard>();
}
