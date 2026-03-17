using FloodOnlineReportingTool.RiskManagement.Data.Repositories;
using FloodOnlineReportingTool.RiskManagement.Extensions;
using FloodOnlineReportingTool.RiskManagement.Models.Search;
using FloodOnlineReportingTool.RiskManagement.Services;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace FloodOnlineReportingTool.RiskManagement.Components.Pages.Search;

public partial class Results(ILogger<Results> logger, ICommonRepository commonRepository, ISearchResultsState searchResultsState)
{
    private EditContext searchContext = default!;
    private readonly CancellationTokenSource _cts = new();
    private SearchResultsViewModel Model { get; set; } = default!;

    [SupplyParameterFromQuery(Name = "ref")]
    public required string SearchReference { get; set; }

    private bool _isLoading = true;

    protected override void OnInitialized()
    {
        // Setup model and edit context
        Model ??= new();
        searchContext = new(Model);
        searchContext.SetFieldCssClassProvider(new GdsFieldCssClassProvider());
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {

            var options = searchResultsState.Options;
            if (options is null)
            {
                var savedSearch = await commonRepository.GetSavedSearchByReference(SearchReference, _cts.Token);

                if (savedSearch == null)
                {
                    // Failure - No saved search found for reference
                    _isLoading = false;
                    StateHasChanged();
                    return;
                }

                // Convert ViewModel → Entity
                Model = savedSearch.SavedSearchToResultsView();

                // Persist in state for page navigation
                searchResultsState.SetResultOptions(Model);
            } else
            {
                Model.ByReference = SearchReference;
            }

            _isLoading = false;
            StateHasChanged();
        }
    }
}