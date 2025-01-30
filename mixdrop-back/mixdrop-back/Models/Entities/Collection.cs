﻿namespace mixdrop_back.Models.Entities;

public class Collection
{
    public int Id { get; set; }
    public string Name { get; set; }
    public ICollection<Card> Cards { get; set; } = new List<Card>();
}
