using mixdrop_back.Models.Entities;
using System.Net.WebSockets;

namespace mixdrop_back.Models.Sockets;

public class UserSocket
{
    public WebSocket WebSocket;
    public User User { get; set; }
}
