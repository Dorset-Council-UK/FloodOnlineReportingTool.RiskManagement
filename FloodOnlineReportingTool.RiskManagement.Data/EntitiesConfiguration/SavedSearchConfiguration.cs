using FloodOnlineReportingTool.Database.Models.SavedSearches;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FloodOnlineReportingTool.Database.EntitiesConfiguration;

internal class SavedSearchConfiguration : IEntityTypeConfiguration<SavedSearch>
{
    public void Configure(EntityTypeBuilder<SavedSearch> builder)
    {
        builder
            .HasKey(s => s.Id);

        builder
            .Property(s => s.Id)
            .ValueGeneratedNever();

        builder
            .Property(s => s.CreatedAt)
            .HasDefaultValue(new DateTimeOffset(2026, 6, 10, 0, 0, 0, TimeSpan.Zero));

        builder
            .Property(s => s.Geom)
            .HasColumnType("geometry(multipolygon, 27700)");

        builder
            .HasIndex(s => s.Reference)
            .IsUnique();
        builder
            .Property(s => s.Reference)
            .IsRequired()
            .HasMaxLength(15);

        // Store enums as integers in the database
        builder
            .Property(s => s.ShowResultsAs)
            .HasConversion<int>();

        builder
            .Property(s => s.SortOrder)
            .HasConversion<int>();
    }
}
