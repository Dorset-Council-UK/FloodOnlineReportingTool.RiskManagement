using FloodOnlineReportingTool.RiskManagement.Models.Order;
using FloodOnlineReportingTool.RiskManagement.Services;
using Microsoft.AspNetCore.Components;

namespace FloodOnlineReportingTool.RiskManagement.Components.Pages.FloodEvent;

public partial class Index(
    ILogger<Index> logger,
    NavigationManager navigationManager
)
{
    [Parameter]
    public string? SearchReference { get; set; }

    private bool _redirectingToSearchOptions;

    protected override void OnInitialized()
    {
        if (string.IsNullOrWhiteSpace(SearchReference))
        {
            logger.LogInformation("No search reference provided in query parameters. Unable to load search results.");
            _redirectingToSearchOptions = true;
            navigationManager.NavigateTo(SearchPages.Options.Url);
        }
    }

    // TODO - uncomment below code and tweak once this page is more complete (this triggers a recent work item to be added to local storage,
    // but the model is not yet populated)

    //protected override async Task OnAfterRenderAsync(bool firstRender)
    //{
    //    if (Model is not null)
    //    {
    //        //add to recent work store
    //        await settingsService.AddRecentWork($"{SearchReference}", "Flood Event", Model.SearchTitle, $"event/{SearchReference}");
    //    }
    //    await base.OnAfterRenderAsync(firstRender);
    //}

}