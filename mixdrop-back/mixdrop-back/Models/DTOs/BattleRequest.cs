using mixdrop_back.Models.Entities;

namespace mixdrop_back.Models.DTOs;

public class BattleRequest
{
    public int User2Id { get; set; } = 0;

    public bool IsRandom = false;
}
