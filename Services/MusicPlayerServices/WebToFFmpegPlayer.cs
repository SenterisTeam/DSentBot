using System.Diagnostics;
using Discord.Audio;
using DSentBot.Models;

namespace DSentBot.Services.MusicPlayerServices;

public class WebToFFmpegPlayer: IMusicPlayer
{
    private readonly FFmpegCollection _ffmpegCollection;
    private readonly ILogger<WebToFFmpegPlayer> _logger;

    public WebToFFmpegPlayer(FFmpegCollection ffmpegCollection, ILogger<WebToFFmpegPlayer> logger)
    {
        _ffmpegCollection = ffmpegCollection;
        _logger = logger;
    }

    public async Task Play(Music music, IAudioClient audioClient)
    {
        using (var client = new HttpClient())
        using (var stream = client.GetStreamAsync(music.Path))
        using (var ffmpeg = CreateStream(stream.Result))
        using (var output = ffmpeg.StandardOutput.BaseStream)
        using (var discord = audioClient.CreatePCMStream(AudioApplication.Mixed, bitrate: 131072, bufferMillis: 10, packetLoss: 0)) // Default bitrate is 96*1024
        {
            try { await output.CopyToAsync(discord); }
            finally { await discord.FlushAsync(); }
        }
    }

    private Process CreateStream(Stream stream)
    {
        _logger.LogInformation("FFmpeg getting..");
        var process = _ffmpegCollection.GetProcess();
        _logger.LogInformation("Stream started");

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