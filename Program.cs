using DSentBot.Services;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using DSentBot.Services.DiscordBot;
using DSentBot.Services.DiscordBot.Configurations;
using DSentBot.Services.MusicPlayerServices;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Options;


var host = Host.CreateDefaultBuilder()
    .ConfigureLogging(l => l.AddDebug().SetMinimumLevel(LogLevel.Debug))
    .ConfigureServices((context, services) =>
    {
        services.Configure<DiscordHostConfiguration>(c =>
        {
            c.Token = context.Configuration["DSentBotToken"];
            
            c.SocketConfig = new DiscordSocketConfig()
            {
                LogLevel = LogSeverity.Info
            };
        });

        services.Configure<CommandServiceConfig>(c =>
        {
            c.DefaultRunMode = RunMode.Async;
            c.LogLevel = LogSeverity.Info;
        });

        services.AddSingleton<DiscordSocketClient>(c => new DiscordSocketClient(c.GetRequiredService<IOptions<DiscordHostConfiguration>>().Value.SocketConfig));
        services.AddSingleton<CommandService>(c => new CommandService(c.GetRequiredService<IOptions<CommandServiceConfig>>().Value));
        services.AddHostedService<DiscordHostedService>();
        services.AddHostedService<CommandHandlerService>();

        services.AddScoped<IMusicGetter, YouTubeUrlMusicGetter>();
        services.AddScoped<IMusicPlayer, WebToFFmpegPlayer>();

        services.AddSingleton<FFmpegCollection>();
        services.AddHostedService<FFmpegCollection>(c => c.GetRequiredService<FFmpegCollection>());
    })
    .Build();

await host.RunAsync();