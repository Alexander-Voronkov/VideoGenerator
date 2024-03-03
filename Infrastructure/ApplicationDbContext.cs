using Microsoft.EntityFrameworkCore;
using VideoGenerator.Infrastructure.Entities;

namespace VideoGenerator.Infrastructure;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Message> Messages { get; set; }
    public DbSet<Group> Groups { get; set; }
    public DbSet<Topic> Topics { get; set; }
    public DbSet<Language> Languages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(x => new { x.MessageId, x.GroupId });
            entity.HasIndex(x => new { x.MessageId, x.GroupId })
                .IsDescending()
                .IsUnique();
            entity.HasOne(x => x.Group)
                .WithMany()
                .HasForeignKey(x => x.GroupId)
                .IsRequired();
        });

        modelBuilder.Entity<Group>(entity =>
        {
            entity.HasKey(x => new { x.GroupId });
            entity.HasIndex(x => new { x.GroupId })
                .IsDescending()
                .IsUnique();
            entity.HasOne(x => x.Topic)
                .WithMany()
                .HasForeignKey(x => x.TopicId)
                .IsRequired();
        });

        modelBuilder.Entity<Topic>(entity =>
        {
            entity.HasKey(x => new { x.TopicId, x.LanguageId });
            entity.HasIndex(x => new { x.TopicId, x.LanguageId })
                .IsDescending()
                .IsUnique();
            entity.HasOne(x => x.Language)
                .WithOne()
                .IsRequired();
            entity.Property(x => x.LanguageId)
                .HasDefaultValue(Guid.NewGuid());
        });

        modelBuilder.Entity<Language>(entity =>
        {
            entity.HasKey(x => new { x.LanguageId, x.LanguageCode });
            entity.HasIndex(x => new { x.LanguageId, x.TopicId, x.LanguageCode })
                .IsDescending()
                .IsUnique();
        });
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=videogenerator.db;");
    }
}