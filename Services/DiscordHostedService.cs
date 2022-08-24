using Discord;
using Discord.WebSocket;

namespace DSentBot.Services;

public class DiscordHostedService : IHostedService
{
    private readonly DiscordSocketClient _client;
    private IConfiguration _configuration;
    private ILogger<DiscordHostedService> _logger;


    public DiscordHostedService(DiscordSocketClient client, IConfiguration configuration, ILogger<DiscordHostedService> logger)
    {
        _client = client;
        _configuration = configuration;
        _logger = logger;
    }

    protected async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _client.Log += (LogMessage msg) =>
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        };

        _client.Ready += () =>
        {
            Console.WriteLine("Bot is connected!");
            return Task.CompletedTask;
        };

        await Task.Delay(-1); // Block this task until the program is closed.
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            var token = Environment.GetEnvironmentVariable("DSentBotToken");
            await _client.LoginAsync(TokenType.Bot, token, true).WaitAsync(cancellationToken);
            await _client.StartAsync().WaitAsync(cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogCritical(e, e.Message);
            throw e;
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _client.StopAsync().WaitAsync(cancellationToken);
        }
        catch(Exception e){_logger.LogError(e, e.Message);}
    }
}