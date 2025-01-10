using NAudio.Wave;
using mixdrop_back.SoundTouch;

namespace mixdrop_back;

// https://stackoverflow.com/questions/52795472/how-to-use-naudio-soundtouch-to-stream-mp3-in-asp-net-mvc-5
// https://www.markheath.net/post/varispeed-naudio-soundtouch (REALMENTE INTERESA ESTO: https://github.com/naudio/varispeed-sample)
// CONVERSIÓN DE BYTE A FLOAT: https://stackoverflow.com/questions/4635769/how-do-i-convert-an-array-of-floats-to-a-byte-and-back

public class HellIsForever
{
    public static void ChangeBPM(string inputFile, string outputFile, float newTempo)
    {
        AudioFileReader reader = new AudioFileReader(inputFile);

        bool useTempo = true;
        VarispeedSampleProvider speedControl = new VarispeedSampleProvider(reader, 100, new SoundTouchProfile(useTempo, false));

        // "newTempo" realmente no es el nuevo tempo, sino el "valor" (no es la palabra exacta creo) por el que quiero incrementar o decrementar el tempo (por ejemplo, un valor de 1.0 dejaría el tempo igual, mientras que 1.2 lo subiría un 20%)
        speedControl.PlaybackRate = newTempo;

        double duration = reader.TotalTime.TotalSeconds;

        // El SampleRate es aparentemente como la cantidad de fotogramas por segundo que hay en un vídeo, y cada uno de estos fotogramas (llamados muestras) representan la intensidad del audio?
        // Esto debe de ser un int (o cualquier tipo de entero realmente) porque es la única forma de meterlo en un array de floats (explicado mejor más adelante xd)
        int totalSamples = (int)(reader.WaveFormat.SampleRate * duration * reader.WaveFormat.Channels);

        // Para calcular la nueva duración (deja unos segundillos al final en el nuevo audio, pero no queda mal)
        int newSamples = (int)(totalSamples / newTempo);

        // Si en total hay 44100 (que parece ser el estándar) muestras, significa que tengo que crear un archivo con ese total (y el array de floats representa cada una de estas muestras)
        float[] buffer = new float[newSamples];

        // El pibe de la respuesta de StackOverflow yapea algo sobre esto, pero como el while que intenté antes no iba y así directamente sí, pues lo dejo así :3
        speedControl.Read(buffer, 0, buffer.Length);

        using (WaveFileWriter writer = new WaveFileWriter(outputFile, reader.WaveFormat))
        {
            byte[] byteBuffer = new byte[buffer.Length * 4];
            Buffer.BlockCopy(buffer, 0, byteBuffer, 0, byteBuffer.Length);
            writer.Write(byteBuffer, 0, byteBuffer.Length);
        }
    }
}
