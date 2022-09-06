using System.Diagnostics;
using Discord.Audio;
using DSentBot.Models;

namespace DSentBot.Services.MusicPlayerServices;

public class WebToFFmpegPlayer: MusicServiceBase
{
    private IAudioClient _audioClient;
    private readonly FFmpegCollection _ffmpegCollection;
    private readonly MusicPlayerCollection _musicPlayerCollection;
    private readonly ILogger<WebToFFmpegPlayer> _logger;

    protected Queue<Music> _musicQueue { get; set; }

    public WebToFFmpegPlayer(FFmpegCollection ffmpegCollection, MusicPlayerCollection musicPlayerCollection, ILogger<WebToFFmpegPlayer> logger)
    {
        _ffmpegCollection = ffmpegCollection;
        _musicPlayerCollection = musicPlayerCollection;
        _logger = logger;

        _musicQueue = new Queue<Music>();
    }

    protected override async Task Player(CancellationToken cancellationToken)
    {
        while (_musicQueue.Count != 0 && !cancellationToken.IsCancellationRequested)
        {
            Music music = _musicQueue.Dequeue();

            using (var client = new HttpClient())
            using (var stream = client.GetStreamAsync(music.Path))
            using (var ffmpeg = CreateStream(stream.Result))
            using (var output = ffmpeg.StandardOutput.BaseStream)
            using (var discord = _audioClient.CreatePCMStream(AudioApplication.Mixed, bitrate: 131072, bufferMillis: 10, packetLoss: 0)) // Default bitrate is 96*1024
            {
                try { await output.CopyToAsync(discord); }
                finally { await discord.FlushAsync(); }
            }

            // TODO it doesn't finish
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

    public override Task AddToQueue(Music music)
    {
        _musicQueue.Enqueue(music);

        return Task.CompletedTask;
    }

    public override Task SkipMusic()
    {
        _logger.LogInformation("SkipMusic is not implemented");
        return Task.CompletedTask;
    }

    public override async Task StopAsync()
    {
        await _audioClient.StopAsync();

        _musicPlayerCollection.Remove(GuildID);
    }

    public override async Task StartAsync(CancellationToken cancellationToken, IAudioClient audioClient, ulong guildID, Music music)
    {
        _audioClient = audioClient;
        GuildID = guildID;

        AddToQueue(music);
        Player(cancellationToken);
    }
}