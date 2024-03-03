using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using VideoGenerator.Infrastructure.Entities;

namespace VideoGenerator.Infrastructure;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext()
    {
        
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) 
    {
        Database.Migrate();
    }

    public DbSet<TelegramMessage> Messages { get; set; }
    public DbSet<Attachment> Attachments { get; set; }
    public DbSet<Group> Groups { get; set; }
    public DbSet<Topic> Topics { get; set; }
    public DbSet<Language> Languages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TelegramMessage>(entity =>
        {
            entity.HasOne(x => x.Group)
                .WithMany(x => x.StolenMessages)
                .HasForeignKey(x => x.GroupId);

            entity.HasMany(x => x.Attachments)
                .WithOne()
                .HasForeignKey(x => x.TelegramMessageId);
        });

        modelBuilder.Entity<Group>(entity =>
        {
            entity.HasOne(x => x.Topic)
                .WithMany()
                .HasForeignKey(x => x.TopicId)
                .IsRequired(false);

            entity.Property(x => x.TopicId)
                .IsRequired(false);
        });

        modelBuilder.Entity<Topic>(entity =>
        {
            entity.HasOne(x => x.Language)
                .WithMany(x => x.Topics)
                .HasForeignKey(x => x.LanguageId);
        });
    }

    //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //{
    //    base.OnConfiguring(optionsBuilder);
    //    optionsBuilder.UseSqlite("Data Source=videogenerator.db;");
    //}
}