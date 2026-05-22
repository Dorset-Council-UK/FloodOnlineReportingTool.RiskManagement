using FloodOnlineReportingTool.RiskManagement.Data.Models.Lookups;
using FloodOnlineReportingTool.RiskManagement.Data.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FloodOnlineReportingTool.RiskManagement.Data.EntitiesConfiguration;

internal class ActionDefinitionConfiguration : IEntityTypeConfiguration<ActionDefinition>
{
    public void Configure(EntityTypeBuilder<ActionDefinition> builder)
    {
        builder.ToTable("Lk_ActionDefinitions");
        builder
            .Property(o => o.Id)
            .ValueGeneratedNever();
        builder
            .ToTable(o => o.HasComment("All possible actions and their user friendly descriptions"));
        builder
            .HasData(InitialData.ActionDefinitionData());
    }
}
