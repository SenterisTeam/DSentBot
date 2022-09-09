using Discord.Commands;

namespace DSentBot.Services.DiscordBot.Modules;

public class Result: RuntimeResult
{
    public Result(CommandError? error, string reason) : base(error, reason) { }

    public static Result FromError(string reason) => new Result(CommandError.Unsuccessful, reason);
}