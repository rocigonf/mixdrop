using Microsoft.AspNetCore.Http.HttpResults;
using mixdrop_back.Models.Entities;
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

        // Enviar solicitud de amistad
        public async Task AddFriend(User user1, User user2)
        {
            try
            {
                // Comprobar si el user2 existe
                var existingUser = await _unitOfWork.UserRepository.GetByNicknameAsync(user2.Nickname);
                if (existingUser == null)
                {
                    throw new Exception("Este usuario no existe.");
                }

                var existingFriendship = await _unitOfWork.FriendshipRepository.GetFriendAsync(user1.Id, user2.Id);
                if (existingFriendship != null)
                {
                    throw new Exception("Esta amistad ya existe");
                }

                // Se crea una nueva "plantilla" de amistad
                var newFriendship = await _unitOfWork.FriendshipRepository.InsertAsync(new Friendship());
                
                // A cada usuario se le asigna la misma ID de amistad.
                var newUserFriend1 = new UserFriend
                {
                    FriendId = newFriendship.Id,
                    UserId = user1.Id,
                    Receiver = false
                };
                var newUserFriend2 = new UserFriend
                {
                    FriendId = newFriendship.Id,
                    UserId = user2.Id,
                    Receiver = true
                };

                await _unitOfWork.SaveAsync();
                
            }
            catch (Exception e)
            {
                Console.WriteLine("Error al añadir amigo: ", e);
            }
        }

        // Aceptar solicitud de amistad
        public async Task AcceptFriend(Friendship friendship, User user)
        {
            var existingFriendship = await _unitOfWork.FriendshipRepository.GetAllFriendsAsync(friendship.Id);
            if (existingFriendship == null)
            {
                throw new Exception("Esta solicitud no existe");
            }

            var receiverUser = existingFriendship.UserFriends.FirstOrDefault(user => user.Receiver);
            if (receiverUser.Id != user.Id)
            {
                throw new Exception("Este usuario no es recibidor");
            }

            existingFriendship.Accepted = true;

            _unitOfWork.FriendshipRepository.Update(existingFriendship);
            await _unitOfWork.SaveAsync();
            // You're my friend now :D turururururu
        }

        // Borrar amigo
        public async Task DeleteFriend (Friendship friendship, User user)
        {
            // Comprobar que la amistad existe y que el usuario es parte de la amistad
        }
    }
}
