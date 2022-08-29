using Discord;
using Discord.Commands;

namespace DSentBot.Services.DiscordBot.Modules;

public class MusicModule : ModuleBase<SocketCommandContext>
{
    [Command("play")]
    [Summary("Add bot to vc and add music to queue")]
    public async Task PlayMusicAsync(IVoiceChannel channel = null)
    {
        // Get the audio channel
        channel = channel ?? (Context.User as IGuildUser)?.VoiceChannel;
        if (channel == null) { await Context.Channel.SendMessageAsync("Connect to a voice channel first!."); return; }

        // For the next step with transmitting audio, you would want to pass this Audio Client in to a service.
        var audioClient = await channel.ConnectAsync();
    }
}