using mixdrop_back.Models.Entities;

namespace mixdrop_back.Models.DTOs;

public class BattleDto
{
    public int Id { get; set; }
    public User User { get; set; }
    public int UserId { get; set; }
    public int BattleStateId { get; set; }
    public bool IsAgainstBot { get; set; }
    public IEnumerable<UserBattleDto> UsersBattles { get; set; } = new List<UserBattleDto>();
    public DateTime CreatedAt { get; set; }
    public DateTime FinishedAt { get; set; }
}
