using System.Diagnostics;
using Discord.Audio;
using DSentBot.Models;

namespace DSentBot.Services.MusicPlayerServices;

public class MusicPlayerManager
{
    public ulong GuildID;

    private IAudioClient _audioClient;
    private readonly MusicPlayerCollection _musicPlayerCollection;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MusicPlayerManager> _logger;

    protected Queue<Music> _musicQueue { get; set; }

    public MusicPlayerManager(MusicPlayerCollection musicPlayerCollection, IServiceProvider serviceProvider, ILogger<MusicPlayerManager> logger)
    {
        _musicPlayerCollection = musicPlayerCollection;
        _serviceProvider = serviceProvider;
        _logger = logger;

        _musicQueue = new Queue<Music>();
    }

    protected async Task Player(CancellationToken cancellationToken)
    {
        while (_musicQueue.Count != 0 && !cancellationToken.IsCancellationRequested)
        {
            Music music = _musicQueue.Dequeue();

            // Take Player and play
            IMusicPlayer player = _serviceProvider.GetRequiredService<IMusicPlayer>();
            await player.PlayAsync(music, _audioClient);

            // TODO it doesn't finish
        }
    }

    public Task AddToQueue(Music music)
    {
        _musicQueue.Enqueue(music);

        return Task.CompletedTask;
    }

    public Task SkipMusic()
    {
        _logger.LogInformation("SkipMusic is not implemented");
        return Task.CompletedTask;
    }

    public async Task StopAsync()
    {
        await _audioClient.StopAsync();

        _musicPlayerCollection.Remove(GuildID);
    }

    public async Task StartAsync(CancellationToken cancellationToken, IAudioClient audioClient, ulong guildID, Music music)
    {
        _audioClient = audioClient;
        GuildID = guildID;

        AddToQueue(music);
        Player(cancellationToken);
    }
}