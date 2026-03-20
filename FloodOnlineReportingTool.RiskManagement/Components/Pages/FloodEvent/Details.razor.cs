using FloodOnlineReportingTool.RiskManagement.Data.Models.Lookups;
using FloodOnlineReportingTool.RiskManagement.Data.Repositories;
using FloodOnlineReportingTool.RiskManagement.Models.Event;
using FloodOnlineReportingTool.RiskManagement.Models.Order;
using FloodOnlineReportingTool.RiskManagement.Services;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace FloodOnlineReportingTool.RiskManagement.Components.Pages.FloodEvent;
public partial class Details(
    ILogger<Details> logger,
    NavigationManager navigationManager,
    ICommonRepository commonRepository, 
    ISearchResultsState searchResultsState) : IAsyncDisposable
{
    private EditContext eventContext = default!;
    private readonly CancellationTokenSource _cts = new();
    private EventDetailsViewModel Model { get; set; } = default!;

    [Parameter]
    public string? SearchReference { get; set; }

    private IReadOnlyCollection<GdsOptionItem<bool>> _triggerAction = [
        new("action-yes", "Yes", value: true),
        new("action-no", "No", value: false),
    ];

    [PersistentState(AllowUpdates = true)]
    public IReadOnlyCollection<FloodStatus>? _floodStatuses { get; set; }
    [PersistentState(AllowUpdates = true)]
    public IReadOnlyCollection<EventStatus>? _eventStatuses { get; set; }
    [PersistentState(AllowUpdates = true)]
    public IReadOnlyCollection<InvestigationStatus>? _investigationStatuses { get; set; }

    private bool _isLoading = true;

    protected override async Task OnInitializedAsync()
    {
        // Setup model and edit context
        Model ??= new();
        eventContext = new(Model);
        eventContext.SetFieldCssClassProvider(new GdsFieldCssClassProvider());

        _floodStatuses ??= [.. await commonRepository.GetFloodStatuses(_cts.Token)];
        _eventStatuses ??= [.. await commonRepository.GetEventStatuses(_cts.Token)];
        _investigationStatuses ??= [.. await commonRepository.GetInvestigationStatuses(_cts.Token)];
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

        navigationManager.NavigateTo($"{EventPages.Estimates.Url}/{SearchReference}");

        _isLoading = false;
        StateHasChanged();
        return;
    }
}