using DSentBot.Services;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
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

    public static IHostBuilder CreateHostBuilder() =>
        Host.CreateDefaultBuilder()
            .ConfigureLogging(l => l.AddDebug().SetMinimumLevel(LogLevel.Debug))
            .ConfigureServices(services =>
            {
                services.AddSingleton<DiscordSocketClient>(new DiscordSocketClient(dsconfig));
                services.AddSingleton<CommandService>();
                services.AddHostedService<DiscordHostedService>();
                services.AddHostedService<CommandHandlerService>();

            });
}
