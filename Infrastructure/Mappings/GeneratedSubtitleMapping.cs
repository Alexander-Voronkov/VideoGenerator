using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VideoGenerator.Entities;

namespace VideoGenerator.Infrastructure.Mappings;
internal class GeneratedSubtitleMapping : IEntityTypeConfiguration<GeneratedSubtitle>
{
	public void Configure(EntityTypeBuilder<GeneratedSubtitle> builder)
	{
		builder.ToTable("GeneratedSubtitle");

		builder.HasKey(x => x.Id);

		builder.HasOne(x => x.GeneratedHistory)
			.WithOne(x => x.GeneratedSubtitle)
			.HasForeignKey<GeneratedSubtitle>(x => x.GeneratedHistoryId);

		builder.HasOne(x => x.ProcessedVideo)
			.WithOne(x => x.GeneratedSubtitle)
			.HasForeignKey<GeneratedSubtitle>(x => x.ProcessedVideoId);
	}
}
