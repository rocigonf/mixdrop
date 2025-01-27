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

        public FriendshipService(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
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
                SenderUserId = user1.Id,
                ReceiverUserId = user2.Id,
            });

            await _unitOfWork.SaveAsync();

            JsonSerializerOptions options = new JsonSerializerOptions(JsonSerializerDefaults.Web);

            options.ReferenceHandler = ReferenceHandler.IgnoreCycles;

            dict["messageType"] = MessageType.AskForFriend;
            await WebSocketHandler.NotifyOneUser(JsonSerializer.Serialize(dict, options), user1.Id);
            await WebSocketHandler.NotifyOneUser(JsonSerializer.Serialize(dict, options), user2.Id);
        }

        // Método solicitud de amistad
        public async Task AcceptFriend(int friendshipId, int userId)
        {
            Friendship existingFriendship = await _unitOfWork.FriendshipRepository.GetByIdAsync(friendshipId);
            if (existingFriendship == null)
            {
                Console.WriteLine("Esta solicitud no existe");
                return;
            }

            if (userId != existingFriendship.ReceiverUserId)
            {
                Console.WriteLine("Este usuario no es recibidor");
                return;
            }

            existingFriendship.Accepted = true;

            _unitOfWork.FriendshipRepository.Update(existingFriendship);
            await _unitOfWork.SaveAsync();

            JsonSerializerOptions options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
            options.ReferenceHandler = ReferenceHandler.IgnoreCycles;

            dict["messageType"] = MessageType.AskForFriend;
            await WebSocketHandler.NotifyOneUser(JsonSerializer.Serialize(dict, options), existingFriendship.SenderUserId);
            await WebSocketHandler.NotifyOneUser(JsonSerializer.Serialize(dict, options), existingFriendship.ReceiverUserId);

            // You're my friend now :D turururururu
        }

        // Método borrar amigo o rechazar solicitud de amistad
        public async Task DeleteFriend(int friendshipId, int userId)
        {
            // Comprobamos que la amistad existe
            Friendship existingFriendship = await _unitOfWork.FriendshipRepository.GetByIdAsync(friendshipId);
            if (existingFriendship == null)
            {
                Console.WriteLine("Esta amistad no existe :(");
                return;
            }

            // Comprobamos que el usuario es parte de la amistad
            if (existingFriendship.SenderUserId != userId && existingFriendship.ReceiverUserId != userId)
            {
                Console.WriteLine("Este usuario no forma parte de esta amistad");
                return;
            }

            UserFriend user2 = existingFriendship.UserFriends.FirstOrDefault(userFriend => userFriend.UserId != user.Id);
            if (user2 == null)
            {
                throw new Exception("Este usuario no forma parte de esta amistad");
            }

            _unitOfWork.FriendshipRepository.Delete(existingFriendship);
            await _unitOfWork.SaveAsync();

            JsonSerializerOptions options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
            options.ReferenceHandler = ReferenceHandler.IgnoreCycles;

            dict["messageType"] = MessageType.AskForFriend;
            await WebSocketHandler.NotifyOneUser(JsonSerializer.Serialize(dict, options), existingFriendship.SenderUserId);
            await WebSocketHandler.NotifyOneUser(JsonSerializer.Serialize(dict, options), existingFriendship.ReceiverUserId);
        }

        public async Task<ICollection<Friendship>> GetFriendList(int userId)
        {
            ICollection<Friendship> friendships = await _unitOfWork.FriendshipRepository.GetFriendshipsByUserAsync(userId);

            foreach (Friendship friendship in friendships)
            {
                if (friendship.SenderUserId == userId)
                {
                    friendship.SenderUser = null;
                    friendship.ReceiverUser.Password = null;
                }
                else
                {
                    friendship.ReceiverUser = null;
                    friendship.SenderUser.Password = null;
                }
            }

            return friendships;
        }
    }
}
