using Discord;
using Discord.Audio;
using DSentBot.Models;

namespace DSentBot.Services.MusicPlayerServices;

public interface IMusicPlayer
{
    ulong GuildID { get; }
    Queue<Music> MusicQueue { get; }

    public Task AddToQueue(Music music);
    public Task SkipMusic();
    public Task StopAsync();

    public Task StartAsync(CancellationToken cancellationToken, IAudioClient audioClient, ulong guildID, Music music);
}