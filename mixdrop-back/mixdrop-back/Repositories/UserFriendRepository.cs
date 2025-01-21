using Microsoft.EntityFrameworkCore;
using mixdrop_back.Models.Entities;
using mixdrop_back.Repositories.Base;

namespace mixdrop_back.Repositories
{
    public class UserFriendRepository : Repository<UserFriend, int>
    {
        public UserFriendRepository(MixDropContext context) : base(context) { }
    }
}
