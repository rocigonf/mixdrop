using Melanchall.DryWetMidi.MusicTheory;
using mixdrop_back.Models.Entities;

namespace mixdrop_back.Sockets.Game;

public class PitchCalculator
{
    private const int SEMITONES_IN_OCTAVE = 12;

    public int CalculateSemitones(Tone current, Song newSong, out bool takeAlternative)
    {
        // Obtenemos la escala de la tonalidad actual
        IEnumerable<Interval> currentIntervals;
        ScaleDegree relativeDegree;

        if (current.IsMajor)
        {
            currentIntervals = ScaleIntervals.Major;
            // El relativo sería menor y tenemos que coger el VI (submediante)
            relativeDegree = ScaleDegree.Submediant;
        }
        else
        {
            currentIntervals = ScaleIntervals.Minor;
            // El relativo sería mayor y tenemos que coger el III (mediante)
            relativeDegree = ScaleDegree.Mediant;
        }

        // Obtenemos grados I, IV, V y relativo
        Scale currentScale = new Scale(currentIntervals, current.Note);
        NoteName tonic = currentScale.RootNote; // I
        NoteName subdominant = currentScale.GetDegree(ScaleDegree.Subdominant); // IV
        NoteName dominant = currentScale.GetDegree(ScaleDegree.Dominant); // V
        NoteName relative = currentScale.GetDegree(relativeDegree); // Relativo

        int semitonesToPitch = 0;
        bool isCompatible = false;
        takeAlternative = false;
        NoteName[] currentScaleNotes = [tonic, subdominant, dominant];

        // Vemos si es compatible la tonalidad preferida
        if (CheckCompatibility(current, currentScaleNotes, relative, newSong.Preferred))
        {
            isCompatible = true;
        }
        // Si no, vemos si es compatible la tonalidad alternativa
        /*else if (CheckCompatibility(current, currentScaleNotes, relative, newSong.Alternative))
        {
            isCompatible = true;
            takeAlternative = true;
        }
        // Si no, tenemos que modificar, pero tenemos que saber cúal tonalidad coger de la nueva canción
        // Cogeremos la alternativa en el caso de que el modo sea igual a la tonalidad actual
        // En caso contrario cogeremos la preferida
        else
        {
            takeAlternative = current.IsMajor == newSong.Alternative.IsMajor;
        }*/

        // Si no es compatible tenemos que saber cuántos semitonos transportarlo
        if (!isCompatible)
        {
            //Tone newTone = takeAlternative ? newSong.Alternative : newSong.Preferred;
            Tone newTone = newSong.Preferred;
            semitonesToPitch = GetMinDistance(newTone.Note, currentScaleNotes);
        }

        return semitonesToPitch;
    }

    private static bool CheckCompatibility(Tone currentTone, NoteName[] currentScaleNotes, NoteName currentRelative, Tone newTone)
    {
        bool isCompatible = false;

        // Si ambos tienen la tonalidad en el mismo modo
        if (currentTone.IsMajor == newTone.IsMajor)
        {
            // Si ambas están en el mismo modo será compatible si la nueva tonalidad
            // coincide con los grados I, IV y V de la tonalidad actual
            isCompatible = currentScaleNotes.Contains(newTone.Note);
        }
        else
        {
            // Si una está en mayor y otra en menor será compatible si la nueva tonalidad
            // es la relativa de la tonalidad actual
            isCompatible = currentRelative == newTone.Note;
        }

        return isCompatible;
    }

    private static int GetMinDistance(NoteName note, NoteName[] scaleNotes)
    {
        // Aquí se guarda la distancia real, será negativo si baja y positivo si sube
        int minRealDistance = int.MaxValue;
        // Aquí se guarda la distancia en valor absoluto, estamos buscando el mínimo
        int minAbsoluteDistance = minRealDistance;

        foreach (NoteName scaleNote in scaleNotes)
        {
            // Tenemos que tener en cuenta las dos posibles direcciones,
            // ir a la nota objetivo de forma directa o dando la vuelta
            int directRealDistance = scaleNote - note;
            int directAbsoluteDistance = Math.Abs(directRealDistance);
            int reverseRealDistance = SEMITONES_IN_OCTAVE - scaleNote - note;
            int reverseAbsoluteDistance = Math.Abs(reverseRealDistance);

            // Si de forma directa es más corto lo guardamos
            if (directAbsoluteDistance < minAbsoluteDistance)
            {
                minRealDistance = directRealDistance;
                minAbsoluteDistance = directAbsoluteDistance;
            }

            // Si de forma inversa es más corto lo guardamos
            if (reverseAbsoluteDistance < minAbsoluteDistance)
            {
                minRealDistance = reverseRealDistance;
                minAbsoluteDistance = reverseAbsoluteDistance;
            }
        }

        return minRealDistance;
    }
}
