namespace mixdrop_back.Models.DTOs;

public class Action
{
    public CardToPlay[] Cards { get; set; } = new CardToPlay[2];
    public ActionType[] ActionsType { get; set; } = new ActionType[2];
}
