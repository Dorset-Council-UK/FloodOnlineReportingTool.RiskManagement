using FloodOnlineReportingTool.Contracts.Shared;
using FloodOnlineReportingTool.RiskManagement.Data.Models.Enums;
using FloodOnlineReportingTool.RiskManagement.Data.Models.Helpers;
using FloodOnlineReportingTool.RiskManagement.Data.Models.Lookups;
using FloodOnlineReportingTool.RiskManagement.Data.Repositories;
using FloodOnlineReportingTool.RiskManagement.Models.Event;
using FloodOnlineReportingTool.RiskManagement.Models.Order;
using FloodOnlineReportingTool.RiskManagement.Models.Search;
using FloodOnlineReportingTool.RiskManagement.Services.SavedStates;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System.Globalization;

namespace FloodOnlineReportingTool.RiskManagement.Components.Pages.FloodEvent;
public partial class Details(
    ILogger<Details> logger,
    NavigationManager navigationManager,
    ICommonRepository commonRepository, 
    IFloodEventState floodEventState,
    ISearchResultsState searchResultsState) : IAsyncDisposable
{
    private EditContext eventContext = default!;
    private readonly CancellationTokenSource _cts = new();
    private EventDetailsViewModel Model { get; set; } = default!;
    private SearchResultsViewModel? SearchResults;

    [Parameter]
    public string? SearchReference { get; set; }

    private IReadOnlyCollection<GdsOptionItem<Guid>> _floodStatuses = [];
    private IReadOnlyCollection<GdsOptionItem<Guid>> _eventStatuses = [];
    private IReadOnlyCollection<GdsOptionItem<Guid>> _investigationStatuses = [];

    private bool _isLoading = true;

    protected override async Task OnInitializedAsync()
    {
        // Setup model and edit context
        Model ??= new();
        eventContext = new(Model);
        eventContext.SetFieldCssClassProvider(new GdsFieldCssClassProvider());

        if (searchResultsState.HasOptions)
        {
            SearchResults = searchResultsState.Options;
        }

        var details = floodEventState.Details;
        if (details != null && string.Equals(details.SearchReference, SearchReference, StringComparison.OrdinalIgnoreCase))
        {
            Model.SearchReference = details.SearchReference;
            Model.SearchTitle = details.SearchTitle;
            Model.RecordStatus = details.RecordStatus;
            Model.FloodStatus = details.FloodStatus;
            Model.InvestigationStatus = details.InvestigationStatus;
            Model.EstimatedVulnerable = details.EstimatedVulnerable;
            Model.EstimatedVulnerableString = details.EstimatedVulnerableString;
            Model.EstimatedUninhabitable = details.EstimatedUninhabitable;
            Model.EstimatedUninhabitableString = details.EstimatedUninhabitableString;
            Model.EstimatedTotalExternalOnly = details.EstimatedTotalExternalOnly;
            Model.EstimatedTotalExternalOnlyString = details.EstimatedTotalExternalOnlyString;
            Model.EstimatedTotalInternallyFlooded = details.EstimatedTotalInternallyFlooded;
            Model.EstimatedTotalInternallyFloodedString = details.EstimatedTotalInternallyFloodedString;
            Model.StatusNote = details.StatusNote;
        } else
        {
            // TODO: Check if the options need picking up from the database? 

            // Clear the existing state if the search reference does not match, to avoid displaying stale data
            floodEventState.SetEventDetails(Model);
        }

        var floodStatuses = await commonRepository.GetFloodStatuses(_cts.Token);
        _floodStatuses = CreateGdsOptionItems(
            floodStatuses.Select(x => new IdTextPair { Id = x.Id, Text = x.Text })
        );

        var eventStatuses = await commonRepository.GetEventStatuses(_cts.Token);
        _eventStatuses = CreateGdsOptionItems(
            eventStatuses.Select(x => new IdTextPair { Id = x.Id, Text = x.Text })
        );

        var investigationStatuses = await commonRepository.GetInvestigationStatuses(_cts.Token);
        _investigationStatuses = CreateGdsOptionItems(
            investigationStatuses.Select(x => new IdTextPair { Id = x.Id, Text = x.Text })
        );
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
        floodEventState.SetEventDetails(Model);
        navigationManager.NavigateTo($"{EventPages.Estimates.Url}/{SearchReference}");

        _isLoading = false;
        StateHasChanged();
        return;
    }

    private static List<GdsOptionItem<Guid>> CreateGdsOptionItems(IEnumerable<IdTextPair> results)
    {
        var optionItems = new List<GdsOptionItem<Guid>>
        {
            new("0".AsSpan(), "Select an option", Guid.Empty, false)
        };
        optionItems.AddRange(results.Select((o, idx) =>
            new GdsOptionItem<Guid>((idx + 1).ToString(CultureInfo.InvariantCulture).AsSpan(), o.Text, o.Id, false)));
        return optionItems;
    }

    private async Task OnEventStatusChange(GdsOptionItem<Guid> option)
    {
        // Do something when the select value changes
        Model.RecordStatus = option.Value;
        StateHasChanged();
    }

    private async Task OnFloodStatusChange(GdsOptionItem<Guid> option)
    {
        // Do something when the select value changes
        Model.FloodStatus = option.Value;
        StateHasChanged();
    }

    private async Task OnInvestigationStatusChange(GdsOptionItem<Guid> option)
    {
        // Do something when the select value changes
        Model.InvestigationStatus = option.Value;
        StateHasChanged();
    }
}