using System.Diagnostics;
using Discord.Audio;
using DSentBot.Models;
using Microsoft.EntityFrameworkCore;

namespace DSentBot.Services.MusicPlayerServices;

public class MusicPlayerManager
{
    public ulong GuildID;

    private Task<IAudioClient> _audioClient;
    private readonly MusicPlayerCollection _musicPlayerCollection;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MusicPlayerManager> _logger;
    private readonly ApplicationDbContext _dbContext;
    private readonly IHostEnvironment _hostEnvironment;

    private CancellationTokenSource _cancellationTokenMusicSrc;
    private CancellationToken _cancellationTokenMusic;

    public Queue<Music> MusicQueue { get; private set; }

    public MusicPlayerManager(MusicPlayerCollection musicPlayerCollection, IServiceProvider serviceProvider, ILogger<MusicPlayerManager> logger, ApplicationDbContext dbContext, IHostEnvironment hostEnvironment)
    {
        _musicPlayerCollection = musicPlayerCollection;
        _serviceProvider = serviceProvider;
        _logger = logger;
        _dbContext = dbContext;
        _hostEnvironment = hostEnvironment;

        MusicQueue = new Queue<Music>();
    }

    protected async Task Player(CancellationToken cancellationToken)
    {
        while (MusicQueue.Count != 0 && !cancellationToken.IsCancellationRequested)
        {
            Music music = MusicQueue.Dequeue();

            // Take Player and play
            IMusicPlayer player;
            Music dbMusic = await _dbContext.Musics.Where(m => m.IsDownloaded).Where(m=> m.Url == music.Url).FirstOrDefaultAsync();
            if (dbMusic != null)
            {
                music.RequestsNumber++;
                _dbContext.SaveChanges();
            }

            if (dbMusic != null && dbMusic.IsDownloaded)
            {
                if (File.Exists($"{_hostEnvironment.ContentRootPath}/music/{music.Id.ToString()}.mp3"))
                    player = _serviceProvider.GetRequiredService<LocalMusicPlayer>();
                else
                {
                    dbMusic.IsDownloaded = false;
                    await _dbContext.SaveChangesAsync();
                    player = _serviceProvider.GetRequiredService<WebMusicPlayer>();
                }
            }
            else player = _serviceProvider.GetRequiredService<WebMusicPlayer>();

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