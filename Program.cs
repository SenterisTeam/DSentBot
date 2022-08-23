using DSentBot.Services;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace DSentBot;

public class Program
{
    private readonly IServiceProvider _provider;

    public Program()
    {
        _provider = CreateProvider();
    }

    public static Task Main(string[] args) => new Program().MainAsync();

    private DiscordSocketClient _client;

    static IServiceProvider CreateProvider()
    {
        var config = new DiscordSocketConfig()
        {
            LogLevel = LogSeverity.Debug
        };

        var collection = new ServiceCollection()
            .AddSingleton(config)
            .AddSingleton<DiscordSocketClient>()
            .AddSingleton<CommandService>()
            .AddSingleton<CommandHandlerService>();

        return collection.BuildServiceProvider();
    }

    async Task MainAsync()
    {
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
    }

    private Task CommandHandler(SocketMessage msg)
    {
        throw new NotImplementedException();
    }
}