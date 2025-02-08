using Microsoft.EntityFrameworkCore;
using mixdrop_back.Models.Entities;
using mixdrop_back.Repositories.Base;

namespace mixdrop_back.Repositories
{
    public class FriendshipRepository : Repository<Friendship, int>
    {
        public FriendshipRepository(MixDropContext context) : base(context) { }
        public async Task<Friendship> GetFriendshipAsync(int userId1, int userId2)
        {
            return await GetQueryable()
                .Include(friendship => friendship.SenderUser)
                .Include(f => f.ReceiverUser)
                .FirstOrDefaultAsync(f => f.SenderUserId == userId1 && f.ReceiverUserId == userId2);

        }

        /*public async Task<Friendship> GetFriendshipByIdAsync(int id)
        {
            return await GetQueryable()
                .Include(friendship => friendship.SenderUser)
                .Include(f => f.ReceiverUser)
                .FirstOrDefaultAsync(f => f.Id == id);

        }*/

        public async Task<ICollection<Friendship>> GetFriendshipsByUserAsync(int userId)
        {
            return await GetQueryable()
                .Include(friendship => friendship.SenderUser)
                    .ThenInclude(u => u.State)
                .Include(f => f.ReceiverUser)
                    .ThenInclude(u => u.State)
                .Where(f => f.SenderUserId == userId || f.ReceiverUserId == userId)
                .ToListAsync();
        }
    }
}
