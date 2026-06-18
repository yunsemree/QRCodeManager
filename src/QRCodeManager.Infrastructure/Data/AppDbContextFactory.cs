using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using QRCodeManager.Application.Constants;

namespace QRCodeManager.Infrastructure.Data;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        var dbPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            ApplicationConstants.ApplicationName,
            ApplicationConstants.DatabaseDirectoryName,
            ApplicationConstants.DatabaseFileName);
        Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);
        optionsBuilder.UseSqlite($"Data Source={dbPath}");
        return new AppDbContext(optionsBuilder.Options);
    }
}
