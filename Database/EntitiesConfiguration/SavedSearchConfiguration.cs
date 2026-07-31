using FloodOnlineReportingTool.Database.Models.SavedSearches;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

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

        // Handle NULL array columns from imported data by converting them to empty collections.
        // This prevents InvalidCastException when EF Core tries to materialize entities with NULL array values.
        // Explicitly allow NULL values from the database while converting them to empty collections in the entity.
        var converter = new ValueConverter<ICollection<Guid>, Guid[]>(
            v => (v ?? Array.Empty<Guid>()).ToArray(),
            v => v == null ? new List<Guid>() : v.ToList());

        builder
            .Property(s => s.FloodSources)
            .HasConversion(converter)
            .IsRequired(false);

        builder
            .Property(s => s.LimitByOrganisation)
            .HasConversion(converter)
            .IsRequired(false);

        builder
            .Property(s => s.LimitByStatus)
            .HasConversion(converter)
            .IsRequired(false);

        builder
            .Property(s => s.SortBy)
            .IsRequired(false)
            .HasDefaultValue(string.Empty);
    }
}
