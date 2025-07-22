using FloodOnlineReportingTool.RiskManagement.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FloodOnlineReportingTool.RiskManagement.Data.EntitiesConfiguration
{
    internal class FloodEventResponsibleAuthorityConfiguration : IEntityTypeConfiguration<FloodEventResponsibleAuthority>
    {
        public void Configure(EntityTypeBuilder<FloodEventResponsibleAuthority> builder)
        {
            builder.HasKey(x => new {x.FloodEventId, x.ResponsibleAuthorityId});

            builder.Navigation(x => x.ResponsibleAuthority)
                .AutoInclude();
        }
    }
}
