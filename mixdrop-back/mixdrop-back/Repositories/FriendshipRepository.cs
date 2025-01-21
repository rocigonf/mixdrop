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
                .Include(friendship => friendship.UserFriends
                    .Where(userFriend => userFriend.UserId == userId1)
                    .Where(userFriend => userFriend.UserId == userId2)
                )
                .FirstOrDefaultAsync();

        }
        public async Task<Friendship> GetAllFriendshipsAsync(int friendshipId)
        {
            return await GetQueryable()
                .Include(friendship => friendship.UserFriends)
                .FirstOrDefaultAsync(friendship => friendship.Id == friendshipId);

        }
    }
}
