using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DSentBot.Models;

public class Music
{
    [Key] public long Id { get; set; }
    public string Name { get; set; }

    [NotMapped] public string LocalPath => $"/music/{Id}.mp3";
    public bool IsDownloaded { get; set; } = false;

    public string Url { get; set; }
    [NotMapped] public string UriToStream { get; set; }
    [NotMapped] public TimeSpan? Duration { get; set; }
    public long RequestsNumber { get; set; } = 1;
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [NotMapped] public double Rating
    {
        get
        {
            return RequestsNumber / (DateTime.Now - CreatedAt).Days / Math.Pow(Duration.Value.Minutes, 0.4) * 100;
        }
    }

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