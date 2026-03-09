using FloodOnlineReportingTool.RiskManagement.Data.Models.Lookups;
using FloodOnlineReportingTool.RiskManagement.Data.Repositories;
using FloodOnlineReportingTool.RiskManagement.Models;
using FloodOnlineReportingTool.RiskManagement.Models.Order;
using FloodOnlineReportingTool.RiskManagement.Models.Search;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;


namespace FloodOnlineReportingTool.RiskManagement.Components.Pages.Search;

public partial class Options(
    ILogger<Options> logger,
    ICommonRepository commonRepository,
    NavigationManager navigationManager,
    ProtectedSessionStorage protectedSessionStorage
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
    private string _searchTerm = string.Empty;
    private IReadOnlyCollection<GdsOptionItem<string>> _parishResults = new List<GdsOptionItem<string>>();
    private IReadOnlyCollection<GdsOptionItem<string>> _wardResults = new List<GdsOptionItem<string>>();
    private CancellationTokenSource? _searchCts;

    protected override async Task OnInitializedAsync()
    {
        // Setup model and edit context
        Model ??= new();
        searchContext = new(Model);
        searchContext.SetFieldCssClassProvider(new GdsFieldCssClassProvider());
        _floodSourceOptions = await CreateFloodSourceOptions(Model.ByFloodCause);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // Set any previously entered data
            var options = await GetSearchOptions();
            Model.ToDate = options.ToDate;
            Model.FromDate = options.FromDate;
            Model.ByPlaceName = options.ByPlaceName;
            Model.ByPersonsName = options.ByPersonsName;
            Model.ByFloodCause = options.ByFloodCause;
            Model.ByPolygon = options.ByPolygon;
            Model.ShowResultsAs = options.ShowResultsAs;
            Model.BufferText = options.BufferText;
            Model.Buffer = options.Buffer;

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
            
            if (_searchCts != null)
            {
                await _searchCts.CancelAsync();
                _searchCts.Dispose();
            }
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
        //Store the search options in session storage for retrieval on the next page
        try
        {
            await protectedSessionStorage.SetAsync(SessionConstants.SearchOptions, Model);
        } catch (Exception ex)
        {
            logger.LogError(ex, "Issue setting protected storage for search options page");
        }

        // Go to the next page or pass back to the summary
        var nextPage = SearchPages.Results;
        var nextPageUrl = nextPage.Url;
        navigationManager.NavigateTo(nextPageUrl);
    }

    private async Task GoToMap()
    {
        //Store the search options in session storage for retrieval on the next page
        try
        {
            await protectedSessionStorage.SetAsync(SessionConstants.SearchOptions, Model);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Issue setting protected storage for search options page");
        }

        // Go to the next page or pass back to the summary
        var nextPage = SearchPages.OptionsMap;
        var nextPageUrl = nextPage.Url;
        navigationManager.NavigateTo(nextPageUrl);
    }

    private async Task<SearchOptionsViewModel> GetSearchOptions()
    {
        var data = await protectedSessionStorage.GetAsync<SearchOptionsViewModel>(SessionConstants.SearchOptions);
        if (data.Success)
        {
            if (data.Value != null)
            {
                return data.Value;
            }
        }

        logger.LogWarning("Search options were not found in the protected storage.");
        return new SearchOptionsViewModel();
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
    
}