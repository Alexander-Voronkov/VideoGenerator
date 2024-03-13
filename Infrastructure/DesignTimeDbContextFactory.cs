using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace VideoGenerator.Infrastructure;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseNpgsql("User ID=user;Password=secret;Host=localhost;Port=5432;Database=StolenDataDB;Pooling=true;Connection Lifetime=0;");

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}