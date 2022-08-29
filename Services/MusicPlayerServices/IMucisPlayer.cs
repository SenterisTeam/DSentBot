using Discord;
using Discord.Audio;
using DSentBot.Models;

namespace DSentBot.Services.MusicPlayerServices;

public interface IMusicPlayer
{
    public Task Play(Music music, IAudioClient audioClient);
}