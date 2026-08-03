using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VideoGenerator.Entities;

namespace VideoGenerator.Infrastructure.Mappings;

public class GenerationQueueItemMapping : IEntityTypeConfiguration<GenerationQueueItem>
{
	public void Configure(EntityTypeBuilder<GenerationQueueItem> builder)
	{
		builder.ToTable("GenerationQueue");

		builder.HasKey(x => x.Id);

		builder.Property(x => x.Id).IsRequired();

		builder.Property(x => x.Title).HasMaxLength(200);
		builder.Property(x => x.Description).HasMaxLength(500);
	}
}
