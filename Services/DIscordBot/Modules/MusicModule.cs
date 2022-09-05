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

    public MusicModule(IServiceProvider provider, MusicPlayerCollection mpCollection)
    {
        _provider = provider;
        _mpCollection = mpCollection;
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
        IMusicPlayer player = _mpCollection.Get(Context.Guild.Id);
        if (player == null)
        {
            Task<IAudioClient> audioClient = channel.ConnectAsync();
            await Task.WhenAll(music, audioClient);
            _mpCollection.Add(Context.Guild.Id, audioClient.Result, music.Result);
        }
        else
        {
            await player.AddToQueue(await music);
        }
    }
}