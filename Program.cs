using DSentBot.Services;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using DSentBot.Services.DiscordBot;
using DSentBot.Services.MusicPlayerServices;
using Microsoft.Extensions.Logging.Configuration;

namespace DSentBot;

public class Program
{
    public Program()
    {
        _provider = CreateHostBuilder();
    }

    private IHostBuilder _provider;

    public static Task Main(string[] args) => new Program().MainAsync();

    private DiscordSocketClient _client;


    async Task MainAsync()
    {
        CreateHostBuilder().Build().Run();
    }

    public static DiscordSocketConfig dsconfig = new()
    {
        LogLevel = LogSeverity.Info
    };

    public static CommandServiceConfig cmdConfig = new()
    {
        DefaultRunMode = RunMode.Async,
        LogLevel = LogSeverity.Info
    };

    public static IHostBuilder CreateHostBuilder() =>
        Host.CreateDefaultBuilder()
            .ConfigureLogging(l => l.AddDebug().SetMinimumLevel(LogLevel.Debug))
            .ConfigureServices(services =>
            {
                services.AddSingleton<DiscordSocketClient>(new DiscordSocketClient(dsconfig));
                services.AddSingleton<CommandService>(new CommandService(cmdConfig));
                services.AddHostedService<DiscordHostedService>();
                services.AddHostedService<CommandHandlerService>();

                services.AddScoped<IMusicGetter, YouTubeUrlMusicGetter>();
                services.AddScoped<IMusicPlayer, WebToFFmpegPlayer>();

                services.AddSingleton<FFmpegCollection>();
                services.AddHostedService<FFmpegCollection>(c => c.GetRequiredService<FFmpegCollection>());
            });
}
