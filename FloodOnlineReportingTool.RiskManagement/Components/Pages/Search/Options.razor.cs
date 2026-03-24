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
    private IReadOnlyCollection<GdsOptionItem<ResultTypes>> _resultOptions = [];

    protected override async Task OnInitializedAsync()
    {
        // Setup model and edit context
        Model ??= new();
        searchContext = new(Model);
        searchContext.SetFieldCssClassProvider(new GdsFieldCssClassProvider());
        _floodSourceOptions = await CreateFloodSourceOptions(Model.ByFloodCause);
        _resultOptions = CreateGdsOptionItems(Enum.GetValues<ResultTypes>());
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            var options = searchOptionsState.Options;
            // GdsInputDate caches the GdsDate object reference on first render, so the existing
            // objects must be mutated in-place rather than replaced to keep the component in sync.
            if (options is not null)
            {
                Model.FromDate.DayText = options.FromDate.DayText;
                Model.FromDate.MonthText = options.FromDate.MonthText;
                Model.FromDate.YearText = options.FromDate.YearText;
                Model.ToDate.DayText = options.ToDate.DayText;
                Model.ToDate.MonthText = options.ToDate.MonthText;
                Model.ToDate.YearText = options.ToDate.YearText;
                Model.ByPlaceName = options.ByPlaceName;
                Model.ByPersonsName = options.ByPersonsName;
                Model.ByFloodCause = options.ByFloodCause;
                Model.ByPolygon = options.ByPolygon;
                Model.ShowResultsAs = options.ShowResultsAs;
                Model.BufferText = options.BufferText;
                Model.Buffer = options.Buffer;
            }
            _floodSourceOptions = await CreateFloodSourceOptions(Model.ByFloodCause);

            _isLoading = false;
            StateHasChanged();
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

    private async Task<IReadOnlyCollection<GdsOptionItem<Guid>>> CreateFloodSourceOptions(IList<Guid> selectedValues)
    {
        const string idPrefix = "flood-source";
        var floodProblems = await commonRepository.GetFloodProblems("Primary", _cts.Token);
        return [.. floodProblems.Select((o, idx) => CreateOption(o, idPrefix, selectedValues))];
    }

    private static GdsOptionItem<Guid> CreateOption(FloodProblem floodProblem, string idPrefix, IList<Guid> selectedValues)
    {
        var id = $"{idPrefix}-{floodProblem.Id}".AsSpan();
        var label = floodProblem.TypeName.AsSpan();
        var selected = selectedValues.Contains(floodProblem.Id);

        return new GdsOptionItem<Guid>(id, label, floodProblem.Id, selected);
    }

    private static List<GdsOptionItem<ResultTypes>> CreateGdsOptionItems(IEnumerable<ResultTypes> results)
    {
        var optionItems = new List<GdsOptionItem<ResultTypes>>
        {
            new("0".AsSpan(), "Select an option".AsSpan(), default, false)
        };
        optionItems.AddRange(results.Select((o, idx) =>
            new GdsOptionItem<ResultTypes>((idx + 1).ToString(CultureInfo.InvariantCulture).AsSpan(), o.ToString().AsSpan(), o, false)));
        return optionItems;
    }

    private async Task OnTypeChange(GdsOptionItem<ResultTypes> option)
    {
        // Do something when the select value changes
        Model.ShowResultsAs = option.Value;
        StateHasChanged();
    }

}