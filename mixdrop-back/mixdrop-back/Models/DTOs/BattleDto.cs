using mixdrop_back.Models.Entities;

namespace mixdrop_back.Models.DTOs;

public class BattleDto
{
    public int Id { get; set; }
    public User User { get; set; }
    public int BattleId { get; set; }
    public int UserId { get; set; }
    public int BattleStateId { get; set; }
}
