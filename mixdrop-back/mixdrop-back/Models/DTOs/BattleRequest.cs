using mixdrop_back.Models.Entities;

namespace mixdrop_back.Models.DTOs;

public class BattleRequest
{
    public User User2 { get; set; }

    public bool IsRandom = false;
}
