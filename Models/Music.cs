using System.ComponentModel.DataAnnotations.Schema;

namespace DSentBot.Models;

public class Music
{
    public long Id { get; set; } // key for DB
    public string Name { get; set; }
    //[NotMapped]
    public string LocalPath => $"/music/{Id}.mp3";
    public string Url { get; set; }
    //[NotMapped]
    public string UriToStream { get; set; }
    //[NotMapped]
    public TimeSpan? Duration { get; set; }
    public long RequestsNumber { get; set; }

    public Music(string name, string url, string uriToStream, TimeSpan? duration)
    {
        Name = name;
        Url = url;
        Duration = duration;
        UriToStream = uriToStream;

        RequestsNumber = 1;
    }
}