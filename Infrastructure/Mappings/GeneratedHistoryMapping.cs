using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VideoGenerator.Entities;

namespace VideoGenerator.Infrastructure.Mappings;
internal class GeneratedHistoryMapping : IEntityTypeConfiguration<GeneratedHistory>
{
	public void Configure(EntityTypeBuilder<GeneratedHistory> builder)
	{
		builder.ToTable("GeneratedHistories");

		builder.HasKey(x => x.Id);

		builder.HasOne(x => x.GeneratedSubtitle)
			.WithOne(x => x.GeneratedHistory)
			.HasForeignKey<GeneratedHistory>(x => x.GeneratedSubtitleId);
	}
}
