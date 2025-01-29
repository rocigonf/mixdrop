namespace mixdrop_back.Models.Entities;

public class BattleCard
{
    public int Id { get; set; }
    public int CardId { get; set; }
    public int BattleId { get; set; }
    public Card Card { get; set; }
    public Battle Battle { get; set; }
}
