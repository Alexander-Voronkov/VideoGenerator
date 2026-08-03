using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VideoGenerator.Entities;

namespace VideoGenerator.Infrastructure.Mappings;

public class ScheduledUploadMapping : IEntityTypeConfiguration<ScheduledUpload>
{
	public void Configure(EntityTypeBuilder<ScheduledUpload> builder)
	{
		builder.ToTable("ScheduledUploads");

		builder.HasKey(x => x.Id);

		builder.Property(x => x.Id)
			.IsRequired()
			.HasMaxLength(50);

		builder.Property(x => x.PublishQueueItemId)
			.IsRequired()
			.HasMaxLength(50);

		builder.HasOne(x => x.PublishQueueItem)
			.WithMany(x => x.ScheduledUploads)
			.HasForeignKey(x => x.PublishQueueItemId)
			.OnDelete(DeleteBehavior.Cascade);

		builder.Property(x => x.GeneratedVideoId)
			.IsRequired()
			.HasMaxLength(50);

		builder.HasOne(x => x.GeneratedVideo)
			.WithMany()
			.HasForeignKey(x => x.GeneratedVideoId)
			.OnDelete(DeleteBehavior.Restrict);

		builder.Property(x => x.ScheduledAt)
			.IsRequired();

		builder.Property(x => x.Status)
			.IsRequired();

		builder.HasIndex(x => new { x.ScheduledAt, x.Status });

		builder.HasIndex(x => new { x.PublishQueueItemId, x.GeneratedVideoId })
			.IsUnique();
	}
}
