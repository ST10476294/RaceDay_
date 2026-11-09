using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
namespace RaceDay.Api.Data;

// EF tooling can inspect the model without starting the HTTP application.
public class RaceDayDbContextFactory : IDesignTimeDbContextFactory<RaceDayDbContext>
{
    public RaceDayDbContext CreateDbContext(string[] args)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Path.GetDirectoryName(typeof(RaceDayDbContext).Assembly.Location)!)
            .AddJsonFile("appsettings.json", optional: true)
            .AddUserSecrets<Program>(optional: true)
            .AddEnvironmentVariables()
            .Build();
        var connection = config.GetConnectionString("RaceDayDB")
            ?? "Server=(localdb)\\MSSQLLocalDB;Database=RaceDayDB;Integrated Security=True;TrustServerCertificate=True";
        return new RaceDayDbContext(new DbContextOptionsBuilder<RaceDayDbContext>().UseSqlServer(connection).Options);
    }
}
