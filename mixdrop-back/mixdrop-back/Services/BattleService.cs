using mixdrop_back.Models.Entities;

namespace mixdrop_back.Services;

public class BattleService
{
    private readonly UnitOfWork _unitOfWork;

    public BattleService(UnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork; 
    }

    public async Task CreateBattle(User user1, User user2)
    {
        try
        {
            User existingUser = await _unitOfWork.UserRepository.GetByNicknameAsync(user2.Nickname);
            if (existingUser == null)
            {
                throw new Exception("Este usuario no existe.");
            }

            Battle existingBattle = await _unitOfWork.BattleRepository.GetBattleByUsersAsync(user1.Id, user2.Id);
            if (existingBattle != null)
            {
                throw new Exception("Esta batalla ya existe");
            }

            Battle newBattle = await _unitOfWork.BattleRepository.InsertAsync(new Battle());

            UserBattle newUserBattle1 = new UserBattle
            {
               BattleId = newBattle.Id,
               UserId = user1.Id,
               Receiver = false,
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
        catch (Exception mortadela)
        {
            Console.WriteLine("Error al crear la batalla: ", mortadela);
        }
    }

    // Método solicitud de batalla
    public async Task AcceptBattle(int battleId, User user)
    {
        Battle existingBattle = await _unitOfWork.BattleRepository.GetCompleteBattleAsync(battleId);
        if (existingBattle == null)
        {
            throw new Exception("Esta solicitud no existe");
        }

        UserBattle receiverUser = existingBattle.BattleUsers.FirstOrDefault(user => user.Receiver == true);
        if (receiverUser.Id != user.Id)
        {
            throw new Exception("Este usuario no es recibidor");
        }

        existingBattle.Accepted = true;

        _unitOfWork.BattleRepository.Update(existingBattle);
        await _unitOfWork.SaveAsync();
    }

    // Método borrar amigo o rechazar solicitud de batalla
    public async Task DeleteBattle(int battleId, User user)
    {
        // Comprobamos que la batalla existe
        Battle existingBattle = await _unitOfWork.BattleRepository.GetCompleteBattleAsync(battleId);
        if (existingBattle == null)
        {
            throw new Exception("Esta batalla no existe :(");
        }

        // Comprobamos que el usuario es parte de la batalla
        UserBattle userBattle = existingBattle.BattleUsers.FirstOrDefault(user => user.UserId == user.Id);
        if (userBattle == null)
        {
            throw new Exception("Este usuario no forma parte de esta batalla");
        }

        _unitOfWork.BattleRepository.Delete(existingBattle);
        await _unitOfWork.SaveAsync();
    }
}
