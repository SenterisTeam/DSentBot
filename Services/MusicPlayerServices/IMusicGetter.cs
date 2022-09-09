using DSentBot.Models;

namespace DSentBot.Services.MusicPlayerServices;

public interface IMusicGetter
{
    public Task<Music> GetMusicAsync(string search);
}