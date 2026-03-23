using FloodOnlineReportingTool.Contracts.Shared;
using FloodOnlineReportingTool.RiskManagement.Data.Models.Helpers;
using FloodOnlineReportingTool.RiskManagement.Data.Models.Lookups;
using FloodOnlineReportingTool.RiskManagement.Data.Repositories;
using FloodOnlineReportingTool.RiskManagement.Models.Event;
using FloodOnlineReportingTool.RiskManagement.Models.Order;
using FloodOnlineReportingTool.RiskManagement.Services;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System.Globalization;

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

    [PersistentState(AllowUpdates = true)]
    public IReadOnlyCollection<FloodStatus>? _floodStatuses { get; set; }
    [PersistentState(AllowUpdates = true)]
    public IReadOnlyCollection<EventStatus>? _eventStatuses { get; set; }
    private IReadOnlyCollection<GdsOptionItem<Guid>> _investigationStatuses = [];
    private Guid _selectedInvestigationStatusId = Guid.Empty;

    private bool _isLoading = true;

    protected override async Task OnInitializedAsync()
    {
        // Setup model and edit context
        Model ??= new();
        eventContext = new(Model);
        eventContext.SetFieldCssClassProvider(new GdsFieldCssClassProvider());

        _floodStatuses ??= [.. await commonRepository.GetFloodStatuses(_cts.Token)];
        _eventStatuses ??= [.. await commonRepository.GetEventStatuses(_cts.Token)];

        var investigationStatuses = await commonRepository.GetInvestigationStatuses(_cts.Token);
        _investigationStatuses = CreateGdsOptionItems(
            investigationStatuses.Select(x => new IdTextPair { Id = x.Id, Text = x.Text })
        );
        _selectedInvestigationStatusId = Model.InvestigationStatus;
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

    private void OnInvestigationStatusChanged(GdsOptionItem<Guid> option)
    {
        // Get the selected item to check its text or ID
        _selectedInvestigationStatusId = option.Value;
        StateHasChanged();
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
}