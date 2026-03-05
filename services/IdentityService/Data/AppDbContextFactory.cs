using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace IdentityService.Data;

/// <summary>
/// Used only by EF Core design-time tools (migrations). Not used at runtime.
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        // Use a placeholder connection string at design time; actual connection is configured via env vars at runtime.
        optionsBuilder.UseNpgsql(
            "Host=localhost;Database=identity_db;Username=identity_user;Password=identity_pass");

        return new AppDbContext(optionsBuilder.Options);
    }
}
