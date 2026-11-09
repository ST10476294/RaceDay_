using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RaceDay.Api.Data;
using RaceDay.Api.Services;
namespace RaceDay.Api.Tests;
// Stands in for Azure so tests never need a real storage account.
public class FakeImageStorage : IImageStorage
{
    public bool IsConfigured { get; set; } = true;
    public List<string> Uploads { get; } = [];
    public Task<string> UploadAsync(IFormFile file, string folder)
    {
        var url = $"https://images.test/{folder}/{file.FileName}";
        Uploads.Add(url);
        return Task.FromResult(url);
    }
}
public class ApiFactory : WebApplicationFactory<Program>
{
    public const string OrganiserCode = "TEST-ORGANISER-CODE";
    public FakeImageStorage Storage { get; } = new();
    private readonly SqliteConnection connection = new("Data Source=:memory:");
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        connection.Open();
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(
            new Dictionary<string, string?> { ["Registration:OrganiserCode"] = OrganiserCode }));
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IImageStorage>();
            services.AddSingleton<IImageStorage>(Storage);
            services.RemoveAll<DbContextOptions<RaceDayDbContext>>();
            services.RemoveAll<RaceDayDbContext>();
            services.AddDbContext<RaceDayDbContext>(o => o.UseSqlite(connection));
        });
    }
    public HttpClient Client()
    {
        var client = CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost"), AllowAutoRedirect = false });
        using var scope = Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<RaceDayDbContext>().Database.EnsureCreated();
        return client;
    }
    public async Task Database(Func<RaceDayDbContext,Task> action)
    {
        using var scope = Services.CreateScope();
        await action(scope.ServiceProvider.GetRequiredService<RaceDayDbContext>());
    }
    protected override void Dispose(bool disposing)
    { base.Dispose(disposing); if (disposing) connection.Dispose(); }
}
