using FloodOnlineReportingTool.RiskManagement.Components.Pages.FloodReport;
using FloodOnlineReportingTool.RiskManagement.Data.Models.Enums;
using FloodOnlineReportingTool.RiskManagement.Data.Repositories;
using FloodOnlineReportingTool.RiskManagement.Models.Event;
using FloodOnlineReportingTool.RiskManagement.Models.Order;
using FloodOnlineReportingTool.RiskManagement.Services.SavedStates;
using GdsBlazorComponents;
using MassTransit.Contracts;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace FloodOnlineReportingTool.RiskManagement.Components.Pages.FloodEvent;

public partial class Estimates(
    ILogger<Estimates> logger,
    NavigationManager navigationManager,
    IFloodEventState floodEventState,
    ICommonRepository commonRepository) : IAsyncDisposable
{
    private readonly CancellationTokenSource _cts = new();
    private EventEstimatesViewModel Model { get; set; } = default!;
    private EditContext estimatesContext = default!;

    [Parameter]
    public string? SearchReference { get; set; }

    private bool _isLoading = true;

    protected override void OnInitialized()
    {
        // Setup model and edit context
        Model ??= new();
        estimatesContext = new(Model);
        estimatesContext.SetFieldCssClassProvider(new GdsFieldCssClassProvider());

        var estimates = floodEventState.Estimates;
        if (estimates != null && string.Equals(estimates.SearchReference, SearchReference, StringComparison.OrdinalIgnoreCase))
        {
            Model.SearchReference = estimates.SearchReference;
            Model.EstimatedTotalInternallyFlooded = estimates.EstimatedTotalInternallyFlooded;
            Model.EstimatedTotalExternalOnly = estimates.EstimatedTotalExternalOnly;
            Model.EstimatedResidentialAffected = estimates.EstimatedResidentialAffected;
            Model.EstimatedResidentialAffectedString = estimates.EstimatedResidentialAffectedString;
            Model.EstimatedResidentialFlooded = estimates.EstimatedResidentialFlooded;
            Model.EstimatedResidentialFloodedString = estimates.EstimatedResidentialFloodedString;
            Model.EstimatedResidentialOther = estimates.EstimatedResidentialOther;
            Model.EstimatedResidentialOtherString = estimates.EstimatedResidentialOtherString;
            Model.EstimatedCommercialAffected = estimates.EstimatedCommercialAffected;
            Model.EstimatedCommercialAffectedString = estimates.EstimatedCommercialAffectedString;
            Model.EstimatedCommercialFlooded = estimates.EstimatedCommercialFlooded;
            Model.EstimatedCommercialFloodedString = estimates.EstimatedCommercialFloodedString;
            Model.EstimatedCommercialOther = estimates.EstimatedCommercialOther;
            Model.EstimatedCommercialOtherString = estimates.EstimatedCommercialOtherString;
            Model.EstimatedNoElectric = estimates.EstimatedNoElectric;
            Model.EstimatedNoElectricString = estimates.EstimatedNoElectricString;
            Model.EstimatedNoGas = estimates.EstimatedNoGas;
            Model.EstimatedNoGasString = estimates.EstimatedNoGasString;
            Model.EstimatedNoFoul = estimates.EstimatedNoFoul;
            Model.EstimatedNoFoulString = estimates.EstimatedNoFoulString;
            Model.EstimatedNoWaterSupply = estimates.EstimatedNoWaterSupply;
            Model.EstimatedNoWaterSupplyString = estimates.EstimatedNoWaterSupplyString;
            Model.EstimatedDisruptedUtilities = estimates.EstimatedDisruptedUtilities;
            Model.EstimatedDisruptedUtilitiesString = estimates.EstimatedDisruptedUtilitiesString;
            Model.FloodExtentConfidence = estimates.FloodExtentConfidence;
        } else
        {
            // TODO: Check if the options need picking up from the database? 

            // Clear the existing state if the search reference does not match, to avoid displaying stale data
            floodEventState.SetEventEstimates(Model);
        }
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            await _cts.CancelAsync();
            _cts.Dispose();
        }
        catch (OperationCanceledException)
        {
            // Expected during normal cancellation; no action required.
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unexpected error occurred while disposing {ComponentName}.", nameof(Results));
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            if (string.IsNullOrEmpty(SearchReference))
            {
                logger.LogInformation("No search reference provided in query parameters. Unable to load search results.");
                navigationManager.NavigateTo(SearchPages.Options.Url);

                _isLoading = false;
                StateHasChanged();
                return;
            }

            _isLoading = false;
            StateHasChanged();
        }
    }

    private async Task OnValidSubmit()
    {
        Model.SearchReference = SearchReference ?? "";
        floodEventState.SetEventEstimates(Model);
        navigationManager.NavigateTo($"{EventPages.View.Url}/{SearchReference}");

        _isLoading = false;
        StateHasChanged();
        return;
    }
}