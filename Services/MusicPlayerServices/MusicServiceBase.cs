using Discord.Audio;
using DSentBot.Models;

namespace DSentBot.Services.MusicPlayerServices;

public abstract class MusicServiceBase : IMusicPlayer
{
    protected IAudioClient _audioClient;

    public ulong GuildID { get; protected set; }

    public Queue<Music> MusicQueue { get; set; }

    public abstract Task AddToQueue(Music music);

    protected abstract Task Player(CancellationToken cancellationToken);

    public abstract Task SkipMusic();

    public abstract Task StopAsync();

    public abstract Task StartAsync(CancellationToken cancellationToken, IAudioClient audioClient, Music music);
}