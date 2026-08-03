using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VideoGenerator.Entities;

namespace VideoGenerator.Infrastructure.Mappings;

public class PublishAccountMapping : IEntityTypeConfiguration<PublishAccount>
{
	public void Configure(EntityTypeBuilder<PublishAccount> builder)
	{
		builder.ToTable("PublishAccounts");

		builder.HasKey(x => x.Id);

		builder.Property(x => x.Id)
			.IsRequired()
			.HasMaxLength(50);

		builder.Property(x => x.Name)
			.IsRequired()
			.HasMaxLength(100);

		builder.Property(x => x.Type)
			.IsRequired();

		builder.Property(x => x.LastPublishAt)
			.IsRequired();

		builder.HasMany(x => x.PublishQueueItems)
			.WithOne(x => x.PublishAccount)
			.HasForeignKey(x => x.PublishAccountId)
			.OnDelete(DeleteBehavior.Restrict);
	}
}
