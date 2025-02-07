using NAudio.Wave;
using mixdrop_back.SoundTouch;
using NAudio.Wave.SampleProviders;

namespace mixdrop_back;

// https://stackoverflow.com/questions/52795472/how-to-use-naudio-soundtouch-to-stream-mp3-in-asp-net-mvc-5
// https://www.markheath.net/post/varispeed-naudio-soundtouch (REALMENTE INTERESA ESTO: https://github.com/naudio/varispeed-sample)
// CONVERSIÓN DE BYTE A FLOAT: https://stackoverflow.com/questions/4635769/how-do-i-convert-an-array-of-floats-to-a-byte-and-back
// PARA LA TONALIDAD: https://github.com/naudio/NAudio/blob/master/Docs/SmbPitchShiftingSampleProvider.md
// MIXEAR: https://github.com/naudio/NAudio/blob/master/Docs/MixTwoAudioFilesToWav.md

public class HellIsForever
{
    // Primero cambia la tonalidad y luego cambia el tono (con valores por defecto por si solo se quiere cambiar una cosa)
    public static void ChangeBPM(string inputFile, string outputFile, float newTempo = 1.0f, float newPitch = 1.0f)
    {
        AudioFileReader reader = new AudioFileReader(inputFile);

        // Tanto "SmbPitchShiftingSampleProvider" como "VarispeedSampleProvider" implementan "ISampleProvider", por lo que se puede utilizar indistintamente
        ISampleProvider sampleProvider = reader.ToSampleProvider();

        // A PARTIR DE AQUÍ, PARA CAMBIAR EL PITCH / TONALIDAD
        SmbPitchShiftingSampleProvider pitchChanger = new SmbPitchShiftingSampleProvider(sampleProvider);
        pitchChanger.PitchFactor = newPitch;
        sampleProvider = pitchChanger;

        // A PARTIR DE AQUÍ, PARA CAMBIAR EL TEMPO
        bool useTempo = true;

        // Se le pasa "sampleProvider" para mantener los cambios
        VarispeedSampleProvider speedControl = new VarispeedSampleProvider(sampleProvider, 100, new SoundTouchProfile(useTempo, false));

        // "newTempo" realmente no es el nuevo tempo, sino el "valor" (no es la palabra exacta creo) por el que quiero incrementar o decrementar el tempo (por ejemplo, un valor de 1.0 dejaría el tempo igual, mientras que 1.2 lo subiría un 20%)
        speedControl.PlaybackRate = newTempo;
        sampleProvider = speedControl; // "sampleProvider" en este punto tiene todos los cambios y es lo que se va a guardar

        // A PARTIR DE AQUÍ, PARA GUARDAR EL ARCHIVO
        double duration = reader.TotalTime.TotalSeconds;

        // El SampleRate es aparentemente como la cantidad de fotogramas por segundo que hay en un vídeo, y cada uno de estos fotogramas (llamados muestras) representan la intensidad del audio?
        // Esto debe de ser un int (o cualquier tipo de entero realmente) porque es la única forma de meterlo en un array de floats (explicado mejor más adelante xd)
        int totalSamples = (int)(reader.WaveFormat.SampleRate * duration * reader.WaveFormat.Channels);

        // Para calcular la nueva duración (deja unos segundillos al final en el nuevo audio, pero no queda mal)
        int newSamples = (int)(totalSamples / newTempo);

        // Si en total hay 44100 (que parece ser el estándar) muestras, significa que tengo que crear un archivo con ese total (y el array de floats representa cada una de estas muestras)
        float[] buffer = new float[newSamples];

        // El pibe de la respuesta de StackOverflow yapea algo sobre esto, pero como el while que intenté antes no iba y así directamente sí, pues lo dejo así :3
        sampleProvider.Read(buffer, 0, buffer.Length);

        using WaveFileWriter writer = new WaveFileWriter(outputFile, reader.WaveFormat);

        byte[] byteBuffer = new byte[buffer.Length * 4];
        Buffer.BlockCopy(buffer, 0, byteBuffer, 0, byteBuffer.Length);
        writer.Write(byteBuffer, 0, byteBuffer.Length);
    }

    public static void MixFiles(string file1, string file2, string outputFile)
    {
        using var reader1 = new AudioFileReader(file1);
        using var reader2 = new AudioFileReader(file2);

        /* SI UN AUDIO ES MÁS ALTO QUE OTRO
        reader1.Volume = 0.75f;
        reader2.Volume = 0.75f;*/

        var mixer = new MixingSampleProvider(new[] { reader1, reader2 });
        WaveFileWriter.CreateWaveFile16(outputFile, mixer);
        //WaveFileWriter.CreateWaveFile16(outputFile.Replace("wwwroot/", ""), mixer);
    }
}
