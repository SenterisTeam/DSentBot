using System.Diagnostics;
using Discord.Audio;
using DSentBot.Models;

namespace DSentBot.Services.MusicPlayerServices;

public class MusicPlayerManager
{
    public ulong GuildID;

    private Task<IAudioClient> _audioClient;
    private readonly MusicPlayerCollection _musicPlayerCollection;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MusicPlayerManager> _logger;

    private CancellationTokenSource _cancellationTokenMusicSrc;
    private CancellationToken _cancellationTokenMusic;

    public Queue<Music> MusicQueue { get; private set; }

    public MusicPlayerManager(MusicPlayerCollection musicPlayerCollection, IServiceProvider serviceProvider, ILogger<MusicPlayerManager> logger)
    {
        _musicPlayerCollection = musicPlayerCollection;
        _serviceProvider = serviceProvider;
        _logger = logger;

        MusicQueue = new Queue<Music>();
    }

    protected async Task Player(CancellationToken cancellationToken)
    {
        while (MusicQueue.Count != 0 && !cancellationToken.IsCancellationRequested)
        {
            Music music = MusicQueue.Dequeue();

            // Take Player and play
            IMusicPlayer player = _serviceProvider.GetRequiredService<IMusicPlayer>();
            _cancellationTokenMusicSrc = new CancellationTokenSource();
            _cancellationTokenMusic = _cancellationTokenMusicSrc.Token;
            await player.PlayAsync(music, _audioClient, _cancellationTokenMusic);
        }

        await StopAsync();
    }

    public Task AddToQueue(Music music)
    {
        MusicQueue.Enqueue(music);

        return Task.CompletedTask;
    }

    public Task SkipMusic()
    {
        _cancellationTokenMusicSrc.Cancel();
        return Task.CompletedTask;
    }

    public async Task StopAsync()
    {
        _cancellationTokenMusicSrc.Cancel();
        await (await _audioClient).StopAsync();

        _musicPlayerCollection.Remove(GuildID);
    }

    public async Task StartAsync(CancellationToken cancellationToken, Task<IAudioClient> audioClient, ulong guildID, Music music)
    {
        _audioClient = audioClient;
        GuildID = guildID;

        AddToQueue(music);
        Player(cancellationToken);
    }
}