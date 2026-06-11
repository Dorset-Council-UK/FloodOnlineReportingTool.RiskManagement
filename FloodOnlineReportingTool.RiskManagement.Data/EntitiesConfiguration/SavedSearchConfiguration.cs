using FloodOnlineReportingTool.RiskManagement.Data.Models.SavedSearches;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FloodOnlineReportingTool.RiskManagement.Data.EntitiesConfiguration;

internal class SavedSearchConfiguration : IEntityTypeConfiguration<SavedSearch>
{
    public void Configure(EntityTypeBuilder<SavedSearch> builder)
    {
        builder
            .HasKey(s => s.Id);

        builder.Property(e => e.Geom)
        builder
            .Property(s => s.CreatedAt)
            .HasDefaultValue(new DateTimeOffset(2026, 6, 10, 0, 0, 0, TimeSpan.Zero));

        builder
            .Property(s => s.Geom)
            .HasColumnType("geometry(multipolygon, 27700)");

        // Store enums as integers in the database
        builder.Property(e => e.ShowResultsAs)
                .HasConversion<int>();
        builder.Property(e => e.SortOrder)
                .HasConversion<int>();
    }
}
