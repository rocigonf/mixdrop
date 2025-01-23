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
        /*public async Task<Friendship> GetAllFriendshipsAsync(int friendshipId)
        {
            return await GetQueryable()
                .Include(friendship => friendship.SenderUser)
                .Include(f => f.ReceiverUser)
                .FirstOrDefaultAsync(friendship => friendship.Id == friendshipId);
        }*/

        public async Task<ICollection<Friendship>> GetFriendshipsByUserAsync(int userId)
        {
            return await GetQueryable()
                .Include(friendship => friendship.SenderUser)
                .Include(f => f.ReceiverUser)
                .ToListAsync();
        }
    }
}
