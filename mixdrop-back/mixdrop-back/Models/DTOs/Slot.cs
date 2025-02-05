using mixdrop_back.Models.Entities;

namespace mixdrop_back.Models.DTOs;

public class Slot
{
    public Card Card { get; set; }
    public CardType CardType { get; set; }
}
