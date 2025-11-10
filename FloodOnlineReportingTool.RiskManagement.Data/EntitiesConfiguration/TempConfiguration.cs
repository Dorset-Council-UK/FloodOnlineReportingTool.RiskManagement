using FloodOnlineReportingTool.RiskManagement.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FloodOnlineReportingTool.RiskManagement.Data.EntitiesConfiguration;

internal class TempConfiguration : IEntityTypeConfiguration<Temp>
{
    public void Configure(EntityTypeBuilder<Temp> builder)
    {
        builder
            .Property(o => o.Id)
            .ValueGeneratedNever();
    }
}
