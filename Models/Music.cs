using System.ComponentModel.DataAnnotations.Schema;

namespace DSentBot.Models;

public class Music
{
    public Music(string name, string path, string enteredSearch)
    {
        Name = name;
        Path = path;
        EnteredSearch = enteredSearch;
    }
    public string Name { get; set; }
    public string Path { get; set; }
    [NotMapped]
    public TimeSpan Duration { get; }
    public string EnteredSearch { get; set; }
}