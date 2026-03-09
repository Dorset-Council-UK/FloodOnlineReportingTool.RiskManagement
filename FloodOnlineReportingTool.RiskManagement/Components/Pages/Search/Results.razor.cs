using FloodOnlineReportingTool.RiskManagement.Models;
using FloodOnlineReportingTool.RiskManagement.Models.Search;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.Extensions.Logging;

namespace FloodOnlineReportingTool.RiskManagement.Components.Pages.Search;

public partial class Results(ILogger<Results> logger, ProtectedSessionStorage protectedSessionStorage)
{
    private EditContext searchContext = default!;
    private SearchResultsViewModel Model { get; set; } = default!;

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

            // Get any search parameters passed from protected storage.
            var options = await GetSearchOptions();
            Model.ToDate = options.ToDate;
            Model.FromDate = options.FromDate;
            Model.ByPlaceName = options.ByPlaceName;
            Model.ByPersonsName = options.ByPersonsName;
            Model.ByFloodCause = options.ByFloodCause;
            Model.ByPolygon = options.ByPolygon;
            Model.ShowResultsAs = options.ShowResultsAs;

            // TODO: logic to get params from DB if not using options route. 

            _isLoading = false;
            StateHasChanged();
        }
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
}