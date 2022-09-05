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
            Arguments = $"-hide_banner -loglevel panic -i - -ac 2 -f s16le -ar 48000 pipe:1", // #TODO change bitrate to 128k/256k
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardInput = true
        }));
    }

    public Process GetProcess()
    {
        AddNewProcess();
        return _processes.Dequeue();
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