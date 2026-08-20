using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Simble.Data.Entities;

namespace Simble.Data.Configuration;

public sealed class SampleConfiguration : IEntityTypeConfiguration<Sample>
{
    public void Configure(EntityTypeBuilder<Sample> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.ToTable("samples");
        builder.HasKey(sample => sample.Id);

        builder.Property(sample => sample.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(sample => sample.Description)
            .HasMaxLength(500);

        builder.HasIndex(sample => sample.Name)
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");
    }
}
