using System.Diagnostics;

namespace DSentBot.Services.MusicPlayerServices;

public class FFmpegCollection: IHostedService
{
    private Queue<Process> _processes = new();

    private void AddNewProcess()
    {
        _processes.Enqueue(Process.Start(new ProcessStartInfo
        {
            FileName = "ffmpeg",
            Arguments = $"-hide_banner -loglevel panic -i - -ac 2 -f s16le -ar 48000 pipe:1",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardInput = true
        }));
    }

    public Process GetStreamProcess()
    {
        AddNewProcess();
        return _processes.Dequeue();
    }

    public Process GetMusicConvertProcess(string musicPath)
    {
        return Process.Start(new ProcessStartInfo
        {
            FileName = "ffmpeg",
            Arguments = $"-hide_banner -loglevel panic -i - -ab 128k {musicPath}",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardInput = true
        });
    }

    public Process GetReadProcess(string musicPath)
    {
        return Process.Start(new ProcessStartInfo
        {
            FileName = "ffmpeg",
            Arguments = $"-hide_banner -loglevel panic -i {musicPath} -ac 2 -f s16le -ar 48000 pipe:1",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardInput = true
        });
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        for (int i = 0; i < 5; i++)
        {
            AddNewProcess();
        }
        
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        foreach (var process in _processes)
        {
            process.Close();
        }
        
        return Task.CompletedTask;
    }
}