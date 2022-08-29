using Discord;
using Discord.Commands;

namespace DSentBot.Services.DiscordBot.Modules;

public class InfoModule : ModuleBase<SocketCommandContext>
{
    [Command("ping")]
    [Summary("Replies ping")]
    public async Task PingAsync()
    {
        DateTimeOffset pingDT = Context.Message.CreatedAt;
        ReplyAsync("Ping!").ContinueWith((reply) =>
        {
            DateTimeOffset pongDT = reply.Result.CreatedAt;
            reply.Result.ModifyAsync(m => m.Content = $"Pong! \n`Ping: {(pongDT - pingDT).TotalMilliseconds}ms`");
        });

    }
}