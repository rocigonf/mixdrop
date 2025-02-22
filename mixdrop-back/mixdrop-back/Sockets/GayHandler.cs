using mixdrop_back.Models.DTOs;
using mixdrop_back.Models.Entities;
using mixdrop_back.Models.Mappers;
using mixdrop_back.Sockets.Game;
using System.Text.Json;
using System.Text.Json.Serialization;
using Action = mixdrop_back.Models.DTOs.Action;

namespace mixdrop_back.Sockets;
// SLAY QUEEN 💅✨
public class GayHandler // GameHandler :3
{
    private const int ACTIONS_REQUIRED = 1;
    private const int POINTS_REQUIRED = 10;

    public readonly ICollection<UserBattle> _participants = new List<UserBattle>();
    public Battle Battle { get; set; }

    // Lista obtenida de la base de datos
    private static ICollection<Card> _cards = new List<Card>();

    private readonly Board _board = new Board();
    private readonly AudioModifier _audioModifier = new AudioModifier();

    private int TotalActions { get; set; } = 0;
    private int TotalTurns { get; set; } = 0;

    private readonly UserBattleMapper _mapper = new UserBattleMapper();
    private readonly Random _random = new Random();
    private RemoveAFKPlayers _service;

    private string Bonus { get; set; } = "";


    /// <summary>
    /// Método que agrega participantes a la batalla
    /// </summary>
    /// <returns>Nada (por ahora)</returns>
    public async Task<UserBattleDto> AddParticipant(Battle battle, int userId, UnitOfWork unitOfWork, IServiceProvider serviceProvider) //💀💀💀💀
    {
        UserBattle player = battle.BattleUsers.FirstOrDefault(user => user.UserId == userId);
        if (_participants.Contains(player) || _participants.Count == 2)
        {
            return null;
        }

        if (_cards.Count == 0)
        {
            _cards = await unitOfWork.CardRepository.GetAllCardsAsync();
        }

        Random rand = new Random();

        // Creo el bot y le reparto las cartas
        if (battle.IsAgainstBot)
        {
            UserBattle bot = new UserBattle() { IsBot = true };
            DistributeCards(bot);
            _participants.Add(bot);
        }

        DistributeCards(player);

        if (_participants.Count + 1 == 2)
        {
            player.IsTheirTurn = true;
            player.TimePlayed = 120;  // 2 minutos para jugar (esta en proceso esto)
            player.ActionsLeft = ACTIONS_REQUIRED;

            UserBattle otherUser = _participants.FirstOrDefault(p => p.UserId != userId);

            if (!battle.IsAgainstBot)
            {
                _service = new RemoveAFKPlayers(player, otherUser, battle, this, serviceProvider);
                await _service.StartAsync(CancellationToken.None);
            }
        }

        _participants.Add(player);

        return _mapper.ToDto(player);
    }

    private void DistributeCards(UserBattle userBattle)
    {
        for (int i = 0; i < 5; i++)
        {
            Card card = _cards.ElementAt(_random.Next(0, _cards.Count));
            userBattle.Cards.Add(card);
        }
    }


