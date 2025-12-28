using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace ArchivSoftware.Infrastructure.Data;

/// <summary>
/// Factory für die Design-Time Erstellung des DbContext (für Migrations).
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ArchivSoftwareDbContext>
{
    public ArchivSoftwareDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../ArchivSoftware.Ui"))
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        var connectionString = configuration.GetConnectionString("ArchivSoftwareDb");

        var optionsBuilder = new DbContextOptionsBuilder<ArchivSoftwareDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new ArchivSoftwareDbContext(optionsBuilder.Options);
    }
}
