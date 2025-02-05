namespace mixdrop_back.Sockets;

public enum MessageType
{
    Friend,                  // lista de amigos
    Stats,                   // jugadores conectados y desconectados
    Play,                    // que la batalla va a comenzar
    AskForFriend,            // cuando ha habido un cambio en la lista de amigos, le pide al front que se la pida 
    AskForBattle,            // cuando ha habido un cambio en la lista de batallas,  " " "
    PendingBattle,           // batallas pendientes    
    DisconnectedFromBattle,   // que alguien se ha desconectado de una batalla
    StartBattle,
    ShuffleDeckStart,
    PlayCard // Cuando se juega una carta
}
