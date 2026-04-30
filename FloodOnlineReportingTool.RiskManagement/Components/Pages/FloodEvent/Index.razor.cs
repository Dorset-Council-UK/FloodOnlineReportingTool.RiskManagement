using FloodOnlineReportingTool.RiskManagement.Models.Order;
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
}