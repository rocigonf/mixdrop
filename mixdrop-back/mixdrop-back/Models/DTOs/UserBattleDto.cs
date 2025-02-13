using mixdrop_back.Models.Entities;

namespace mixdrop_back.Models.DTOs;

public class UserBattleDto
{
    public ICollection<Card> Cards { get; set; } = new List<Card>();
    public bool IsTheirTurn { get; set; } = false;
    public int Punctuation { get; set; }
    public int BattleResultId { get; set; }
    public int ActionsLeft { get; set; } = 0;
}
