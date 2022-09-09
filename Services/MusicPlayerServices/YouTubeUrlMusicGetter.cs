using System.Text.RegularExpressions;
using DSentBot.Models;
using VideoLibrary;
using VideoLibrary.Exceptions;
using ArgumentException = System.ArgumentException;

namespace DSentBot.Services.MusicPlayerServices;

public class YouTubeUrlMusicGetter : IMusicGetter
{
    private readonly ILogger<YouTubeUrlMusicGetter> _logger;

    public YouTubeUrlMusicGetter(ILogger<YouTubeUrlMusicGetter> logger)
    {
        _logger = logger;
    }

    public async Task<Music> GetMusic(string search)
    {
        var youtube = YouTube.Default;

        try
        {
            var video = await youtube.GetVideoAsync(search);
            return new Music(video.Title, video.Uri, search);
        }
        catch (ArgumentException e) { }
        catch (UnavailableStreamException e) { }

        return null;
    }
}