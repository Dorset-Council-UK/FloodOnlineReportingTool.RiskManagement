using FloodOnlineReportingTool.RiskManagement.Data.Models.FloodEvents;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FloodOnlineReportingTool.RiskManagement.Data.EntitiesConfiguration;

internal class FloodEventConfiguration : IEntityTypeConfiguration<FloodEvent>
{
    public void Configure(EntityTypeBuilder<FloodEvent> builder)
    {
        // Store enums as integers in the database
        builder.Property(e => e.MitigationTaken)
                .HasConversion<int>();
        builder.Property(e => e.FloodExtentConfidence)
                .HasConversion<int>();
    }
}
