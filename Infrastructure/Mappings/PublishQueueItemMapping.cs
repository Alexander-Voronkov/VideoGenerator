using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VideoGenerator.Entities;

namespace VideoGenerator.Infrastructure.Mappings;

public class PublishQueueItemMapping : IEntityTypeConfiguration<PublishQueueItem>
{
	public void Configure(EntityTypeBuilder<PublishQueueItem> builder)
	{
		builder.ToTable("PublishQueueItems");

		builder.HasKey(x => x.Id);

		builder.Property(x => x.Id)
			.IsRequired()
			.HasMaxLength(50);

		builder.Property(x => x.PublishBatchId)
			.IsRequired()
			.HasMaxLength(50);

		builder.HasOne(x => x.PublishBatch)
			.WithMany(x => x.PublishQueueItems)
			.HasForeignKey(x => x.PublishBatchId)
			.OnDelete(DeleteBehavior.Cascade);

		builder.Property(x => x.PublishAccountId)
			.IsRequired()
			.HasMaxLength(50);

		builder.HasOne(x => x.PublishAccount)
			.WithMany(x => x.PublishQueueItems)
			.HasForeignKey(x => x.PublishAccountId)
			.OnDelete(DeleteBehavior.Restrict);

		builder.Property(x => x.PlaylistId)
			.IsRequired(false)
			.HasMaxLength(100);

		builder.HasMany(x => x.ScheduledUploads)
			.WithOne(x => x.PublishQueueItem)
			.HasForeignKey(x => x.PublishQueueItemId)
			.OnDelete(DeleteBehavior.Cascade);

		builder.HasIndex(x => new { x.PublishBatchId, x.PublishAccountId })
			.IsUnique();
	}
}
