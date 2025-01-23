using Microsoft.AspNetCore.Http.HttpResults;
using mixdrop_back.Models.Entities;
using mixdrop_back.Repositories.Base;
using mixdrop_back.Sockets;
using System.Runtime.Intrinsics.X86;
using System.Text.Json;

namespace mixdrop_back.Services
{
    public class FriendshipService
    {
        private readonly UnitOfWork _unitOfWork;
        private Dictionary<object, object> dict = new Dictionary<object, object>
                    {
                        { "messageType", MessageType.Friend }
                    };
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
                
                // Se insertan los nuevos amigos
                await _unitOfWork.UserFriendRepository.InsertAsync(newUserFriend1);
                await _unitOfWork.UserFriendRepository.InsertAsync(newUserFriend2);

                await _unitOfWork.SaveAsync();

                dict.Add("friends", user1.UserFriends.Append(newUserFriend1));
                await WebSocketHandler.NotifyOneUser(JsonSerializer.Serialize(dict), user1.Id);

                dict["friends"] = user2.UserFriends.Append(newUserFriend2);
                await WebSocketHandler.NotifyOneUser(JsonSerializer.Serialize(dict), user2.Id);
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

            // Notificar a usuario recibidor
            user.UserFriends.Remove(receiverUser); // Se borra la amistad del usuario
            receiverUser.Friendships.Accepted = true; // Se cambia el estado de la amistad

            dict.Add("friends", user.UserFriends.Append(receiverUser)); // Y se vuelve a pegar la amistad
            await WebSocketHandler.NotifyOneUser(JsonSerializer.Serialize(dict), user.Id);

            // Notificar a usuario enviador
            UserFriend senderUser = existingFriendship.UserFriends.FirstOrDefault(user => user.Receiver == false);
            User sender = await _unitOfWork.UserRepository.GetUserById(senderUser.Id);

            sender.UserFriends.Remove(senderUser); // Se borra la amistad del usuario
            senderUser.Friendships.Accepted = true; // Se cambia el estado de la amistad

            dict["friends"] = sender.UserFriends.Append(senderUser); // Y se vuelve a pegar la amistad
            await WebSocketHandler.NotifyOneUser(JsonSerializer.Serialize(dict), sender.Id);

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
            UserFriend userFriend = existingFriendship.UserFriends.FirstOrDefault(userFriend => userFriend.UserId == user.Id);
            if (userFriend == null)
            {
                throw new Exception("Este usuario no forma parte de esta amistad");
            }

            UserFriend user2 = existingFriendship.UserFriends.FirstOrDefault(userFriend => userFriend.UserId != user.Id);
            if (user2 == null)
            {
                throw new Exception("Este usuario no forma parte de esta amistad");
            }

            _unitOfWork.FriendshipRepository.Delete(existingFriendship);
            await _unitOfWork.SaveAsync();

            // Notificar al primer usuario
            dict.Add("friends", user.UserFriends);
            await WebSocketHandler.NotifyOneUser(JsonSerializer.Serialize(dict), user.Id);

            // Notificar al segundo usuario
            dict.Add("friends", user2.Friendships.UserFriends);
            await WebSocketHandler.NotifyOneUser(JsonSerializer.Serialize(dict), user2.UserId);
        }

        public async Task<ICollection<UserFriend>> GetFriendList(int userId)
        {
            User user = await _unitOfWork.UserRepository.GetUserById(userId);
            if (user == null) {
                throw new Exception("Si es nulo vete a tomar por culo >:(");
            }

            return user.UserFriends;
        }
    }
}