    public async Task PlayCard(Action action, int userId, UnitOfWork unitOfWork, IServiceProvider serviceProvider)
    {
        UserBattle playerInTurn = _participants.FirstOrDefault(u => u.UserId == userId && u.IsTheirTurn);
        UserBattle otherUser = _participants.FirstOrDefault(u => u.UserId != userId);

        byte[] output = [];
        List<int> positions = new List<int>();
        Card randomCard = null;
        bool spinTheWheel = false;

        if (playerInTurn == null)
        {
            Console.WriteLine("No le toca a este jugador");
            return;
        }

        // Juega las cartas que quiera
        if (action.Card != null)
        {
            bool wasEmpty = false;

            CardToPlay card = action.Card;

            // Se comprueba que el jugador tuviese esta carta
            Card existingCard = playerInTurn.Cards.FirstOrDefault(c => c.Id == card.CardId);
            if (existingCard == null)
            {
                Console.WriteLine("La carta no existe");
                return;
            }

            Slot alreadyPlacedCard = _board.Slots.FirstOrDefault(s => s.Card?.Id == card.CardId);
            if(alreadyPlacedCard != null)
            {
                Console.WriteLine("Esa carta ya está en juego :(");
                return;
            }

            // Chequeo para ver si hay puntos extra
            Slot slut = _board.Slots.ElementAt(card.Position);
            if (slut.Card == null)
            {
                wasEmpty = true;
            }
            else
            {
                // Chequeo del nivel para que no se jueguen cartas inferiores
                if (slut.Card.Level > existingCard.Level)
                {
                    Console.WriteLine("El nivel de la carta jugada es inferior");
                    return;
                }
            }

            bool isCorrectType = true;
            string partName = existingCard.Track.Part.Name;

            // Chequeo que se pueda jugar una carta del tipo correcto para esa posición
            switch (card.Position)
            {
                case 0:
                    isCorrectType = CheckCardType(["Vocal", "Main"], partName);
                    break;
                case 1:
                    isCorrectType = CheckCardType(["Main"], partName);
                    break;
                case 2:
                    isCorrectType = CheckCardType(["Main", "Drums"], partName);
                    break;
                case 3:
                    isCorrectType = CheckCardType(["Drums"], partName);
                    break;
                case 4:
                    isCorrectType = CheckCardType(["Drums", "Bass"], partName);
                    break;
                default:
                    Console.WriteLine("La posición no es correcta");
                    return;
            }

            if (!isCorrectType)
            {
                Console.WriteLine("El tipo de la carta no es el correcto");
                return;
            }

            // Si todo está correcto, establezco la nueva carta y la borro del mazo
            slut.Card = existingCard;
            slut.UserId = playerInTurn.UserId;
            playerInTurn.Cards.Remove(existingCard);

            randomCard = _cards.ElementAt(_random.Next(0, _cards.Count));
            playerInTurn.Cards.Add(randomCard);

            positions.Add(card.Position);

            // Bonificaciones random
            switch (Bonus)
            {
                case "Amarillo":
                    playerInTurn.Punctuation += CheckForCardType(1, existingCard.CardType.Id);
                    break;
                case "Rojo":
                    playerInTurn.Punctuation += CheckForCardType(2, existingCard.CardType.Id);
                    break;
                case "Verde":
                    playerInTurn.Punctuation += CheckForCardType(3, existingCard.CardType.Id);
                    break;
                case "Azul":
                    playerInTurn.Punctuation += CheckForCardType(4, existingCard.CardType.Id);
                    break;
            }

            // Establezco la nueva mezcla
            output = PlayMusic(_board.Playing, existingCard);
            playerInTurn.Punctuation += 1;

            if (TotalTurns >= 1)
            {
                if (wasEmpty) { playerInTurn.Punctuation++; } // Puntos extra
            }

            //mix = PlayMusic(_board.Playing, existingCard);
            playerInTurn.ActionsLeft--;
        }
        else if (action.ActionType != null)
        {
            ActionType actionType = action.ActionType;
            switch (actionType.Name)
            {
                case "button":
                    positions = SpinTheWheel(otherUser);
                    spinTheWheel = true;
                    playerInTurn.ActionsLeft--;
                    break;
                default:
                    Console.WriteLine("La acción no existe");
                    break;
            }
        }

        // Si el total de acciones en la partida es par, significa que se ha completado un turno entero
        TotalActions++;
        if (TotalActions % 2 == 0)
        {
            TotalTurns++;

            // CUANDO TERMINO EL TURNO, GENERO UN BONUS ALEATORIO (25% de que ocurra, idk cuanto es en Dropmix realmente, y lo mismo habría que poner por nivel también)
            int random = _random.Next(0, 100);
            string[] colors = ["Amarillo", "Rojo", "Verde", "Azul"];
            if (random < 25)
            {
                Bonus = colors[_random.Next(0, colors.Length)];
            }
            else
            {
                Bonus = "";
            }
        }


        if (playerInTurn.ActionsLeft <= 0 && !otherUser.IsBot)
        {
            playerInTurn.IsTheirTurn = false;
            otherUser.IsTheirTurn = true;
        }

        Dictionary<object, object> dict = new Dictionary<object, object>
        {
            { "messageType", MessageType.TurnResult },
            { "board", _board },
            { "player", null },
            { "bonus", Bonus },
            { "position", positions },
            { "otherplayer", otherUser.Punctuation },
            { "wheel", spinTheWheel },
            //{ "card", randomCard }
        };

        await NotifyUsers(dict, playerInTurn, otherUser, output);

        dict["card"] = null;

        // Si aún puede seguir jugando
        if (playerInTurn.ActionsLeft > 0)
        {
            return;
        }

        // Doy por terminada la batalla
        if (playerInTurn.Punctuation >= POINTS_REQUIRED)
        {
            await EndBattle(playerInTurn, otherUser, unitOfWork);
            //GayNetwork._handlers.Remove(this);

            //dict["player"] = _mapper.ToDto(playerInTurn);
            dict["messageType"] = MessageType.EndGame;
            await NotifyUsers(dict, playerInTurn, otherUser, output, true);
        }
        else
        {
            if (otherUser.IsBot)
            {
                await DoBotActions(otherUser, playerInTurn, dict, unitOfWork);
                playerInTurn.ActionsLeft = ACTIONS_REQUIRED;
            }
            else
            {
                // Para el servicio en segundo plano
                if (_service != null)
                {
                    await _service.StopAsync(CancellationToken.None);
                    _service = new RemoveAFKPlayers(otherUser, playerInTurn, Battle, this, serviceProvider);
                    await _service.StartAsync(CancellationToken.None);
                }

                //otherUser.TimePlayed = 120;
                otherUser.ActionsLeft = ACTIONS_REQUIRED;
            }
        }
    }

