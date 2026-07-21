using FloodOnlineReportingTool.Database.Models.Lookups;
using FloodOnlineReportingTool.Database.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FloodOnlineReportingTool.Database.EntitiesConfiguration;
internal class DownstreamApiLayerConfiguration : IEntityTypeConfiguration<DownstreamApiLayer>
{
    public void Configure(EntityTypeBuilder<DownstreamApiLayer> builder)
    {
        builder.ToTable("Lk_DownstreamApiLayers");
        builder
            .Property(o => o.Id)
            .ValueGeneratedNever();
        builder
            .ToTable(o => o.HasComment("All possible downstream API layers and their user friendly descriptions"));
        builder
            .HasData(InitialData.DownstreamApiLayerData());
    }
}
