using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VideoGenerator.Entities;

namespace VideoGenerator.Infrastructure.Mappings;
internal class ProcessedVideoMapping : IEntityTypeConfiguration<ProcessedVideo>
{
	public void Configure(EntityTypeBuilder<ProcessedVideo> builder)
	{
		builder.ToTable("ProcessedVideos");

		builder.HasKey(x => x.Id);

		builder.HasOne(x => x.SourceVideo)
			.WithMany(x => x.ProcessedVideos)
			.HasForeignKey(x => x.SourceVideoId);

		builder.HasOne(x => x.GeneratedSubtitle)
			.WithOne(x => x.ProcessedVideo)
			.HasForeignKey<ProcessedVideo>(x => x.GeneratedSubtitleId);
	}
}
