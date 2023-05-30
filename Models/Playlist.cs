using System.ComponentModel.DataAnnotations;

namespace DSentBot.Models;

public class Playlist
{
    [Key] public ulong Id { get; set; }
    [Required] public string Name { get; set; }
    public List<Music> Musics { get; set; }
    [Required] public User Author { get; set; }
}