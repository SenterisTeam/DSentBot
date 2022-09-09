using Discord;
using Discord.WebSocket;
using DSentBot.Services.DiscordBot.Configurations;
using Microsoft.Extensions.Options;

namespace DSentBot.Services.DiscordBot;

public class DiscordHostedService : IHostedService
{
    private readonly DiscordSocketClient _client;
    private IConfiguration _configuration;
    private ILogger<DiscordHostedService> _logger;
    private readonly DiscordHostConfiguration _options;


    public DiscordHostedService(DiscordSocketClient client, IConfiguration configuration, ILogger<DiscordHostedService> logger, IOptions<DiscordHostConfiguration> options)
    {
        _client = client;
        _configuration = configuration;
        _logger = logger;
        _options = options.Value;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _client.LoginAsync(TokenType.Bot, _options.Token, true).WaitAsync(cancellationToken);
            await _client.StartAsync().WaitAsync(cancellationToken);

            _client.Log += (LogMessage msg) =>
            {
                LogLevel level = msg.Severity switch
                {
                    LogSeverity.Critical => LogLevel.Critical,
                    LogSeverity.Error => LogLevel.Error,
                    LogSeverity.Warning => LogLevel.Warning,
                    LogSeverity.Info => LogLevel.Information,
                    LogSeverity.Verbose => LogLevel.Debug,
                    LogSeverity.Debug => LogLevel.Trace
                };
                _logger.Log<LogMessage>(level, new EventId(), msg, msg.Exception, (m, e) => $"[{m.Source}] {m.Message}");
                return Task.CompletedTask;
            };

            _client.Ready += () =>
            {
                _logger.LogInformation("Bot is connected!");
                return Task.CompletedTask;
            };
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