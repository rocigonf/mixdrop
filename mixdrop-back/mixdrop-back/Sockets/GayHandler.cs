namespace mixdrop_back.Sockets;
// SLAY QUEEN 💅✨
public class GayHandler // GameHandler :3
{
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1,1);
    private readonly Dictionary<object, object> _participants = new Dictionary<object, object>(); // Guarda los usuarios y sus cartas
    private int _timmyTurnerId = 0; // Id del usuario que tiene el turno

    /// <summary>
    /// Método que maneja la lógica del juego
    /// </summary>
    /// <returns>Nada (por ahora)</returns>
    public async Task GameHandlerAsync()
    {
        await _semaphore.WaitAsync();

        // TODO: Lógica que maneja el juego 💀

        _semaphore.Release();
    }    
}
