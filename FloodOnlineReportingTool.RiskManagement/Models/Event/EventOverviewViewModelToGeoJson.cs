using FloodOnlineReportingTool.RiskManagement.Converters;
using NetTopologySuite.Features;

namespace FloodOnlineReportingTool.RiskManagement.Models.Event;

/// <summary>
/// A class to convert an EventOverviewViewModel object into a GeoJSON feature for export.
/// </summary>
public record EventOverviewViewModelToGeoJson : GeoJsonDataCreator<EventOverviewViewModel>
{
    /// <summary>
    /// Builds the GeoJSON attributes table from the properties of an EventOverviewViewModel object.
    /// </summary>
    /// <param name="objectToWriteToGeoJson">The EventOverviewViewModel object to convert.</param>
    protected override AttributesTable CreateAttributesTable(EventOverviewViewModel objectToWriteToGeoJson)
        => new()
        {
            { "reference", objectToWriteToGeoJson.SearchReference },
            { "title", objectToWriteToGeoJson.SearchTitle },
            { "floodingStart", objectToWriteToGeoJson.FloodingStart },
            { "floodingEnd", objectToWriteToGeoJson.FloodingEnd },
            { "primaryFloodCauses", objectToWriteToGeoJson.PrimaryFloodCauses },
            { "secondaryFloodCauses", objectToWriteToGeoJson.SecondaryFloodCauses },
            { "responsibleOrganisations", objectToWriteToGeoJson.ResponsibleOrganisations },
            { "uprn", objectToWriteToGeoJson.UPRN },
            { "usrn", objectToWriteToGeoJson.USRN },
            { "recordStatus", objectToWriteToGeoJson.RecordStatus },
            { "floodStatus", objectToWriteToGeoJson.FloodStatus },
            { "investigationStatus", objectToWriteToGeoJson.InvestigationStatus },
            { "estimatedVulnerable", objectToWriteToGeoJson.EstimatedVulnerable },
            { "estimatedUninhabitable", objectToWriteToGeoJson.EstimatedUninhabitable },
            { "estimatedTotalExternalOnly", objectToWriteToGeoJson.EstimatedTotalExternalOnly },
            { "estimatedTotalInternallyFlooded", objectToWriteToGeoJson.EstimatedTotalInternallyFlooded },
            { "estimatedDisruptedUtilities", objectToWriteToGeoJson.EstimatedDisruptedUtilities },
            { "estimatedResidentialAffected", objectToWriteToGeoJson.EstimatedResidentialAffected },
            { "estimatedResidentialFlooded", objectToWriteToGeoJson.EstimatedResidentialFlooded },
            { "estimatedResidentialOther", objectToWriteToGeoJson.EstimatedResidentialOther },
            { "estimatedCommercialAffected", objectToWriteToGeoJson.EstimatedCommercialAffected },
            { "estimatedCommercialFlooded", objectToWriteToGeoJson.EstimatedCommercialFlooded },
            { "estimatedCommercialOther", objectToWriteToGeoJson.EstimatedCommercialOther },
            { "estimatedNoElectric", objectToWriteToGeoJson.EstimatedNoElectric },
            { "estimatedNoGas", objectToWriteToGeoJson.EstimatedNoGas },
            { "estimatedNoFoul", objectToWriteToGeoJson.EstimatedNoFoul },
            { "estimatedNoWaterSupply", objectToWriteToGeoJson.EstimatedNoWaterSupply },
            { "statusNote", objectToWriteToGeoJson.StatusNote },
        };

    /// <summary>
    /// Creates a file name for the GeoJSON export based on the reference and the current date and time.
    /// </summary>
    /// <param name="reference">The reference to include in the file name.</param>
    /// <returns>A string representing the file name for the GeoJSON export.</returns>
    public static string CreateFileName(string reference)
        => $"EventOverview-{reference}-{DateTime.Now:yyyyMMddHHmmss}.geojson";
}
