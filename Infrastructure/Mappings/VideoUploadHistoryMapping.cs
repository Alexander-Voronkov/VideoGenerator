using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VideoGenerator.Entities;

namespace VideoGenerator.Infrastructure.Mappings;

public class VideoUploadHistoryMapping : IEntityTypeConfiguration<VideoUploadHistory>
{
	public void Configure(EntityTypeBuilder<VideoUploadHistory> builder)
	{
		builder.ToTable("VideoUploadHistories");

		builder.HasKey(vuh => vuh.Id);

		builder.Property(vuh => vuh.Id)
			.IsRequired()
			.HasMaxLength(50);

		builder.Property(vuh => vuh.GeneratedVideoId)
			.IsRequired()
			.HasMaxLength(50);

		builder.HasOne(vuh => vuh.GeneratedVideo)
			.WithMany()
			.HasForeignKey(vuh => vuh.GeneratedVideoId)
			.OnDelete(DeleteBehavior.Cascade);

		builder.Property(vuh => vuh.Metadata)
			.IsRequired(false);

		builder.Property(vuh => vuh.UploadType)
			.IsRequired();

		builder.Property(vuh => vuh.UploadedAt)
			.IsRequired();

		builder.HasIndex(vuh => new { vuh.GeneratedVideoId, vuh.UploadType })
			.IsUnique();
	}
}
