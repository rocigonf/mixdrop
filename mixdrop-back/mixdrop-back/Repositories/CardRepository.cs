﻿using Microsoft.EntityFrameworkCore;
using mixdrop_back.Models.Entities;
using mixdrop_back.Repositories.Base;

namespace mixdrop_back.Repositories
{
    public class CardRepository : Repository<Card, int>
    {
        public CardRepository(MixDropContext context) : base(context)
        {
        }

        public async Task<ICollection<Card>> GetAllCardsAsync()
        {
            return await GetQueryable()
                .Include(card => card.Track)
                    .ThenInclude(track => track.Song)
                .Include(card => card.Track)
                    .ThenInclude(track => track.Part)
                .Include(card => card.CardType)
                .Include(card => card.Collection)
                .ToListAsync();
        }
    }
}
