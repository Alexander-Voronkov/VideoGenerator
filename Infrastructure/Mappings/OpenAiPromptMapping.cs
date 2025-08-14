using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using VideoGenerator.Entities;

namespace VideoGenerator.Infrastructure.Mappings;

public class OpenAiPromptMapping : IEntityTypeConfiguration<OpenAiPrompt>
{
	public void Configure(EntityTypeBuilder<OpenAiPrompt> builder)
	{
		builder.ToTable("OpenAiPrompts");

		builder.HasKey(x => x.Id);
	}
}
