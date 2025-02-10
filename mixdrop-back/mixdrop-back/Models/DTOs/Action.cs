namespace mixdrop_back.Models.DTOs;

public class Action
{
    public ICollection<CardToPlay> Cards { get; set; } = new List<CardToPlay>();
    public ICollection<ActionType> ActionsType { get; set; } = new List<ActionType>();
}
