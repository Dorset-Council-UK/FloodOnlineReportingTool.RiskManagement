using FloodOnlineReportingTool.Contracts.Shared.Search;
using FloodOnlineReportingTool.RiskManagement.Models.Export;

namespace FloodOnlineReportingTool.RiskManagement.Extensions;
internal static class SearchResultsViewModelExtensions
{
    public static FloodReportExportDto? ToExportDto(this SearchResultFloodReport viewModel, string reportStatus)
    {
        var firstSource = viewModel?.SourceRecords?.FirstOrDefault();
        if (firstSource is null)
        {
            // Invalid record
            return null;
        }

        return new FloodReportExportDto
        {
            Reference = viewModel?.Reference ?? string.Empty,
            ReportedOn = firstSource.CreatedUtc.GdsReadable(),
            RecordStatus = reportStatus,
            Easting = Math.Truncate(firstSource.Easting),
            Northing = Math.Truncate(firstSource.Northing),
            Y = firstSource.Latitude,
            X = firstSource.Longitude,
        };
    }
}
