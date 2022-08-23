namespace DSentBot.Services.MusicPlayerServices;

public class Music
{
    public Music(string name, string path, string enteredSearch)
    {
        Name = name;
        Path = path;
        EnteredSearch = enteredSearch;
    }
    public string Name { get; private set; }
    public string Path { get; private set; }
    public TimeSpan Duration { get; }
    public string EnteredSearch { get; private set; }
}