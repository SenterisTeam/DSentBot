using Discord.WebSocket;

namespace DSentBot.Services.DiscordBot.Configurations;

public class DiscordHostConfiguration
{
    public DiscordSocketConfig SocketConfig { get; set; } = new();
    public string Token { get; set; } = string.Empty;
}