using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VideoGenerator.Entities;

namespace VideoGenerator.Infrastructure.Mappings;

public class PublishBatchMapping : IEntityTypeConfiguration<PublishBatch>
{
	public void Configure(EntityTypeBuilder<PublishBatch> builder)
	{
		builder.ToTable("PublishBatches");

		builder.HasKey(x => x.Id);

		builder.Property(x => x.Id)
			.IsRequired()
			.HasMaxLength(50);

		builder.Property(x => x.GenerationQueueItemId)
			.IsRequired()
			.HasMaxLength(50);

		builder.HasOne(x => x.GenerationQueueItem)
			.WithMany()
			.HasForeignKey(x => x.GenerationQueueItemId)
			.OnDelete(DeleteBehavior.Cascade);

		builder.Property(x => x.ApproveStatus)
			.IsRequired();

		builder.Property(x => x.CreatedAt)
			.IsRequired();

		builder.Property(x => x.IsScheduled)
			.IsRequired();

		builder.HasMany(x => x.PublishQueueItems)
			.WithOne(x => x.PublishBatch)
			.HasForeignKey(x => x.PublishBatchId)
			.OnDelete(DeleteBehavior.Cascade);
	}
}
