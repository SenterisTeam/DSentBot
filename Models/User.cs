using System.ComponentModel.DataAnnotations;

namespace DSentBot.Models;

public class User
{
    [Key] public ulong Id { get; set; }
    [Required] public string Username { get; set; }
    [Required] public ulong DiscordId { get; set; }
    public List<Playlist> Playlists { get; set; }
}