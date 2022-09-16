using System.Text.RegularExpressions;
using DSentBot.Models;
using VideoLibrary;
using YoutubeExplode;
using YoutubeExplode.Search;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;
using ArgumentException = System.ArgumentException;

namespace DSentBot.Services.MusicPlayerServices;

public class YouTubeSearchMusicGetter : IMusicGetter
{
    public async Task<Music> GetMusicAsync(string search)
    {
        var youtube = new YoutubeExplode.YoutubeClient();
        var youtubevl = VideoLibrary.YouTube.Default;
        try
        {
            VideoSearchResult video = await youtube.Search.GetVideosAsync(search).FirstAsync();
            //var videoStream = await youtube.Videos.GetAsync(video.Id); // TODO VideoLibrary -> YoutubeExplode
            var videoStream = await youtubevl.GetVideoAsync(video.Url);
            Music music = new Music(video.Title, video.Url, videoStream.Uri ,video.Duration);
            return music;
        }
        catch (ArgumentException e)
        {
        }

        return null;
    }
}