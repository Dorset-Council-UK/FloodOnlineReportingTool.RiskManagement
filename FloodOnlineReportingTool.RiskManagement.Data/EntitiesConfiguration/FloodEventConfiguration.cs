using FloodOnlineReportingTool.RiskManagement.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FloodOnlineReportingTool.RiskManagement.Data.EntitiesConfiguration
{
    internal class FloodEventConfiguration : IEntityTypeConfiguration<FloodEvent>
    {
        public void Configure(EntityTypeBuilder<FloodEvent> builder)
        {
            builder
                .Property(o => o.FloodStartDate)
                .IsRequired();

            builder
                .Property(o => o.FloodTitle)
                .IsRequired()
                .HasMaxLength(200);

            builder.Navigation(o => o.ResponsibleAuthorites)
                .AutoInclude();
        }
    }
}
