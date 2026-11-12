using FloodOnlineReportingTool.Database.Models.Media;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FloodOnlineReportingTool.Database.EntitiesConfiguration;

internal class MediaItemConfiguration : IEntityTypeConfiguration<MediaItem>
{
    public void Configure(EntityTypeBuilder<MediaItem> builder)
    {
        builder.Property(m => m.Id).ValueGeneratedNever();
        builder.HasIndex(m => m.PublicMediaSubmissionId);
    }
}
