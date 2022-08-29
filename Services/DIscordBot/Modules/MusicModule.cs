using System.Diagnostics;
using Discord;
using Discord.Audio;
using Discord.Commands;
using DSentBot.Models;
using DSentBot.Services.MusicPlayerServices;

namespace DSentBot.Services.DiscordBot.Modules;

public class MusicModule : ModuleBase<SocketCommandContext>
{
    private readonly IServiceProvider _provider;

    public MusicModule(IServiceProvider provider)
    {
        _provider = provider;
    }
    
    [Command("play", RunMode = RunMode.Async)]
    [Summary("Add bot to vc and add music to queue")]
    public async Task PlayMusicAsync(string search, IVoiceChannel channel = null)
    {
        // Get the audio channel
        channel = channel ?? (Context.User as IGuildUser)?.VoiceChannel;
        if (channel == null) { await Context.Channel.SendMessageAsync("Connect to a voice channel first!."); return; }

        var music = _provider.GetRequiredService<IMusicGetter>().GetMusic(search); // Will ber GetServices later

        // For the next step with transmitting audio, you would want to pass this Audio Client in to a service.
        var audioClient = channel.ConnectAsync();

        await Task.WhenAll(music, audioClient);

        var player = _provider.GetRequiredService<IMusicPlayer>();

        if (music != null)
            await player.Play(music.Result, audioClient.Result);
    }
}