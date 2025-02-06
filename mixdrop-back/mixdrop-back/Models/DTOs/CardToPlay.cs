using mixdrop_back.Models.Entities;

namespace mixdrop_back.Models.DTOs;

public class CardToPlay
{
    public Card Card { get; set; }
    public int Position { get; set; }
}
