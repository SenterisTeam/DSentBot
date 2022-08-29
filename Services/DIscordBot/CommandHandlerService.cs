using System.Reflection;
using Discord.Commands;
using Discord.WebSocket;

namespace DSentBot.Services.DiscordBot;

public class CommandHandlerService : BackgroundService
{
    private readonly DiscordSocketClient _client;
    private readonly CommandService _commands;
    private readonly ILogger<CommandHandlerService> _logger;
    private readonly IServiceProvider _provider;

    public CommandHandlerService(CommandService commandService, DiscordSocketClient discordSocketClient,
        ILogger<CommandHandlerService> logger, IServiceProvider provider)
    {
        _commands = commandService;
        _client = discordSocketClient;
        _logger = logger;
        _provider = provider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using (var scope = _provider.CreateScope())
        {
            _logger.LogInformation("Execute CommandHandlerService");
            _client.MessageReceived += HandleCommandAsync;

            await _commands.AddModulesAsync(assembly: Assembly.GetEntryAssembly(), services: scope.ServiceProvider);
        }
    }

    private async Task HandleCommandAsync(SocketMessage messageParam)
    {
        // Don't process the command if it was a system message
        var message = messageParam as SocketUserMessage;
        if (message == null) return;

        // Create a number to track where the prefix ends and the command begins
        int argPos = 0;

        // Determine if the message is a command based on the prefix and make sure no bots trigger commands
        if (!(message.HasCharPrefix('~', ref argPos) ||
              message.HasMentionPrefix(_client.CurrentUser, ref argPos)) ||
            message.Author.IsBot)
            return;

        _logger.LogInformation("cmd: " + message);

        // Create a WebSocket-based command context based on the message
        var context = new SocketCommandContext(_client, message);

        // Execute the command with the command context we just
        // created, along with the service provider for precondition checks.
        await _commands.ExecuteAsync(
            context: context,
            argPos: argPos,
            services: _provider);
    }
}