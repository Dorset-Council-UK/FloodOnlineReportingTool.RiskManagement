using CsvHelper;
using FloodOnlineReportingTool.RiskManagement.Converters;

namespace FloodOnlineReportingTool.RiskManagement.Models.Event;

/// <summary>
/// A class to convert an EventOverviewViewModel object into CSV format for export.
/// </summary>
public record EventOverviewViewModelToCSV : CSVDataCreator<EventOverviewViewModel>
{
    /// <summary>
    /// Initialises a new instance of the EventOverviewViewModelToCSV class with predefined headers for the CSV file.
    /// </summary>
    public EventOverviewViewModelToCSV()
        : base([
            "Reference", "Title", "FloodingStart", "FloodingEnd", "PrimaryFloodCauses", "SecondaryFloodCauses",
            "ResponsibleOrganisations", "UPRN", "USRN", "RecordStatus", "FloodStatus", "InvestigationStatus",
            "EstimatedVulnerable", "EstimatedUninhabitable", "EstimatedTotalExternalOnly",
            "EstimatedTotalInternallyFlooded", "EstimatedDisruptedUtilities", "EstimatedResidentialAffected",
            "EstimatedResidentialFlooded", "EstimatedResidentialOther", "EstimatedCommercialAffected",
            "EstimatedCommercialFlooded", "EstimatedCommercialOther", "EstimatedNoElectric", "EstimatedNoGas",
            "EstimatedNoFoul", "EstimatedNoWaterSupply", "StatusNote",
        ]) { }

    /// <summary>
    /// Writes the properties of an EventOverviewViewModel object to a CSV file using the provided CsvWriter.
    /// </summary>
    /// <param name="objectToWriteToCSV">The EventOverviewViewModel object to write to the CSV file.</param>
    /// <param name="csvWriter">The CsvWriter instance to write the object to.</param>
    protected override void WriteObjectToMemoryStream(EventOverviewViewModel objectToWriteToCSV, CsvWriter csvWriter)
    {        
        csvWriter.WriteField(objectToWriteToCSV.SearchReference ?? string.Empty);
        csvWriter.WriteField(objectToWriteToCSV.SearchTitle ?? string.Empty);
        csvWriter.WriteField(objectToWriteToCSV.FloodingStart);
        csvWriter.WriteField(objectToWriteToCSV.FloodingEnd);
        csvWriter.WriteField(StringListToSingleValue(objectToWriteToCSV.PrimaryFloodCauses));
        csvWriter.WriteField(StringListToSingleValue(objectToWriteToCSV.SecondaryFloodCauses));
        csvWriter.WriteField(StringListToSingleValue(objectToWriteToCSV.ResponsibleOrganisations));
        csvWriter.WriteField(NullableNumberToString(objectToWriteToCSV.UPRN));
        csvWriter.WriteField(NullableNumberToString(objectToWriteToCSV.USRN));
        csvWriter.WriteField(objectToWriteToCSV.RecordStatus ?? string.Empty);
        csvWriter.WriteField(objectToWriteToCSV.FloodStatus);
        csvWriter.WriteField(objectToWriteToCSV.InvestigationStatus);
        csvWriter.WriteField(objectToWriteToCSV.EstimatedVulnerable);
        csvWriter.WriteField(objectToWriteToCSV.EstimatedUninhabitable);
        csvWriter.WriteField(objectToWriteToCSV.EstimatedTotalExternalOnly);
        csvWriter.WriteField(objectToWriteToCSV.EstimatedTotalInternallyFlooded);
        csvWriter.WriteField(objectToWriteToCSV.EstimatedDisruptedUtilities);
        csvWriter.WriteField(NullableNumberToString(objectToWriteToCSV.EstimatedResidentialAffected));
        csvWriter.WriteField(NullableNumberToString(objectToWriteToCSV.EstimatedResidentialFlooded));
        csvWriter.WriteField(NullableNumberToString(objectToWriteToCSV.EstimatedResidentialOther));
        csvWriter.WriteField(NullableNumberToString(objectToWriteToCSV.EstimatedCommercialAffected));
        csvWriter.WriteField(NullableNumberToString(objectToWriteToCSV.EstimatedCommercialFlooded));
        csvWriter.WriteField(NullableNumberToString(objectToWriteToCSV.EstimatedCommercialOther));
        csvWriter.WriteField(NullableNumberToString(objectToWriteToCSV.EstimatedNoElectric));
        csvWriter.WriteField(NullableNumberToString(objectToWriteToCSV.EstimatedNoGas));
        csvWriter.WriteField(NullableNumberToString(objectToWriteToCSV.EstimatedNoFoul));
        csvWriter.WriteField(NullableNumberToString(objectToWriteToCSV.EstimatedNoWaterSupply));
        csvWriter.WriteField(objectToWriteToCSV.StatusNote ?? string.Empty);
    }

    /// <summary>
    /// Creates a file name for the CSV export based on the reference and the current date and time.
    /// </summary>
    /// <param name="reference">The reference to include in the file name.</param>
    /// <returns>A string representing the file name for the CSV export.</returns>
    public static string CreateFileName(string reference)
        => $"EventOverview-{reference}-{DateTime.Now:yyyyMMddHHmmss}.csv";
}
