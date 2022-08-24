using System.Reflection;
using Discord.Commands;
using Discord.WebSocket;

namespace DSentBot.Services;

public class CommandHandlerService : BackgroundService
{
    private readonly DiscordSocketClient _client;
    private readonly CommandService _commands;
    private readonly ILogger<CommandHandlerService> _logger;

    public CommandHandlerService(CommandService commandService, DiscordSocketClient discordSocketClient, ILogger<CommandHandlerService> logger)
    {
        _commands = commandService;
        _client = discordSocketClient;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Execute CommandHandlerService");
        _client.MessageReceived += HandleCommandAsync;

        await _commands.AddModulesAsync(assembly: Assembly.GetEntryAssembly(),
            services: null);
    }

    private async Task HandleCommandAsync(SocketMessage messageParam)
    {
        // Don't process the command if it was a system message
        var message = messageParam as SocketUserMessage;
        Console.WriteLine(message);
        if (message == null) return;

        // Create a number to track where the prefix ends and the command begins
        int argPos = 0;

        // Determine if the message is a command based on the prefix and make sure no bots trigger commands
        if (!(message.HasCharPrefix('&', ref argPos) ||
            message.HasMentionPrefix(_client.CurrentUser, ref argPos)) ||
            message.Author.IsBot)
            return;

        // Create a WebSocket-based command context based on the message
        var context = new SocketCommandContext(_client, message);

        // Execute the command with the command context we just
        // created, along with the service provider for precondition checks.
        await _commands.ExecuteAsync(
            context: context,
            argPos: argPos,
            services: null);
    }

}