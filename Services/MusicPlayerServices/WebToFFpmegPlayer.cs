using System.Diagnostics;
using Discord.Audio;
using DSentBot.Models;

namespace DSentBot.Services.MusicPlayerServices;

public class WebToFFpmegPlayer: IMusicPlayer
{
    public async Task Play(Music music, IAudioClient audioClient)
    {
        using (var client = new HttpClient())
        using (var stream = client.GetStreamAsync(music.Path))
        using (var ffmpeg = CreateStream(stream.Result))
        using (var output = ffmpeg.StandardOutput.BaseStream)
        using (var discord = audioClient.CreatePCMStream(AudioApplication.Mixed, bufferMillis: 3000, packetLoss: 0))
        {
            try { await output.CopyToAsync(discord); }
            finally { await discord.FlushAsync(); }
        }
    }

    private Process CreateStream(Stream stream)
    {
        var process = Process.Start(new ProcessStartInfo
        {
            FileName = "ffmpeg",
            Arguments = $"-hide_banner -loglevel debug -i - -ac 2 -f s16le -ar 48000 pipe:1", // #TODO change bitrate to 128k/256k
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardInput = true
        });

        stream.CopyToAsync(process.StandardInput.BaseStream);

        return process;
    }

    private MemoryStream CreateMemoryStream(Stream input)
    {
        var stream = new MemoryStream();
        input.CopyToAsync(stream);
        return stream;
    }
}