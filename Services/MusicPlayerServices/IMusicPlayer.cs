using Discord.Audio;
using DSentBot.Models;

namespace DSentBot.Services.MusicPlayerServices;

public interface IMusicPlayer
{
    Task PlayAsync(Music music, Task<IAudioClient>  audioClient);
}