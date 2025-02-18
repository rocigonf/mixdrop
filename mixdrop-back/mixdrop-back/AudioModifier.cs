using NAudio.Lame;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using SoundTouch.Net.NAudioSupport;

namespace mixdrop_back;

// https://stackoverflow.com/questions/52795472/how-to-use-naudio-soundtouch-to-stream-mp3-in-asp-net-mvc-5
// MIXEAR: https://github.com/naudio/NAudio/blob/master/Docs/MixTwoAudioFilesToWav.md

public class AudioModifier
{
    public byte[] Modify(string inputFile, float newTempo = 1.0f, float newPitch = 1.0f)
    {
        AudioFileReader reader = new AudioFileReader(inputFile);

        IWaveProvider pitchChanger = new SoundTouchWaveProvider(reader)
        {
            Pitch = newPitch,
            Tempo = newTempo
        };

        /*using MemoryStream memoryStream = new MemoryStream();
        reader.Position = 0;
        WaveFileWriter.WriteWavFileToStream(memoryStream, pitchChanger);

        reader.Position = 0;
        //WaveFileWriter.CreateWaveFile("aa.wav", pitchChanger);

        memoryStream.Position = 0;
        using WaveFileReader wavRdr = new WaveFileReader(memoryStream);

        using MemoryStream finalMemoryStream = new MemoryStream();

        using (var mp3Writer = new LameMP3FileWriter(finalMemoryStream, wavRdr.WaveFormat, 128))
        {
            memoryStream.CopyTo(mp3Writer);
        }*/

        using MemoryStream mp3Stream = new MemoryStream();
        using LameMP3FileWriter writer = new LameMP3FileWriter(mp3Stream, reader.WaveFormat, LAMEPreset.STANDARD);

        byte[] buffer = new byte[pitchChanger.WaveFormat.AverageBytesPerSecond];
        int count;
        reader.Position = 0;
        mp3Stream.Position = 0;

        while ((count = pitchChanger.Read(buffer, 0, buffer.Length)) > 0)
        {
            writer.Write(buffer, 0, count);
        }

        writer.Flush();

        return mp3Stream.ToArray();
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
