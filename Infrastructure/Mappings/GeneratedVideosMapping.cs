using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VideoGenerator.Entities;

namespace VideoGenerator.Infrastructure.Mappings;

public class GeneratedVideosMapping : IEntityTypeConfiguration<GeneratedVideo>
{
	public void Configure(EntityTypeBuilder<GeneratedVideo> builder)
	{
		builder.ToTable("GeneratedVideos");

		builder.HasKey(x => x.Id);

		builder.Property(x => x.Id)
			.IsRequired();

		builder.Property(x => x.BlobPath)
			.HasMaxLength(200)
			.IsRequired();

		builder.HasOne(x => x.GenerationQueueItem)
			.WithMany()
			.HasForeignKey(x => x.GenerationQueueId);
	}
}
