using Microsoft.EntityFrameworkCore;
using mixdrop_back.Models.Entities;
using mixdrop_back.Repositories.Base;

namespace mixdrop_back.Repositories;

public class UserRepository : Repository<User, int>
{
    public UserRepository(MixDropContext context) : base(context) { }

    // TODO: Incluir las amistades

    public async Task<ICollection<User>> SearchUser(string search)
    {
        return await GetQueryable()
            .Where(user => user.Nickname.Contains(search))
            .ToListAsync();
    }

    public async Task<User> GetByEmailAsync(string email)
    {
        return await GetQueryable()
            .Include(user => user.Friendships)
            .Include(user => user.State)
            /*.Include(user => user.BattleUsers)
                .ThenInclude(userBattle => userBattle.Battle)
            .Include(user => user.BattleUsers)
               .ThenInclude(userBattle => userBattle.BattleResult)*/
            .FirstOrDefaultAsync(user => user.Email.Equals(email));
    }

    public async Task<User> GetByNicknameAsync(string nickname)
    {
        return await GetQueryable()
            .Include(user => user.Friendships)
            .Include(user => user.State)
            /*.Include(user => user.BattleUsers)
                .ThenInclude(userBattle => userBattle.Battle)
            .Include(user => user.BattleUsers)
               .ThenInclude(userBattle => userBattle.BattleResult)*/
            .FirstOrDefaultAsync(user => user.Nickname.Equals(nickname));
    }

    public async Task<User> GetByEmailOrNickname(string emailOrNickname)
    {
        return await GetQueryable()
            .Include(user => user.Friendships)
            .Include(user => user.State)
            /*.Include(user => user.BattleUsers)
                .ThenInclude(userBattle => userBattle.Battle)
            .Include(user => user.BattleUsers)
               .ThenInclude(userBattle => userBattle.BattleResult)*/
        .FirstOrDefaultAsync(user => user.Email == emailOrNickname || user.Nickname == emailOrNickname);
    }

    public async Task<User> GetUserById(int id)
    {
        return await GetQueryable()
            .Include(user => user.Friendships)
            .Include(user => user.State)
            /*.Include(user => user.BattleUsers)
                .ThenInclude(userBattle => userBattle.Battle)
            .Include(user => user.BattleUsers)
               .ThenInclude(userBattle => userBattle.BattleResult)*/
            .FirstOrDefaultAsync(user => user.Id == id);
    }

    public async Task<User> GetUserInQueueAsync()
    {
        return await GetQueryable()
            .FirstOrDefaultAsync(user => user.IsInQueue == true);
    }

    public async Task<List<User>> GetAllUsersAsync(int userId)
    {
        return await GetQueryable()
            .Where(u => u.Id != userId)
            .ToListAsync();
    }
}