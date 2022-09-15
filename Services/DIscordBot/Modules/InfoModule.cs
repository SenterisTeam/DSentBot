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

    [Command ("help")]
    public async Task HelpAsync()
    {
        EmbedBuilder embed = new EmbedBuilder{
            Title = "Command list:",
            Description = "Other information you can get [there.](https://github.com/SenterisTeam/DSentBot)"
        };
        embed.WithFooter(footer => footer.Text = "By Senteris Team.");
        embed.AddField("Info", "ping \n~help", false);
        embed.AddField("Music", "play *youtube music url* \n~stop", false);
        embed.WithColor(Color.Gold);
        await ReplyAsync(embed: embed.Build());
    }
}