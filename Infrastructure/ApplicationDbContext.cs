﻿using Microsoft.EntityFrameworkCore;
using VideoGenerator.Infrastructure.Entities;

namespace VideoGenerator.Infrastructure;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
        Database.Migrate();
    }

    public DbSet<QueueMessage> QueueMessages { get; set; }
    public DbSet<PublishedMessage> PublishedMessages { get; set; }
    public DbSet<Attachment> MessageAttachments { get; set; }
    public DbSet<Group> Groups { get; set; }
    public DbSet<Topic> Topics { get; set; }
    public DbSet<Language> Languages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<QueueMessage>(entity =>
        {
            entity.HasKey(x => new { x.QueueMessageId })
                .HasName("PK_QueueMessageId");

            entity.Property(x => x.QueueMessageId)
                .ValueGeneratedNever();

            entity.Property(x => x.Text)
                .IsRequired(true)
                .HasColumnName("Text");

            entity.ToTable(t => t.HasCheckConstraint("CK_Message_Text", "LENGTH(Text) > 1"));

            entity.HasOne(x => x.TargetGroup)
                .WithMany(x => x.InputQueueMessages)
                .HasForeignKey(x => x.TargetGroupId)
                .IsRequired(true)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(x => x.SourceGroup)
                .WithMany(x => x.OutputQueueMessages)
                .HasForeignKey(x => x.SourceGroupId)
                .IsRequired(true)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasMany(x => x.Attachments)
                .WithOne(x => x.Message)
                .HasForeignKey(x => x.MessageId)
                .IsRequired(true)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(x => new { x.QueueMessageId, x.TargetGroupId, x.SourceGroupId })
                .IsDescending(true, true, true)
                .IsUnique(true)
                .HasDatabaseName("UI_QueueMessageId_GroupId");
        });

        modelBuilder.Entity<PublishedMessage>(entity =>
        {
            entity.HasKey(x => new { x.PublishedMessageId })
                .HasName("PK_PublishedMessageId");

            entity.Property(x => x.PublishedMessageId)
                .ValueGeneratedNever();

            entity.Property(x => x.Text)
                .IsRequired(true)
                .HasColumnName("Text");

            entity.ToTable(t => t.HasCheckConstraint("CK_Message_Text", "LENGTH(Text) > 1"));

            entity.HasOne(x => x.TargetGroup)
                .WithMany(x => x.InputPublishedMessages)
                .HasForeignKey(x => x.TargetGroupId)
                .IsRequired(true)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(x => x.SourceGroup)
                .WithMany(x => x.OutputPublishedMessages)
                .HasForeignKey(x => x.SourceGroupId)
                .IsRequired(true)
                .OnDelete(DeleteBehavior.NoAction);

            entity.HasMany(x => x.Attachments)
                .WithOne(x => x.PublishedMessage)
                .HasForeignKey(x => x.MessageId)
                .IsRequired(true)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(x => new { x.PublishedMessageId, x.TargetGroupId, x.SourceGroupId })
                .IsDescending(true, true, true)
                .IsUnique(true)
                .HasDatabaseName("UI_PublishedMessageId_GroupId");
        });

        modelBuilder.Entity<Group>(entity =>
        {
            entity.HasKey(x => new { x.GroupId })
                .HasName("PK_GroupId");

            entity.Property(x => x.GroupId)
                .ValueGeneratedNever();

            entity.HasOne(x => x.Topic)
                .WithMany(x => x.Groups)
                .HasForeignKey(x => x.TopicId)
                .IsRequired(true);

            entity.HasMany(x => x.InputQueueMessages)
                .WithOne(x => x.TargetGroup)
                .HasForeignKey(x => x.TargetGroupId)
                .IsRequired(true);

            entity.HasMany(x => x.OutputPublishedMessages)
                .WithOne(x => x.SourceGroup)
                .HasForeignKey(x => x.SourceGroupId)
                .IsRequired(true);

            entity.HasMany(x => x.InputPublishedMessages)
                .WithOne(x => x.TargetGroup)
                .HasForeignKey(x => x.TargetGroupId)
                .IsRequired(true);

            entity.HasMany(x => x.OutputQueueMessages)
                .WithOne(x => x.SourceGroup)
                .HasForeignKey(x => x.SourceGroupId)
                .IsRequired(true);

            entity.HasIndex(x => new { x.TopicId, x.GroupId })
                .IsUnique(true)
                .IsDescending(true, true)
                .HasDatabaseName("UI_TopicId_GroupId");
        });

        modelBuilder.Entity<Topic>(entity =>
        {
            entity.HasKey(x => new { x.TopicId })
                .HasName("PK_TopicId");

            entity.Property(x => x.TopicId)
                .ValueGeneratedNever();

            entity.HasOne(x => x.Language)
                .WithMany(x => x.Topics)
                .HasForeignKey(x => x.LanguageId);

            entity.HasIndex(x => new { x.TopicName, x.TopicId, x.LanguageId })
                .IsUnique(true)
                .IsDescending(true, true, true)
                .HasDatabaseName("UI_TopicName_TopicId");
        });

        modelBuilder.Entity<Attachment>(entity =>
        {
            entity.HasKey(x => new { x.AttachmentId })
                .HasName("PK_AttachmentId");

            entity.Property(x => x.AttachmentId)
                .ValueGeneratedNever();

            entity.HasOne(x => x.Message)
                .WithMany(x => x.Attachments)
                .HasForeignKey(x => x.MessageId)
                .IsRequired(true);

            entity.HasIndex(x => new { x.AttachmentId, x.MessageId })
                .IsUnique(true)
                .IsDescending(true, true);
        });

        modelBuilder.Entity<Language>(entity =>
        {
            entity.HasKey(x => new { x.LanguageId })
                .HasName("PK_LanguageId");

            entity.HasMany(x => x.Topics)
                .WithOne(x => x.Language)
                .HasForeignKey(x => x.LanguageId)
                .IsRequired(true);

            entity.HasIndex(x => new { x.LanguageId, x.LanguageCode })
                .IsUnique(true)
                .IsDescending(true, true);
        });
    }
}