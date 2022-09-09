using System.Text.RegularExpressions;
using DSentBot.Models;
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
        var youtube = new YoutubeClient();
        try
        {
            VideoSearchResult video = await youtube.Search.GetVideosAsync(search).FirstAsync();
            Music music = new Music(video.Title, search, video.Url, video.Duration);

            var videoStream = await youtube.Videos.Streams.GetHttpLiveStreamUrlAsync(video.Id);
            music.Path = videoStream;
            return music;
        }
        catch (ArgumentException e)
        {
        }

        return null;
    }
}