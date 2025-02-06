using mixdrop_back.Models.Entities;

namespace mixdrop_back.Models.DTOs;

public class Board
{
    public Slot[] Slots { get; set; } = new Slot[4];
    public Track Playing { get; set; }
}