    private UserBattleDto MapUserBattle(UserBattle userBattle)
    {
        UserBattleDto userBattleDto = _mapper.ToDto(userBattle);
        //userBattleDto.Cards = null;
        return userBattleDto;
    }

    public async Task EndBattle(UserBattle winner, UserBattle loser, UnitOfWork unitOfWork)
    {
        Battle.BattleStateId = 4;
        Battle.BattleUsers = [];
        Battle.FinishedAt = DateTime.UtcNow;

        var state = await unitOfWork.StateRepositoty.GetByIdAsync(2); // Conectado

        if (!winner.IsBot)
        {
            winner.BattleResultId = 1;
            winner.Cards = new List<Card>();

            winner.User.State = state;
            winner.User.StateId = state.Id;

            Battle.BattleUsers.Add(winner);
        }

        if (!loser.IsBot)
        {
            loser.BattleResultId = 2;
            loser.Cards = new List<Card>();

            loser.User.State = state;
            loser.User.StateId = state.Id;

            Battle.BattleUsers.Add(loser);
        }

        // Deberíamos hacer insert en lugar de update
        unitOfWork.BattleRepository.Update(Battle);

        await unitOfWork.SaveAsync();

        await WebSocketHandler.SendStatsMessage();
        GayNetwork._handlers.Remove(this);
    }

    private async Task NotifyUsers(Dictionary<object, object> dict, UserBattle playerInTurn, UserBattle otherUser, byte[] blob, bool end = false)
    {
        JsonSerializerOptions options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull; // Ignora nulos :>

        if (end)
        {
            dict.Add("otherUserId", otherUser.UserId);
        }

        // Notifico a los usuarios
        dict["player"] = MapUserBattle(playerInTurn);
        dict["otherplayer"] = otherUser.Punctuation;
        await WebSocketHandler.NotifyOneUser(JsonSerializer.Serialize(dict, options), playerInTurn.UserId);

        if (blob.Length > 0)
        {
            await WebSocketHandler.NotifyOneUserBlob(blob, playerInTurn.UserId);
        }

        if (otherUser.IsBot)
        {
            return;
        }

        if(end)
        {
            dict["otherUserId"] = playerInTurn.UserId;
        }

        dict["player"] = MapUserBattle(otherUser);
        dict["otherplayer"] = playerInTurn.Punctuation;
        dict["card"] = null;
        await WebSocketHandler.NotifyOneUser(JsonSerializer.Serialize(dict, options), otherUser.UserId);

        if (blob.Length > 0)
        {
            await WebSocketHandler.NotifyOneUserBlob(blob, otherUser.UserId);
        }
    }

