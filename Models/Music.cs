using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DSentBot.Models;

public class Music
{
    [Key] public long Id { get; set; } // key for DB
    public string Name { get; set; }

    [NotMapped] public string LocalPath => $"/music/{Id}.mp3";
    public bool IsDownloaded(string root) => File.Exists(root+"/music/{Id}.mp3");

    public string Url { get; set; }
    [NotMapped] public string UriToStream { get; set; }
    [NotMapped] public TimeSpan? Duration { get; set; }
    public long RequestsNumber { get; set; } = 1;
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public Music(){}

    public Music(string name, string url, string uriToStream, TimeSpan? duration)
    {
        Name = name;
        Url = url;
        Duration = duration;
        UriToStream = uriToStream;

        RequestsNumber = 1;
        CreatedAt = DateTime.Now;
    }
}