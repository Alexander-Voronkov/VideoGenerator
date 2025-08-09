using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VideoGenerator.Entities;

namespace VideoGenerator.Infrastructure.Mappings;
internal class SourceVideoMapping : IEntityTypeConfiguration<SourceVideo>
{
	public void Configure(EntityTypeBuilder<SourceVideo> builder)
	{
		builder.ToTable("SourceVideos");

		builder.HasKey(x => x.Id);

		builder.HasMany(x => x.ProcessedVideos)
			.WithOne(x => x.SourceVideo)
			.HasForeignKey(x => x.SourceVideoId);
	}
}
