namespace mixdrop_back.Models.Entities;

public class UserBattleDto
{
    public ICollection<Card> Cards { get; set; } = new List<Card>();
    public bool IsTheirTurn { get; set; } = false;
}
