using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VideoGenerator.Entities;

namespace VideoGenerator.Infrastructure.Mappings;
internal class GeneratedSubtitleMapping : IEntityTypeConfiguration<GeneratedSubtitle>
{
	public void Configure(EntityTypeBuilder<GeneratedSubtitle> builder)
	{
		builder.ToTable("GeneratedSubtitle");

		builder.HasKey(x => x.Id);
	}
}
