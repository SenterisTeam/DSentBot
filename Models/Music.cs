using System.ComponentModel.DataAnnotations.Schema;

namespace DSentBot.Models;

public class Music
{
    public long Id { get; set; } // key for DB
    public string Name { get; set; }
    public string Path { get; set; }
    public string Url { get; set; }
    [NotMapped]
    public TimeSpan? Duration { get; set; }
    public string EnteredSearch { get; set; }

    public Music(string name, string enteredSearch, string url, TimeSpan? duration)
    {
        Name = name;
        EnteredSearch = enteredSearch;
        Url = url;
        Duration = duration;
    }
}