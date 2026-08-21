using FloodOnlineReportingTool.RiskManagement.Converters;
using FloodOnlineReportingTool.RiskManagement.Models.Report;
using NetTopologySuite.Features;

namespace FloodOnlineReportingTool.RiskManagement.Models.Event;

/// <summary>
/// A class to convert an ReportOverviewViewModel object into a GeoJSON feature for export.
/// </summary>
public record ReportOverviewViewModelToGeoJson : GeoJsonDataCreator<ReportOverviewViewModel>
{
    /// <summary>
    /// Builds the GeoJSON attributes table from the properties of a ReportOverviewViewModel object.
    /// </summary>
    /// <param name="objectToWriteToGeoJson">The ReportOverviewViewModel object to convert.</param>
    protected override AttributesTable CreateAttributesTable(ReportOverviewViewModel objectToWriteToGeoJson)
        => new()
        {
            { "reference", objectToWriteToGeoJson.Reference },
            { "createdAt", objectToWriteToGeoJson.CreatedUtc },
            { "updatedAt", objectToWriteToGeoJson.UpdatedAt },
            { "updatedBy", objectToWriteToGeoJson.UpdatedBy },
            { "locationDescription", objectToWriteToGeoJson.LocationDescription },
            { "numSourceRecords", objectToWriteToGeoJson.SourceRecords.Count },
        };

    /// <summary>
    /// Creates a file name for the GeoJSON export based on the reference and the current date and time.
    /// </summary>
    /// <param name="reference">The reference to include in the file name.</param>
    /// <returns>A string representing the file name for the GeoJSON export.</returns>
    public static string CreateFileName(string reference)
        => $"ReportOverview-{reference}-{DateTime.Now:yyyyMMddHHmmss}.geojson";
}
