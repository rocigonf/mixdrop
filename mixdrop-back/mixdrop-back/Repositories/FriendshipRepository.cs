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
                .Include(friendship => friendship.User1)
                .Include(f => f.User2)
                .FirstOrDefaultAsync(f => f.User1Id == userId1 && f.User2Id == userId2);

        }
        public async Task<Friendship> GetAllFriendshipsAsync(int friendshipId)
        {
            return await GetQueryable()
                .Include(friendship => friendship.User1)
                .Include(f => f.User2)
                .FirstOrDefaultAsync(friendship => friendship.Id == friendshipId);
        }

        public async Task<Friendship> GetFriendshipWithUserByIdAsync(int id)
        {
            return await GetQueryable()
                .Include(friendship => friendship.User1)
                .Include(f => f.User2)
                .FirstOrDefaultAsync(friendship => friendship.Id == id);
        }

        public async Task<ICollection<Friendship>> GetFriendshipyByUserIdAsync(int id)
        {
            return await GetQueryable()
                .Where(friendship => friendship.User1Id == id || friendship.User2Id == id)
                .Include(friendship => friendship.User1)
                .Include(f => f.User2)
                .ToListAsync();
        }
    }
}
