using FloodOnlineReportingTool.RiskManagement.Models.Event;
using FloodOnlineReportingTool.RiskManagement.Models.Export;

namespace FloodOnlineReportingTool.RiskManagement.Extensions;

internal static class EventOverviewViewModelExtensions
{
    public static FloodEventExportDto? ToExportDto(this EventOverviewViewModel viewModel)
    {
        return new FloodEventExportDto
        {
            Reference = viewModel?.SearchReference ?? string.Empty,
            Title = viewModel?.SearchTitle ?? string.Empty,
            Geom = viewModel?.Geom ?? new NetTopologySuite.Geometries.MultiPolygon(null)
        };
    }
}
