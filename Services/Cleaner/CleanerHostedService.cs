using DSentBot.Models;
using DSentBot.Services.MusicPlayerServices;
using Microsoft.EntityFrameworkCore;

namespace DSentBot.Services.Cleaner;

public class CleanerHostedService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    CancellationToken _cancellationToken;

    public CleanerHostedService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task CleanAsync()
    {
        using (var provider = _serviceProvider.CreateScope())
        {
            var dbContext = provider.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var musics = await dbContext.Musics
                .Where(m => m.IsDownloaded)
                .Select(m => new
                {
                    Music = m,
                    Rating = (double)m.RequestsNumber / (m.DurationMin+1) / ((DateTime.Now - m.CreatedAt).Days + 1)
                })
                .OrderBy(m => m.Rating)
                .Take(3)
                .Select(m => m.Music).ToListAsync();

            // используем отсортированные записи
            // await foreach (var music in musics)
            // {
            //     Console.WriteLine($"Name: {music.Name}, RequestsNumber: {music.RequestsNumber}, DurationMin: {music.DurationMin} \n" +
            //                       $"Rating: {((double)music.RequestsNumber / (music.DurationMin+1) / ((DateTime.Now - music.CreatedAt).Days + 1)).ToString()}");
            // }

            DirectoryInfo directory = new DirectoryInfo("music");
            // Получение размера директории
            long size = directory.EnumerateFiles("*", SearchOption.AllDirectories).Sum(file => file.Length);
            int sizeMb = (int)(size / 1024 / 1024);
            Console.WriteLine(sizeMb);
            if (sizeMb > 50)
            {
                foreach (var music in musics)
                {
                    //Console.WriteLine($"Removed: {music.Id}) {music.Name}");
                    music.IsDownloaded = false;
                    dbContext.Musics.Update(music);
                    FileInfo musicFile = new FileInfo(music.LocalPath);
                    if(musicFile.Exists) musicFile.Delete();
                }

                await dbContext.SaveChangesAsync();
            }
        }
    }


    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _cancellationToken = cancellationToken;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}