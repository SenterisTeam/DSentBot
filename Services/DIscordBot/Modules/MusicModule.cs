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
    private readonly MusicPlayerCollection _mpCollection;
    private readonly ILogger<MusicModule> _logger;

    public MusicModule(IServiceProvider provider, MusicPlayerCollection mpCollection, ILogger<MusicModule> logger)
    {
        _provider = provider;
        _mpCollection = mpCollection;
        _logger = logger;
    }
    
    [Command("play", RunMode = RunMode.Async)]
    [Summary("Add bot to vc and add music to queue")]
    public async Task PlayMusicAsync(string search, IVoiceChannel channel = null)
    {
        // Get the audio channel
        channel = channel ?? (Context.User as IGuildUser)?.VoiceChannel;
        if (channel == null) { await Context.Channel.SendMessageAsync("Connect to a voice channel first!."); return; }

        var music = _provider.GetRequiredService<IMusicGetter>().GetMusic(search); // Will ber GetServices later

        if (music == null) return;
        MusicPlayerManager playerManager = _mpCollection.Get(Context.Guild.Id);
        if (playerManager == null)
        {
            Task<IAudioClient> audioClient = channel.ConnectAsync();
            await music;
            _mpCollection.Add(Context.Guild.Id, audioClient, music.Result);
        }
        else
        {
            await playerManager.AddToQueue(await music);
        }
    }

    [Command("stop", RunMode = RunMode.Async)]
    [Summary("Disconnect bot from guild voice channel")]
    public async Task StopMusic()
    {
        MusicPlayerManager playerManager = _mpCollection.Get(Context.Guild.Id);
        if (playerManager != null) await playerManager.StopAsync();
        else await ReplyAsync("Bot is not connected!");
    }
}