using System.Text.RegularExpressions;
using DSentBot.Models;
using VideoLibrary;
using VideoLibrary.Exceptions;
using YoutubeExplode;
using YoutubeExplode.Search;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;
using ArgumentException = System.ArgumentException;

namespace DSentBot.Services.MusicPlayerServices;

public class YouTubeSearchMusicGetter : IMusicGetter
{
    private readonly ILogger<YouTubeSearchMusicGetter> _logger;

    public YouTubeSearchMusicGetter(ILogger<YouTubeSearchMusicGetter> logger)
    {
        _logger = logger;
    }

    public async Task<Music> GetMusicAsync(string search)
    {
        var youtube = new YoutubeExplode.YoutubeClient();
        var youtubevl = VideoLibrary.YouTube.Default;
        try
        {
            VideoSearchResult video = await youtube.Search.GetVideosAsync(search).FirstAsync();
            //var videoStream = await youtube.Videos.GetAsync(video.Id); // TODO VideoLibrary -> YoutubeExplode
            var videoStream = await youtubevl.GetVideoAsync(video.Url);
            Music music = new Music(video.Title, video.Url, videoStream.Uri, video.Duration);
            return music;
        }
        catch (ArgumentException e)
        {
            _logger.LogDebug("ArgumentException for " + search);
        }
        catch (UnavailableStreamException e)
        {
            _logger.LogInformation("UnavailableStreamException for " + search);
        }
        catch (Exception e)
        {
            _logger.LogError(e.ToString());
        }

        return null;
    }
}