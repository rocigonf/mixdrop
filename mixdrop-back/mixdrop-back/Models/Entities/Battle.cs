namespace mixdrop_back.Models.Entities;

public class Battle
{
    public int Id { get; set; }
    public bool IsAgainstBot { get; set; } = false;
    public BattleState BattleState { get; set; }
    public int BattleStateId { get; set; }
    public ICollection<UserBattle> BattleUsers { get; set; } = new List<UserBattle>();
    public DateTime CreatedAt { get; set; }
    public DateTime FinishedAt { get; set; } = DateTime.UtcNow.AddMonths(1); // Valor por defecto
}
