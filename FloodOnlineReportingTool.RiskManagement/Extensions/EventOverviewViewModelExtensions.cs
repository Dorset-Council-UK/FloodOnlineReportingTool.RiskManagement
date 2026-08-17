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
            FloodingStart = viewModel?.FloodingStart ?? DateTimeOffset.MinValue,
            FloodingEnd = viewModel?.FloodingEnd ?? DateTimeOffset.MinValue,
            PrimaryFloodCauses = viewModel?.PrimaryFloodCauses ?? [],
            SecondaryFloodCauses = viewModel?.SecondaryFloodCauses ?? [],
            UPRN = viewModel?.UPRN,
            USRN = viewModel?.USRN,
            RecordStatus = viewModel?.RecordStatus ?? string.Empty,
            FloodStatus = viewModel?.FloodStatus ?? string.Empty,
            InvestigationStatus = viewModel?.InvestigationStatus ?? string.Empty,
            EstimatedVulnerable = viewModel?.EstimatedVulnerable ?? 0,
            EstimatedUninhabitable = viewModel?.EstimatedUninhabitable ?? 0,
            EstimatedTotalExternalOnly = viewModel?.EstimatedTotalExternalOnly ?? 0,
            EstimatedTotalInternallyFlooded = viewModel?.EstimatedTotalInternallyFlooded ?? 0,
            EstimatedDisruptedUtilities = viewModel?.EstimatedDisruptedUtilities ?? 0,
            EstimatedResidentialAffected = viewModel?.EstimatedResidentialAffected,
            EstimatedResidentialFlooded = viewModel?.EstimatedResidentialFlooded,
            EstimatedResidentialOther = viewModel?.EstimatedResidentialOther,
            EstimatedCommercialAffected = viewModel?.EstimatedCommercialAffected,
            EstimatedCommercialFlooded = viewModel?.EstimatedCommercialFlooded,
            EstimatedCommercialOther = viewModel?.EstimatedCommercialOther,
            EstimatedNoElectric = viewModel?.EstimatedNoElectric,
            EstimatedNoGas = viewModel?.EstimatedNoGas,
            EstimatedNoFoul = viewModel?.EstimatedNoFoul,
            EstimatedNoWaterSupply = viewModel?.EstimatedNoWaterSupply,
            StatusNote = viewModel?.StatusNote ?? string.Empty,
        };
    }
}
