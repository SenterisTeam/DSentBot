using DSentBot.Models;

namespace DSentBot.Services.MusicPlayerServices;

public interface IMusicGetter
{
    public Task<Music> GetMusic(string search);
}