using mixdrop_back.Models.Entities;
using mixdrop_back.Sockets;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace mixdrop_back.Services
{
    public class FriendshipService
    {
        private readonly UnitOfWork _unitOfWork;
        private Dictionary<object, object> dict = new Dictionary<object, object>
        {
            { "messageType", MessageType.Friend },
            { "friends", null }
        };
        private static readonly JsonSerializerOptions OPTIONS = new JsonSerializerOptions();


        public FriendshipService(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            OPTIONS.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        }

        // Método enviar solicitud de amistad
        public async Task AddFriend(User user1, User user2)
        {
            // Comprobamos si el user2 existe
            User existingUser = await _unitOfWork.UserRepository.GetByNicknameAsync(user2.Nickname);
            if (existingUser == null)
            {
                Console.WriteLine("Este usuario no existe.");
                return;
            }

            Friendship existingFriendship = await _unitOfWork.FriendshipRepository.GetFriendshipAsync(user1.Id, user2.Id);
            if (existingFriendship != null)
            {
                Console.WriteLine("Esta amistad ya existe");
                return;
            }

            // Se crea una nueva "plantilla" de amistad
            var newFriendship = await _unitOfWork.FriendshipRepository.InsertAsync(new Friendship() 
            {
                User1Id = user1.Id,
                User2Id = user2.Id,
                ReceiverId = user2.Id
            });


            // A cada usuario se le asigna la misma ID de amistad
            /*UserFriend newUserFriend1 = new UserFriend
            {
                FriendshipId = newFriendship.Id,
                UserId = user1.Id,
                Receiver = false,
                Friendships = newFriendship
            };
            UserFriend newUserFriend2 = new UserFriend
            {
                FriendshipId = newFriendship.Id,
                UserId = user2.Id,
                Receiver = true,
                Friendships = newFriendship
            };

            // Se insertan los nuevos amigos
            await _unitOfWork.UserFriendRepository.InsertAsync(newUserFriend1);
            await _unitOfWork.UserFriendRepository.InsertAsync(newUserFriend2);*/

            await _unitOfWork.SaveAsync();

            dict["friends"] = user1.Friendships.Append(newFriendship);

            await WebSocketHandler.NotifyOneUser(JsonSerializer.Serialize(dict, OPTIONS), user1.Id);

            dict["friends"] = user2.Friendships.Append(newFriendship);
            await WebSocketHandler.NotifyOneUser(JsonSerializer.Serialize(dict, OPTIONS), user2.Id);
        }

        // Método solicitud de amistad
        public async Task AcceptFriend(int friendshipId, User user)
        {
            Friendship existingFriendship = await _unitOfWork.FriendshipRepository.GetAllFriendshipsAsync(friendshipId);
            if (existingFriendship == null)
            {
                Console.WriteLine("Esta solicitud no existe");
                return;
            }

            //UserFriend receiverUser = existingFriendship.UserFriends.FirstOrDefault(user => user.Receiver == true);
            if (user.Id != existingFriendship.ReceiverId)
            {
                Console.WriteLine("Este usuario no es recibidor");
                return;
            }

            existingFriendship.Accepted = true;

            _unitOfWork.FriendshipRepository.Update(existingFriendship);
            await _unitOfWork.SaveAsync();

            // Notificar a usuario recibidor
            user.Friendships.Remove(existingFriendship); // Se borra la amistad del usuario
            //receiverUser.Friendships.Accepted = true; // Se cambia el estado de la amistad

            dict["friends"] = user.Friendships.Append(existingFriendship); // Y se vuelve a pegar la amistad
            await WebSocketHandler.NotifyOneUser(JsonSerializer.Serialize(dict), user.Id);

            // Notificar a usuario enviador
            //UserFriend senderUser = existingFriendship.UserFriends.FirstOrDefault(user => user.Receiver == false);
            User sender = existingFriendship.User1;

            sender.Friendships.Remove(existingFriendship); // Se borra la amistad del usuario
            //sender.Friendships.Accepted = true; // Se cambia el estado de la amistad

            dict["friends"] = sender.Friendships.Append(existingFriendship); // Y se vuelve a pegar la amistad
            await WebSocketHandler.NotifyOneUser(JsonSerializer.Serialize(dict), sender.Id);

            // You're my friend now :D turururururu
        }

        // Método borrar amigo o rechazar solicitud de amistad
        public async Task DeleteFriend(int friendshipId, User user)
        {
            // Comprobamos que la amistad existe
            Friendship existingFriendship = await _unitOfWork.FriendshipRepository.GetAllFriendshipsAsync(friendshipId);
            if (existingFriendship == null)
            {
                Console.WriteLine("Esta amistad no existe :(");
                return;
            }

            // Comprobamos que el usuario es parte de la amistad
            //User user = existingFriendship.FirstOrDefault(user => user.UserId == user.Id);
            if (existingFriendship.User1Id != user.Id && existingFriendship.User2Id != user.Id)
            {
                Console.WriteLine("Este usuario no forma parte de esta amistad");
                return;
            }

            _unitOfWork.FriendshipRepository.Delete(existingFriendship);
            await _unitOfWork.SaveAsync();
        }

        // TODO: Agregar verificación
        public async Task<ICollection<Friendship>> GetFriendList(int userId)
        {
            /*User user = await _unitOfWork.UserRepository.GetUserWithFriends(userId);
            if (user == null)
            {
                Console.WriteLine("Si es nulo vete a tomar por culo >:(");
                return null;
            }*/

            ICollection<Friendship> friendships = await _unitOfWork.FriendshipRepository.GetFriendshipyByUserIdAsync(userId);
            return friendships;
        }
    }
}
