namespace mixdrop_back.Sockets.Game;

public class MusicNotes
{
    public static readonly Dictionary<string, int> NOTE_MAP = new Dictionary<string, int>
    {
        { "C", 0 }, { "C#", 1 }, { "D", 2 }, { "D#", 3 }, { "E", 4 },
        { "F", 5 }, { "F#", 6 }, { "G", 7 }, { "G#", 8 }, { "A", 9 },
        { "A#", 10 }, { "B", 11 }
    };

    public static readonly Dictionary<int, string> NOTE_MAP_REVERSE = new Dictionary<int, string>
    {
        { 0, "C" }, { 1, "C#" }, { 2, "D" }, { 3, "D#" }, { 4, "E" },
        { 5, "F" }, { 6, "F#" }, { 7, "G" }, { 8, "G#" }, { 9 , "A" },
        { 10 , "A#" }, { 11 , "B" }
    };

    public static readonly double SEMITONE = Math.Pow(2, 1.0 / 12);
    public static readonly double UP_ONE_TONE = SEMITONE * SEMITONE;
    public static readonly double DOWN_ONE_TONE = 1.0 / UP_ONE_TONE;
}
