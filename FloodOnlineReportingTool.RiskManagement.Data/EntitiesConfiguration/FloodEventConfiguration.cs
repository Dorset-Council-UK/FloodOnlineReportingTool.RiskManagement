using FloodOnlineReportingTool.Database.Models.FloodEvents;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FloodOnlineReportingTool.Database.EntitiesConfiguration;

internal sealed class FloodEventConfiguration : IEntityTypeConfiguration<FloodEvent>
{
    public void Configure(EntityTypeBuilder<FloodEvent> builder)
    {
        builder.HasKey(f => f.Id);

        builder.Property(f => f.Id)
            .ValueGeneratedNever();

        builder.HasOne(f => f.SavedSearch)
            .WithOne(s => s.FloodEvent)
            .HasForeignKey<FloodEvent>(f => f.SavedSearchId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(f => f.SavedSearch).AutoInclude();

        builder.HasIndex(f => f.SavedSearchId)
            .IsUnique();

        // Store enums as integers in the database
        builder
            .Property(fe => fe.MitigationTaken)
            .HasConversion<int>();

        builder
            .Property(fe => fe.FloodExtentConfidence)
            .HasConversion<int>();
    }
}
