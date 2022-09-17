using System.Diagnostics;
using Discord.Audio;
using DSentBot.Models;
using Microsoft.EntityFrameworkCore;

namespace DSentBot.Services.MusicPlayerServices;

public class LocalMusicPlayer : IMusicPlayer
{
    private readonly FFmpegCollection _ffmpegCollection;
    private readonly ILogger<LocalMusicPlayer> _logger;
    private readonly ApplicationDbContext _dbContext;
    private CancellationToken _cancellationToken;

    public LocalMusicPlayer(FFmpegCollection ffmpegCollection, ILogger<LocalMusicPlayer> logger, ApplicationDbContext dbContext)
    {
        _ffmpegCollection = ffmpegCollection;
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task PlayAsync(Music lMusic, Task<IAudioClient> audioClient, CancellationToken cancellationToken)
    {
        _logger.LogInformation("LocalMusicPlayer PlayAsync!");
        _cancellationToken = cancellationToken;

        Music music = await _dbContext.Musics.FirstOrDefaultAsync(m => m.Url == lMusic.Url);

        Process readerProcess = _ffmpegCollection.GetReadProcess(Path.GetFullPath(Path.Combine(".", music.LocalPath)));

        using (var discord = (await audioClient).CreatePCMStream(AudioApplication.Mixed, bitrate: 131072, bufferMillis: 1500, packetLoss: 0)) // Default bitrate is 96*1024
        {
            try
            {
                _logger.LogInformation($"Start Copying");
                await readerProcess.StandardOutput.BaseStream.CopyToAsync(discord, _cancellationToken);
            }
            catch (Exception e) { }
            finally
            {
                readerProcess.StandardInput.BaseStream.Close();
                await discord.FlushAsync(); // TODO Does it make a sense?
            }
        }
    }
}