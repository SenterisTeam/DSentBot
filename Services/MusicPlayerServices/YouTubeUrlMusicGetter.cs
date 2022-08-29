using System.Text.RegularExpressions;
using DSentBot.Models;
using VideoLibrary;
using VideoLibrary.Exceptions;
using ArgumentException = System.ArgumentException;

namespace DSentBot.Services.MusicPlayerServices;

public class YouTubeUrlMusicGetter : IMusicGetter
{
    public async Task<Music> GetMusic(string search)
    {
        var youtube = YouTube.Default;

        try
        {
            var video = await youtube.GetVideoAsync("https://www.youtube.com/watch?v=l4uPgYnC2fM");
            return new Music(video.Title, video.Uri, search);
        }
        catch (ArgumentException e) { }
        catch (UnavailableStreamException e) { }

        return null;
    }
}