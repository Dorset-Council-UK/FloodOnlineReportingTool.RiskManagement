using FloodOnlineReportingTool.RiskManagement.Data.Models.Enums;
using FloodOnlineReportingTool.RiskManagement.Data.Models.Helpers;
using FloodOnlineReportingTool.RiskManagement.Data.Models.Lookups;
using FloodOnlineReportingTool.RiskManagement.Data.Repositories;
using FloodOnlineReportingTool.RiskManagement.Extensions;
using FloodOnlineReportingTool.RiskManagement.Models.Order;
using FloodOnlineReportingTool.RiskManagement.Models.Search;
using FloodOnlineReportingTool.RiskManagement.Services.SavedStates;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System.Globalization;
using static MassTransit.ValidationResultExtensions;


namespace FloodOnlineReportingTool.RiskManagement.Components.Pages.Search;

public partial class Options(
    ILogger<Options> logger,
    ICommonRepository commonRepository,
    NavigationManager navigationManager,
    ISearchOptionsState searchOptionsState
) : IPageOrder, IAsyncDisposable
{
    public string Title { get; set; } = SearchPages.Options.Title;

    public IReadOnlyCollection<GdsBreadcrumb> Breadcrumbs { get; set; } = [
        GeneralPages.Home.ToGdsBreadcrumb(),
        SearchPages.Home.ToGdsBreadcrumb(),
        SearchPages.Options.ToGdsBreadcrumb(),
    ];

    private SearchOptionsViewModel Model { get; set; } = default!;

    private EditContext searchContext = default!;
    private readonly CancellationTokenSource _cts = new();
    private bool _isLoading = true;

    private IReadOnlyCollection<GdsOptionItem<Guid>> _floodSourceOptions = [];

    protected override void OnInitialized()
    {
        // Setup model and edit context
        Model ??= new();
        searchContext = new(Model);
        searchContext.SetFieldCssClassProvider(new GdsFieldCssClassProvider());
    }

    protected override async Task OnParametersSetAsync()
    {
        // Configure option lists once on page load - No selections these are fixed option lists
        _floodSourceOptions = _floodSourceOptions.Count == 0 ? await CreateFloodSourceOptions() : _floodSourceOptions;

        // GdsInputDate caches the GdsDate object reference on first render, so the existing
        // objects must be mutated in-place rather than replaced to keep the component in sync.
        var options = searchOptionsState.Options;
        if (options is not null)
        {
            Model = options.ShallowCopy();
        }

        SetGdsOptions();
        _isLoading = false;
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
            logger.LogError(ex, "An unexpected error occurred while disposing {ComponentName}.", nameof(Options));
        }
    }

    private async Task OnSubmit()
    {
        // Update flood cause selection from checkboxes
        Model.ByFloodCause = [.. _floodSourceOptions.Where(o => o.Selected).Select(o => o.Value)];
        
        // Persist in state for page navigation
        searchOptionsState.SetOptions(Model);
        
        try
        {
            // Convert ViewModel → Entity
            var savedSearchEntity = Model.OptionsViewToSavedSearch(null);
            
            // Get or create SavedSearch in database
            var savedSearch = await commonRepository.GetOrCreateSavedSearch(null, null, savedSearchEntity, _cts.Token);

            // Navigate to results
            navigationManager.NavigateTo($"{SearchPages.Results.Url}/{savedSearch.Reference}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create or retrieve saved search");
        }
    }

    private void GoToMap()
    {
        Model.ByFloodCause = [.. _floodSourceOptions.Where(o => o.Selected).Select(o => o.Value)];
        searchOptionsState.SetOptions(Model);
        navigationManager.NavigateTo(SearchPages.OptionsMap.Url);
    }

    private void SetGdsOptions()
    {
        foreach(var options in _floodSourceOptions)
        {
            if (Model.ByFloodCause.Contains(options.Value))
            {
                options.Selected = true;
            }
            else
            {
                options.Selected = false;
            }
        }
    }

    private async Task<IReadOnlyCollection<GdsOptionItem<Guid>>> CreateFloodSourceOptions()
    {
        const string idPrefix = "flood-source";
        var floodProblems = await commonRepository.GetFloodProblems("Primary", _cts.Token);
        return [.. floodProblems.Select((o, idx) => CreateFloodOption(o, idPrefix))];
    }

    private static GdsOptionItem<Guid> CreateFloodOption(FloodProblem floodProblem, string idPrefix)
    {
        var id = $"{idPrefix}-{floodProblem.Id}".AsSpan();
        var label = floodProblem.TypeName.AsSpan();
        //var selected = selectedValues.Contains(floodProblem.Id);

        return new GdsOptionItem<Guid>(id, label, floodProblem.Id, false);
    }

}