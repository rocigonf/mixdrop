using Microsoft.AspNetCore.Http.HttpResults;
using mixdrop_back.Models.Entities;
using mixdrop_back.Repositories.Base;
using System.Runtime.Intrinsics.X86;

namespace mixdrop_back.Services
{
    public class FriendshipService
    {
        private readonly UnitOfWork _unitOfWork;

        public FriendshipService(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // Método enviar solicitud de amistad
        public async Task AddFriend(User user1, User user2)
        {
            try
            {
                // Comprobamos si el user2 existe
                User existingUser = await _unitOfWork.UserRepository.GetByNicknameAsync(user2.Nickname);
                if (existingUser == null)
                {
                    throw new Exception("Este usuario no existe.");
                }

                Friendship existingFriendship = await _unitOfWork.FriendshipRepository.GetFriendshipAsync(user1.Id, user2.Id);
                if (existingFriendship != null)
                {
                    throw new Exception("Esta amistad ya existe");
                }

                // Se crea una nueva "plantilla" de amistad
                var newFriendship = await _unitOfWork.FriendshipRepository.InsertAsync(new Friendship());

                // A cada usuario se le asigna la misma ID de amistad
                UserFriend newUserFriend1 = new UserFriend
                {
                    FriendshipId = newFriendship.Id,
                    UserId = user1.Id,
                    Receiver = false
                };
                UserFriend newUserFriend2 = new UserFriend
                {
                    FriendshipId = newFriendship.Id,
                    UserId = user2.Id,
                    Receiver = true
                };

                await _unitOfWork.SaveAsync();
                
            }
            catch (Exception mortadela)
            {
                Console.WriteLine("Error al añadir amigo: ", mortadela);
            }
        }

        // Método solicitud de amistad
        public async Task AcceptFriend(int friendshipId, User user)
        {
            Friendship existingFriendship = await _unitOfWork.FriendshipRepository.GetAllFriendshipsAsync(friendshipId);
            if (existingFriendship == null)
            {
                throw new Exception("Esta solicitud no existe");
            }

            UserFriend receiverUser = existingFriendship.UserFriends.FirstOrDefault(user => user.Receiver == true);
            if (receiverUser.Id != user.Id)
            {
                throw new Exception("Este usuario no es recibidor");
            }

            existingFriendship.Accepted = true;

            _unitOfWork.FriendshipRepository.Update(existingFriendship);
            await _unitOfWork.SaveAsync();
            // You're my friend now :D turururururu
        }

        // Método borrar amigo o rechazar solicitud de amistad
        public async Task DeleteFriend (int friendshipId, User user)
        {
            // Comprobamos que la amistad existe
            Friendship existingFriendship = await _unitOfWork.FriendshipRepository.GetAllFriendshipsAsync(friendshipId);
            if (existingFriendship == null)
            {
                throw new Exception("Esta amistad no existe :(");
            }

            // Comprobamos que el usuario es parte de la amistad
            UserFriend userFriend = existingFriendship.UserFriends.FirstOrDefault(user => user.UserId == user.Id);
            if (userFriend == null)
            {
                throw new Exception("Este usuario no forma parte de esta amistad");
            }

            _unitOfWork.FriendshipRepository.Delete(existingFriendship);
            await _unitOfWork.SaveAsync();
        }
    }
}
