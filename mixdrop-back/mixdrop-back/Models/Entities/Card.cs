﻿namespace mixdrop_back.Models.Entities;

public class Card
{
    public int Id { get; set; }
    public string ImagePath { get; set; }
    public int Level { get; set; }
    public int TrackId { get; set; }
    public int CardTypeId { get; set; }
    public int CollectionId { get; set; }
    public Track Track { get; set; }
    public CardType CardType { get; set; } // Si es comodín o no
    public Collection Collection { get; set; }
    public ICollection<BattleCard> BattleCards { get; set; } = new List<BattleCard>();
}
