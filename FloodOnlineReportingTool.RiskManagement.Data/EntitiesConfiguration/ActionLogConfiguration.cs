using FloodOnlineReportingTool.RiskManagement.Data.Models.Actions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FloodOnlineReportingTool.RiskManagement.Data.EntitiesConfiguration;

internal class ActionLogConfiguration : IEntityTypeConfiguration<ActionLog>
{
    public void Configure(EntityTypeBuilder<ActionLog> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.PointGeom)
            .HasColumnType("geometry(point, 27700)");

        builder.Property(e => e.PolygonGeom)
            .HasColumnType("geometry(multipolygon, 27700)");
    }
}
