using DSentBot.Models;
using DSentBot.Services.Cleaner;

namespace DSentBot.Services.MusicPlayerServices;

public class MusicEventArgs : EventArgs
{
    public Music Music { get; set; }
}

public class MusicEventDispatcher
{
    private readonly CleanerHostedService _cleanerService;

    public MusicEventDispatcher(CleanerHostedService cleanerService)
    {
        _cleanerService = cleanerService;
        MusicDownloaded += OnMusicDownloadedAsync;
    }

    public delegate Task AsyncEventHandler(object sender, MusicEventArgs e);
    public event AsyncEventHandler MusicDownloaded;

    public async Task RaiseMusicDownloadedAsync(object sender, MusicEventArgs e)
    {
        var eventHandlers = MusicDownloaded;
        if (eventHandlers != null)
        {
            foreach (var handler in eventHandlers.GetInvocationList())
            {
                var asyncHandler = (AsyncEventHandler)handler;
                await asyncHandler(sender, e);
            }
        }
    }

    private async Task OnMusicDownloadedAsync(object sender, MusicEventArgs e)
    {
        await _cleanerService.CleanAsync();
    }
}