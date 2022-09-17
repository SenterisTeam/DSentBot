using DSentBot.Services;
using Discord;
using Discord.Commands;
using Discord.Rest;
using Discord.WebSocket;
using DSentBot;
using DSentBot.Services.DiscordBot;
using DSentBot.Services.DiscordBot.Configurations;
using DSentBot.Services.MusicPlayerServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Options;
using Serilog;


var host = Host.CreateDefaultBuilder()
    .UseSerilog((hostingContext, services, loggerConfiguration) => loggerConfiguration
        .ReadFrom.Configuration(hostingContext.Configuration)
        .MinimumLevel.Information()
        .Enrich.FromLogContext()
        .WriteTo.Console())
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

        services.AddTransient<IMusicGetter, YouTubeUrlMusicGetter>();
        services.AddTransient<IMusicGetter, YouTubeSearchMusicGetter>();
        services.AddTransient<MusicPlayerManager>();
        services.AddTransient<WebMusicPlayer>();
        services.AddTransient<LocalMusicPlayer>();

        services.AddDbContext<ApplicationDbContext>(c => c.UseSqlite("Data Source=test.db"));

        services.AddSingleton<MusicPlayerCollection>();
        services.AddHostedService<MusicPlayerCollection>(c => c.GetRequiredService<MusicPlayerCollection>());

        services.AddSingleton<FFmpegCollection>();
        services.AddHostedService<FFmpegCollection>(c => c.GetRequiredService<FFmpegCollection>());
    })
    .Build();

await host.RunAsync();