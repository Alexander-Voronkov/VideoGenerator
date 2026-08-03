using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VideoGenerator.Entities;

namespace VideoGenerator.Infrastructure.Mappings;
public class SplitHistoryMapping : IEntityTypeConfiguration<SplitHistory>
{
	public void Configure(EntityTypeBuilder<SplitHistory> builder)
	{
		builder.ToTable("SplitHistories");
		builder.HasKey(x => x.Id);

		builder.Property(x => x.ParentBlobPath).HasMaxLength(200).IsRequired();
		builder.Property(x => x.BlobPath).HasMaxLength(200).IsRequired();
	}
}
