using NAudio.Wave;
using mixdrop_back.SoundTouch;

namespace mixdrop_back;

// https://stackoverflow.com/questions/52795472/how-to-use-naudio-soundtouch-to-stream-mp3-in-asp-net-mvc-5
// https://www.markheath.net/post/varispeed-naudio-soundtouch (REALMENTE INTERESA ESTO: https://github.com/naudio/varispeed-sample)

public class HellIsForever
{
    public static void ChangeBPM(string inputFile, string outputFile, int newTempo)
    {
        AudioFileReader reader = new AudioFileReader(inputFile);

        bool useTempo = true;

        // "newTempo" realmente no es el nuevo tempo, sino el "valor" (no es la palabra exacta creo) por el que quiero incrementar o decrementar el tempo (por ejemplo, un valor de 100 dejaría el tempo igual, mientras que 120 lo subiría un 20%)
        VarispeedSampleProvider speedControl = new VarispeedSampleProvider(reader, newTempo, new SoundTouchProfile(useTempo, false));

        using (WaveFileWriter waveFileWriter = new WaveFileWriter(outputFile, reader.WaveFormat))
        {
            // El SampleRate es aparentemente como la cantidad de fotogramas por segundo que hay en un vídeo, y cada uno de estos fotogramas (llamados muestras) representan la intensidad del audio?
            // Si en total hay 44100 (que parece ser el estándar) muestras, significa que tengo que crear un archivo con ese total (y el array de floats representa cada una de estas muestras)
            float[] buffer = new float[reader.WaveFormat.SampleRate * reader.WaveFormat.Channels];

            int read;
            
            // A lo que se refería el pibe de StackOverflow en la respuesta es que "WriteSamples" escribe por bloques, y me interesa saber si ya he terminado de escribirlos todos (si la lectura es 0, rompo el bucle porque significa que ya he terminado)
            while ((read = speedControl.Read(buffer, 0, buffer.Length)) > 0)
            {
                waveFileWriter.WriteSamples(buffer, 0, read);
            }

            waveFileWriter.Flush();
        }
    }
}
