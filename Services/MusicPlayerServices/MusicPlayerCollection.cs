using System.Collections.Concurrent;
using Discord.Audio;
using DSentBot.Models;

namespace DSentBot.Services.MusicPlayerServices;

public class MusicPlayerCollection : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    CancellationToken _cancellationToken;

    public MusicPlayerCollection(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    ConcurrentDictionary<ulong, MusicPlayerManager> _musicPlayers = new ();

    public void Add(ulong guildID, Task<IAudioClient> audioClient, Music music)
    {
        var musicPlayer = _serviceProvider.GetRequiredService<MusicPlayerManager>();
        Task.Run(async () => await musicPlayer.StartAsync(_cancellationToken, audioClient, guildID, music));
        _musicPlayers.TryAdd(guildID, musicPlayer);

    }

    public MusicPlayerManager Get(ulong guildID)
    {
        _musicPlayers.TryGetValue(guildID, out var musicPlayer);
        return musicPlayer;
    }

    public Task Remove(ulong guildID)
    {
        _musicPlayers.Remove(guildID, out _);

        return Task.CompletedTask;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _cancellationToken = cancellationToken;

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        foreach (var musicPlayer in _musicPlayers)
        {
            musicPlayer.Value.StopAsync();
        }
        return Task.CompletedTask;
    }
}