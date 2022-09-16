using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.Http.Headers;
using Discord.Audio;
using DSentBot.Models;
using Microsoft.EntityFrameworkCore;

namespace DSentBot.Services.MusicPlayerServices;

public class WebMusicPlayer : IMusicPlayer
{
    private readonly FFmpegCollection _ffmpegCollection;
    private readonly ILogger<WebMusicPlayer> _logger;
    private readonly IHostEnvironment _hostEnvironment;
    private readonly ApplicationDbContext _dbContext;
    private CancellationToken _cancellationToken;

    private HttpClient _client = new();
    private ConcurrentDictionary<int, MapElement> _chuckMap = new();
    private byte[] _array;
    private long _length;

    private Process _streamProcess;
    private Process _musicConverterProcess;

    public WebMusicPlayer(FFmpegCollection ffmpegCollection, ILogger<WebMusicPlayer> logger, IHostEnvironment hostEnvironment, ApplicationDbContext dbContext)
    {
        _ffmpegCollection = ffmpegCollection;
        _logger = logger;
        _hostEnvironment = hostEnvironment;
        _dbContext = dbContext;
    }

    public async Task PlayAsync(Music music, Task<IAudioClient> audioClient, CancellationToken cancellationToken)
    {
        _cancellationToken = cancellationToken;
        _logger.LogInformation($"Start Play");
        
        _chuckMap.Clear();
        _length = (long) await GetContentLengthAsync(music.UriToStream);
        _array = new byte[_length];
        var k = 1.5f;
        long segmentPointer = 0;
        long chunkSize = 64_000;
        var counter = 0;
        var tasksList = new List<Task>();

        while (segmentPointer < _length)
        {
            tasksList.Add(StartDownloadTask(segmentPointer, chunkSize, counter, music.UriToStream));

            segmentPointer += chunkSize;
            chunkSize = (long)Math.Floor(chunkSize * k);
            counter++;
        }

        using (var ffmpeg = CreateStream(music))
        using (var output = ffmpeg.StandardOutput.BaseStream)
        using (var discord = (await audioClient).CreatePCMStream(AudioApplication.Mixed, bitrate: 131072, bufferMillis: 2500, packetLoss: 0)) // Default bitrate is 96*1024
        {
            try
            {
                // var buffer = new byte[16 * 1024];
                // int read;
                // while ((read = await output.ReadAsync(buffer, 0, 16 * 1024)) > 0)
                // {
                //     await discord.WriteAsync(buffer, 0, read);
                // }
                _logger.LogInformation($"Start Copying");
                await output.CopyToAsync(discord, _cancellationToken);
            }
            catch (OperationCanceledException e)
            {
                _streamProcess.StandardInput.BaseStream.Close();
                _musicConverterProcess.StandardInput.BaseStream.Close();
                try
                {
                    await Task.Delay(5000);
                    File.Delete($"{_hostEnvironment.ContentRootPath}/music/{music.Id.ToString()}.mp3");
                }
                catch (Exception ex) { _logger.LogTrace($"{_hostEnvironment.ContentRootPath}/music/{music.Id.ToString()}.mp3 {ex.ToString()}"); }
            }
            finally
            {
                await discord.FlushAsync();
            }
        }
    }

