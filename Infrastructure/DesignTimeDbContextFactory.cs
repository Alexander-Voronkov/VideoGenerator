using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace VideoGenerator.Infrastructure;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseNpgsql("User ID=user@wilby.com;Password=secretsecret;Host=postgres.xoxol.xyz;Port=5432;Database=brainrotdb;Pooling=true;Connection Lifetime=180;");

        return new ApplicationDbContext(optionsBuilder.Options); 
    }
}