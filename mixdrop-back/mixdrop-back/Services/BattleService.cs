using mixdrop_back.Models.Entities;

namespace mixdrop_back.Services;

public class BattleService
{
    private readonly UnitOfWork _unitOfWork;

    public BattleService(UnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task CreateBattle(User user1, User user2 = null, bool isRandom = false)
    {
        User existingUser = await _unitOfWork.UserRepository.GetByNicknameAsync(user2.Nickname);
        if (existingUser == null)
        {
            Console.WriteLine("Este usuario no existe.");
            return;
        }

        Battle existingBattle = await _unitOfWork.BattleRepository.GetBattleByUsersAsync(user1.Id, user2.Id);
        if (existingBattle != null)
        {
            Console.WriteLine("Esta batalla ya existe");
            return;
        }

        Battle newBattle = await _unitOfWork.BattleRepository.InsertAsync(isRandom ? new Battle() { Accepted = true } : new Battle());

        UserBattle newUserBattle1 = new UserBattle
        {
            BattleId = newBattle.Id,
            UserId = user1.Id,
            Receiver = false
        };
        UserBattle newUserBattle2 = new UserBattle
        {
            BattleId = newBattle.Id,
            UserId = user2.Id,
            Receiver = true,
        };

        await _unitOfWork.UserBattleRepository.InsertAsync(newUserBattle1);
        await _unitOfWork.UserBattleRepository.InsertAsync(newUserBattle2);
        await _unitOfWork.SaveAsync();
    }

    // Método solicitud de batalla
    public async Task AcceptBattle(int battleId, int userId)
    {
        Battle existingBattle = await _unitOfWork.BattleRepository.GetCompleteBattleAsync(battleId);
        if (existingBattle == null)
        {
            Console.WriteLine("Esta solicitud no existe");
            return;
        }

        UserBattle receiverUser = existingBattle.BattleUsers.FirstOrDefault(user => user.Receiver == true);
        if (receiverUser.Id != userId)
        {
            Console.WriteLine("Este usuario no es recibidor");
            return;
        }

        existingBattle.Accepted = true;

        _unitOfWork.BattleRepository.Update(existingBattle);
        await _unitOfWork.SaveAsync();
    }

    // Método borrar amigo o rechazar solicitud de batalla
    public async Task DeleteBattle(int battleId, int userId)
    {
        // Comprobamos que la batalla existe
        Battle existingBattle = await _unitOfWork.BattleRepository.GetCompleteBattleAsync(battleId);
        if (existingBattle == null)
        {
            Console.WriteLine("Esta batalla no existe :(");
            return;
        }

        // Comprobamos que el usuario es parte de la batalla
        UserBattle userBattle = existingBattle.BattleUsers.FirstOrDefault(user => user.UserId == userId);
        if (userBattle == null)
        {
            Console.WriteLine("Este usuario no forma parte de esta batalla");
            return;
        }

        _unitOfWork.BattleRepository.Delete(existingBattle);
        await _unitOfWork.SaveAsync();
    }

    // Emparejamiento aleatorio
    public async Task RandomBattle(User user)
    {
        User userInQueue = await _unitOfWork.UserRepository.GetUserInQueueAsync();
        if (userInQueue == null)
        {
            user.IsInQueue = true;
            _unitOfWork.UserRepository.Update(user);
            await _unitOfWork.SaveAsync();
        }
        else
        {
            userInQueue.IsInQueue = false;
            _unitOfWork.UserRepository.Update(userInQueue);
            await CreateBattle(user, userInQueue, true);
        }
    }

    public async Task<ICollection<Battle>> GetBattleList(int userId)
    {
        ICollection<Battle> battles = await _unitOfWork.BattleRepository.GetBattleByUserIdAsync(userId);
        return battles;
    }
}