    private Process CreateStream(Music music)
    {
        _streamProcess = _ffmpegCollection.GetStreamProcess();

        Task.Factory.StartNew(async () =>
        {
            _dbContext.Musics.Add(music);
            await _dbContext.SaveChangesAsync();

            if (!Directory.Exists(_hostEnvironment.ContentRootPath + @"\music"))
                Directory.CreateDirectory(_hostEnvironment.ContentRootPath + @"\music");

            _musicConverterProcess = _ffmpegCollection.GetMusicConvertProcess(_hostEnvironment.ContentRootPath+music.LocalPath);

            long downloadedPointer = 0;

            while (downloadedPointer + 1023 < _length && !_cancellationToken.IsCancellationRequested)
            {
                if (IsRangeReady(downloadedPointer, downloadedPointer + 1023))
                {
                    if (downloadedPointer == 0) _logger.LogInformation($"First network read");
                    Task dsWriteTask = _streamProcess.StandardInput.BaseStream.WriteAsync(_array, (int) downloadedPointer, 1024);
                    Task fsWriteTask = _musicConverterProcess.StandardInput.BaseStream.WriteAsync(_array, (int) downloadedPointer, 1024);
                    await Task.WhenAll(dsWriteTask, fsWriteTask);
                    downloadedPointer += 1024;
                }
                else
                {
                    await Task.Delay(1000, _cancellationToken); // TODO Test
                }
            }
            _streamProcess.StandardInput.BaseStream.Close();
            _musicConverterProcess.StandardInput.BaseStream.Close();
            music.IsDownloaded = true;
            _dbContext.SaveChangesAsync();


        }, _cancellationToken);

        return _streamProcess;
    }

    private async Task<long?> GetContentLengthAsync(string requestUrl, bool ensureSuccess = true)
    {
        using (var request = new HttpRequestMessage(HttpMethod.Head, requestUrl))
        {
            var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            if (ensureSuccess)
                response.EnsureSuccessStatusCode();
            return response.Content.Headers.ContentLength;
        }
    }

    bool IsRangeReady(long start, long end)
    {
        var startSegment = FindSegment(start);
        var endSegment = FindSegment(end);

        if (startSegment.Key == endSegment.Key)
        {
            var readyPointer = endSegment.Value.SegmentPointer + endSegment.Value.DownloadedBytesCount;
            // Console.WriteLine($"Current {endSegment.Key} | {readyPointer} | {end}");
            return end <= readyPointer;
        }
        else
        {
            var startSegmentReady = startSegment.Value.DownloadedBytesCount == startSegment.Value.ChunkSize;
            var endSegmentReady = end <= endSegment.Value.SegmentPointer + endSegment.Value.DownloadedBytesCount;
            return startSegmentReady && endSegmentReady;
        }
    }

    KeyValuePair<int, MapElement> FindSegment(long pointer) => _chuckMap.OrderByDescending(e => e.Key).First(e => e.Value.SegmentPointer <= pointer);

    Task StartDownloadTask(long segmentPointerL, long chunkSizeL, int counter, string url)
    {
        var currentChunkSize = Math.Min(chunkSizeL, _length - segmentPointerL);
        var currentSegmentPointer = segmentPointerL;

        _chuckMap.TryAdd(counter, new MapElement()
        {
            ChunkSize = currentChunkSize,
            SegmentPointer = currentSegmentPointer,
            DownloadedBytesCount = 0
        });

        return Task.Run(async () =>
        {
            var from = currentSegmentPointer;
            var to = currentSegmentPointer + currentChunkSize - 1;
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Range = new RangeHeaderValue(from, to);
            using (request)
            {
                // Console.WriteLine($"Current Segment: {currentSegmentPointer}, Current size: {currentChunkSize}, Total lenght: {_length}");
                // Download Stream
                var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                if (response.IsSuccessStatusCode)
                    response.EnsureSuccessStatusCode();
                var stream = await response.Content.ReadAsStreamAsync();
                var buffer = new byte[181920]; // TODO Test
                int bytesCopied;
                int totalBytesCopied = 0;
                do
                {
                    bytesCopied = await stream.ReadAsync(_array, (int)currentSegmentPointer + totalBytesCopied,
                        (int)currentChunkSize - totalBytesCopied, _cancellationToken);
                    totalBytesCopied += bytesCopied;
                    if (_chuckMap.TryGetValue(counter, out var map))
                    {
                        map.DownloadedBytesCount = totalBytesCopied;
                    }
                } while (bytesCopied > 0 && !_cancellationToken.IsCancellationRequested);
            }
        });
    }
}

class MapElement
{
    public long SegmentPointer;
    public long ChunkSize;
    public int DownloadedBytesCount;
}