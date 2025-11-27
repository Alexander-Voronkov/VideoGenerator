using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VideoGenerator.Entities;

namespace VideoGenerator.Infrastructure.Mappings;

public class InterestingFactQueueItemMapping : IEntityTypeConfiguration<InterestingFactQueueItem>
{
	public void Configure(EntityTypeBuilder<InterestingFactQueueItem> builder)
	{
	}
}
