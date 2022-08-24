using DSentBot.Services;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;

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

        /*
        _client = _provider.GetRequiredService<DiscordSocketClient>();
        _provider.GetRequiredService<CommandHandlerService>();

        _client.Log += (LogMessage msg) =>
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        };

        var token = Environment.GetEnvironmentVariable("DSentBotToken");
        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();

        _client.Ready += () =>
        {
            Console.WriteLine("Bot is connected!");
            return Task.CompletedTask;
        };

        await Task.Delay(-1); // Block this task until the program is closed.
        */
    }

    static DiscordSocketConfig dsconfig = new()
    {
        LogLevel = LogSeverity.Debug
    };
    public static IHostBuilder CreateHostBuilder() =>
        Host.CreateDefaultBuilder()
            .ConfigureServices(services =>
            {
                services.AddSingleton(dsconfig);
                services.AddSingleton<DiscordSocketClient>();
                services.AddSingleton<CommandService>();
                services.AddHostedService<DiscordHostedService>();
                services.AddHostedService<CommandHandlerService>();

            });
}