    private static int CheckForCardType(int desiredType, int actualType)
    {
        return desiredType == actualType ? 1 : 0;
    }

    private async Task DoBotActions(UserBattle bot, UserBattle notBot, Dictionary<object, object> dict, UnitOfWork unitOfWork)
    {
        byte[] output = [];
        int totalActions = 0;
        bool spinTheWheel = false;
        bool end = false;

        while (totalActions < ACTIONS_REQUIRED)
        {
            bool couldPlay = false;

            for(int i = 0; i < _board.Slots.Length; i++)
            {
                Slot currentSlot = _board.Slots.ElementAt(i);
                Card card = GetValidCardForSlot(currentSlot, bot);
                if(card == null)
                {
                    continue;
                }

                bool canPlay = false;
                bool wasEmpty = false;

                if (currentSlot.Card == null)
                {
                    canPlay = true;
                    wasEmpty = true;
                }
                else
                {
                    // El Max está para comprobar si puede poner la carta ahí
                    if (currentSlot.Card.Level <= card.Level)
                    {
                        canPlay = true;
                    }
                }

                if (canPlay && card != null)
                {
                    currentSlot.Card = card;
                    currentSlot.UserId = 0;

                    bot.Cards.Remove(card);
                    bot.Cards.Add(_cards.ElementAt(_random.Next(0, _cards.Count)));
                    bot.Punctuation++;

                    output = PlayMusic(_board.Playing, card);
                    dict["position"] = new List<int>() { i };
                    dict["board"] = _board;
                    dict["otherplayer"] = bot.Punctuation;

                    if (TotalTurns >= 1)
                    {
                        if (wasEmpty) { bot.Punctuation++; }
                    }

                    couldPlay = true;

                    totalActions++;
                    if (totalActions == ACTIONS_REQUIRED)
                    {
                        break;
                    }
                }
            }

            // Si no pudo jugar ninguna carta, tira la ruleta
            if (!couldPlay)
            {
                dict["position"] = SpinTheWheel(notBot);
                spinTheWheel = true;
                totalActions++;
            }

            if (bot.Punctuation >= POINTS_REQUIRED)
            {
                dict["messageType"] = MessageType.EndGame;
                await EndBattle(bot, notBot, unitOfWork);
                GayNetwork._handlers.Remove(this);
                totalActions = ACTIONS_REQUIRED;
                end = true;
            }

            dict["wheel"] = spinTheWheel;
            await NotifyUsers(dict, notBot, bot, output, end);
        }

        TotalActions++;
        if (TotalActions % 2 == 0)
        {
            TotalTurns++;
        }
    }

    private List<int> SpinTheWheel(UserBattle opponent)
    {
        int result = _random.Next(0, 100);
        List<int> positions;

        if (result < 50)
        {
            positions = RemoveCardsOfLevel(3, opponent);
        }
        else if (result >= 50 && result < 75)
        {
            positions = RemoveCardsOfLevel(2, opponent);
        }
        else if(result >= 75 && result < 90)
        {
            positions = RemoveCardsOfLevel(1, opponent);
        }
        else
        {
            positions = RemoveCardsOfLevel(0, opponent);
        }

        return positions;
    }

    private List<int> RemoveCardsOfLevel(int level, UserBattle player)
    {
        List<int> positions = new List<int>();

        for (int i = 0; i < _board.Slots.Length; i++)
        {
            Slot slot = _board.Slots[i];
            if(slot.Card == null || slot.UserId != player.UserId)
            {
                continue;
            }

            if(level == 0)
            {
                slot.Card = null;
                positions.Add(i);
            }
            else if(slot.Card.Level == level)
            {
                slot.Card = null;
                positions.Add(i);
            }
        }

        return positions;
    }

