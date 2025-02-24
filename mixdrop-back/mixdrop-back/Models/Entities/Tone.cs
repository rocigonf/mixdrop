using Melanchall.DryWetMidi.MusicTheory;

namespace mixdrop_back.Models.Entities;

public class Tone
{
    public int Id { get; set; }
    public NoteName Note { get; set; }
    public bool IsMajor { get; set;  }

    public Tone() { }

    public Tone(string note, bool isMajor)
    {
        Note noteParsed = Melanchall.DryWetMidi.MusicTheory.Note.Parse(note + 4);
        Note = noteParsed.NoteName;
        IsMajor = isMajor;
    }
}