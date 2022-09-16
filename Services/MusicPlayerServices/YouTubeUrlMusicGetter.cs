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

    public async Task<Music> GetMusicAsync(string search)
    {
        var youtube = YouTube.Default;

        try
        {
            var video = await youtube.GetVideoAsync(search);
            Music music = new Music(video.Title, search, video.Uri, null);
            return music;
        }
        catch (ArgumentException e) { }
        catch (UnavailableStreamException e) { }

        return null;
    }
}