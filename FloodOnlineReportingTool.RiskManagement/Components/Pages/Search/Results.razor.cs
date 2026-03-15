using FloodOnlineReportingTool.RiskManagement.Models;
using FloodOnlineReportingTool.RiskManagement.Models.Search;
using FloodOnlineReportingTool.RiskManagement.Services;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.Extensions.Logging;

namespace FloodOnlineReportingTool.RiskManagement.Components.Pages.Search;

public partial class Results(ILogger<Results> logger, ISearchOptionsState searchOptionsState)
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
            }

            // TODO: logic to get params from DB if not using options route. 

            _isLoading = false;
            StateHasChanged();
        }
    }
}