using CsvHelper;
using FloodOnlineReportingTool.RiskManagement.Converters;
using FloodOnlineReportingTool.RiskManagement.Models.Report;

namespace FloodOnlineReportingTool.RiskManagement.Models.Event;

/// <summary>
/// A class to convert an ReportOverviewViewModel object into CSV format for export.
/// </summary>
public record ReportOverviewViewModelToCSV : CSVDataCreator<ReportOverviewViewModel>
{
    /// <summary>
    /// Initialises a new instance of the ReportOverviewViewModelToCSV class with predefined headers for the CSV file.
    /// </summary>
    public ReportOverviewViewModelToCSV()
        : base([
            "Reference", "CreatedAt", "UpdatedAt", "UpdatedBy", "LocationDescription", "NumSourceRecords",
        ]) { }

    /// <summary>
    /// Writes the properties of an EventOverviewViewModel object to a CSV file using the provided CsvWriter.
    /// </summary>
    /// <param name="objectToWriteToCSV">The ReportOverviewViewModel object to write to the CSV file.</param>
    /// <param name="csvWriter">The CsvWriter instance to write the object to.</param>
    protected override void WriteObjectToMemoryStream(ReportOverviewViewModel objectToWriteToCSV, CsvWriter csvWriter)
    {        
        csvWriter.WriteField(objectToWriteToCSV.Reference ?? string.Empty);
        csvWriter.WriteField(objectToWriteToCSV.CreatedUtc);
        csvWriter.WriteField(objectToWriteToCSV.UpdatedAt);
        csvWriter.WriteField(objectToWriteToCSV.UpdatedBy ?? string.Empty);
        csvWriter.WriteField(objectToWriteToCSV.LocationDescription ?? string.Empty);
        csvWriter.WriteField(objectToWriteToCSV.SourceRecords.Count);
    }

    /// <summary>
    /// Creates a file name for the CSV export based on the reference and the current date and time.
    /// </summary>
    /// <param name="reference">The reference to include in the file name.</param>
    /// <returns>A string representing the file name for the CSV export.</returns>
    public static string CreateFileName(string reference)
        => $"ReportOverview-{reference}-{DateTime.Now:yyyyMMddHHmmss}.csv";
}