    private Card GetValidCardForSlot(Slot slot, UserBattle bot)
    {
        // Obtengo los id de las cartas que ya están en juego
        List<int> occupiedCards = _board.Slots.Where(s => s.Card != null).Select(s => s.Card.Id).ToList();
        switch (Array.IndexOf(_board.Slots, slot))
        {
            case 0:
                return bot.Cards.FirstOrDefault(c => (c.Track.Part.Name.Equals("Vocal") || c.Track.Part.Name.Equals("Main")) && !occupiedCards.Contains(c.Id));
            case 1:
                return bot.Cards.FirstOrDefault(c => c.Track.Part.Name.Equals("Main") && !occupiedCards.Contains(c.Id));
            case 2:
                return bot.Cards.FirstOrDefault(c => (c.Track.Part.Name.Equals("Main") || c.Track.Part.Name.Equals("Drums")) && !occupiedCards.Contains(c.Id));
            case 3:
                return bot.Cards.FirstOrDefault(c => c.Track.Part.Name.Equals("Drums") && !occupiedCards.Contains(c.Id));
            case 4:
                return bot.Cards.FirstOrDefault(c => (c.Track.Part.Name.Equals("Drums") || c.Track.Part.Name.Equals("Bass")) && !occupiedCards.Contains(c.Id));
            default:
                return null;
        }
    }


    private static bool CheckCardType(List<string> possibleTypes, string actualType)
    {
        return possibleTypes.Contains(actualType);
    }

    private byte[] PlayMusic(Track playing, Card card)
    {
        if (playing == null)
        {
            _board.Playing = card.Track;
            return File.ReadAllBytes("wwwroot/" + card.Track.TrackPath);
        }
        else
        {
            // Cálculo de los nuevos BPM
            float currentBpm = playing.Song.Bpm;
            float cardBpm = card.Track.Song.Bpm;

            float changeForCard = (cardBpm - currentBpm) / cardBpm;
            float newBpmForCard = CalculateNewBpm(changeForCard);

            // Cálculo del nuevo pitch (CALCULAR COMO EN EL BPM PORQUE SI EN LA NOTA "Do" LA CANTIDAD SE SEMITONOS A DIVIDIR SERÍA 0)            
            int semitoneCurrent = GetFromDictionary(playing.Song.Pitch);
            int semitoneCard = GetFromDictionary(card.Track.Song.Pitch);

            // TODO: LA DIFERENCIA SE CALCULA SI LA DISTANCIA ARMÓNICA ES MAYOR QUE 1, IGUAL QUE TODO LO DEMÁS
            int difference = semitoneCard - semitoneCurrent;
            float pitchFactor = 1.0f;

            // Sólo aplico si es mayor que 1
            if (Math.Abs(difference) > 1)
            {
                pitchFactor = (float)Math.Pow(2, difference / 12.0);
            }

            /*if (pitchFactor < card.MinPitch)
            {
                difference += 12;
                pitchFactor = (float)Math.Pow(2, difference / 12.0);
            }
            else if(pitchFactor > card.MaxPitch)
            {
                difference -= 12;
                pitchFactor = (float)Math.Pow(2, difference / 12.0);
            }*/

            byte[] newAudio = _audioModifier.Modify("wwwroot/" + card.Track.TrackPath, newBpmForCard, pitchFactor);
            //byte[] message = [..BitConverter.GetBytes(card.Id), .. newAudio];
            byte[] message = [..newAudio];

            _board.Playing = new Track()
            {
                Song = new Song()
                {
                    Bpm = playing.Song.Bpm,
                    Pitch = playing.Song.Pitch,
                }
            };

            return message;

        }
    }

    private static float CalculateNewBpm(float difference)
    {
        if (difference > 0)
        {
            return 1 + difference;
        }
        else
        {
            return 1 - difference;
        }
    }

    private static int GetFromDictionary(string value)
    {
        int semitone = 0;

        bool couldGet = MusicNotes.NOTE_MAP.TryGetValue(value, out semitone);

        // Si no lo puede conseguir significa que está en la escala menor
        if (!couldGet)
        {
            semitone = MusicNotes.NOTE_MAP[MusicNotes.FIFTH_CIRCLE[value]];
        }
        return semitone;
    }
}
