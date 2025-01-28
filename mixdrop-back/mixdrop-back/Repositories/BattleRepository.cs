using Microsoft.EntityFrameworkCore;
using mixdrop_back.Models.Entities;
using mixdrop_back.Repositories.Base;

namespace mixdrop_back.Repositories;

public class BattleRepository : Repository<Battle, int>
{
    public BattleRepository(MixDropContext context) : base(context) { }

    public async Task<Battle> GetBattleByUsersAsync(int userId1, int userId2)
    {
        return await GetQueryable()
            .Include(friendship => friendship.BattleUsers
                .Where(userFriend => userFriend.UserId == userId1)
                .Where(userFriend => userFriend.UserId == userId2)
            )
            .FirstOrDefaultAsync();
    }

    public async Task<Battle> GetCompleteBattleAsync(int battleId)
    {
        return await GetQueryable()
            .Include(battle => battle.BattleUsers)
            .FirstOrDefaultAsync(battle => battle.Id == battleId);

    }

    public async Task<ICollection<Battle>> GetPendingBattlesByUserIdAsync(int userId)
    {
        // Con el Any obtengo todas las batallas que incluyan al id del usuario :'D
        return await GetQueryable()
            .Where(battle => battle.Accepted == false)
            .Where(battle => battle.BattleUsers.Any(user => user.UserId == userId && user.Receiver == true))
                .Include(battle => battle.BattleUsers.Where(user => user.UserId != userId))
                .ThenInclude(userBattle => userBattle.User)
                .ToListAsync();
    }
}
