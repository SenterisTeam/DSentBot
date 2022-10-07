using System.Diagnostics;
using Discord;
using Discord.Audio;
using DSentBot.Models;
using Microsoft.EntityFrameworkCore;

namespace DSentBot.Services.MusicPlayerServices;

public class MusicPlayerManager
{
    public ulong GuildId;

    private Task<IAudioClient> _audioClient;
    private readonly MusicPlayerCollection _musicPlayerCollection;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MusicPlayerManager> _logger;
    private readonly ApplicationDbContext _dbContext;

    private CancellationTokenSource _cancellationTokenMusicSrc;
    private CancellationToken _cancellationTokenMusic;

    public Queue<Music> MusicQueue { get; private set; }

    public MusicPlayerManager(MusicPlayerCollection musicPlayerCollection, IServiceProvider serviceProvider, ILogger<MusicPlayerManager> logger, ApplicationDbContext dbContext)
    {
        _musicPlayerCollection = musicPlayerCollection;
        _serviceProvider = serviceProvider;
        _logger = logger;
        _dbContext = dbContext;

        MusicQueue = new Queue<Music>();
    }

    protected async Task Player(CancellationToken cancellationToken)
    {
        while (MusicQueue.Count != 0 && !cancellationToken.IsCancellationRequested)
        {
            Music lmusic = MusicQueue.Dequeue();

            // Normally combine variables
            IMusicPlayer player;
            Music music = await _dbContext.Musics.Where(m=> m.Url == lmusic.Url).FirstOrDefaultAsync();
            if (music != null)
            {
                music.RequestsNumber++;
                _dbContext.SaveChanges();
            }
            else
            {
                _dbContext.Musics.Add(lmusic);
                await _dbContext.SaveChangesAsync();
                music = await _dbContext.Musics.Where(m=> m.Url == lmusic.Url).FirstOrDefaultAsync();
            }

            music.UriToStream = lmusic.UriToStream;

            if (music.IsDownloaded)
            {
                Console.WriteLine(Path.GetFullPath(Path.Combine(".", music.LocalPath)));
                if (File.Exists(Path.GetFullPath(Path.Combine(".", music.LocalPath))))
                    player = _serviceProvider.GetRequiredService<LocalMusicPlayer>();
                else
                {
                    _logger.LogInformation($"Music {music.Name} not found!");
                    music.IsDownloaded = false;
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

        _musicPlayerCollection.Remove(GuildId);
    }

    private async Task AudioCheck(IAudioClient audioClient)
    {
        while (audioClient.ConnectionState != ConnectionState.Disconnected)
        {
            await Task.Delay(200);
        }
        StopAsync();
    }

    public async Task StartAsync(CancellationToken cancellationToken, Task<IAudioClient> audioClient, ulong guildId, Music music)
    {
        _audioClient = audioClient;
        GuildId = guildId;

        AddToQueue(music);
        Player(cancellationToken);
        AudioCheck(audioClient.Result);
    }
}