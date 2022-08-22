using System.Reflection;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace DSentBot.Services;

public class CommandHandlerService
{
    private readonly DiscordSocketClient _client;
    private readonly CommandService _commands;

    public CommandHandlerService(IServiceProvider provider)
    {
        _commands = provider.GetRequiredService<CommandService>();
        _client = provider.GetRequiredService<DiscordSocketClient>();

    Task.Run(() => InstallCommandsAsync());
    }

    public async Task InstallCommandsAsync()
    {
        Console.WriteLine("InstallCommandsAsync");
        _client.MessageReceived += HandleCommandAsync;

        // If you do not use Dependency Injection, pass null.
        // See Dependency Injection guide for more information.
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