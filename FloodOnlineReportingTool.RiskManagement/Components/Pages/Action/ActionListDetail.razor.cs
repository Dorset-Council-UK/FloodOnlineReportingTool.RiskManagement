using FloodOnlineReportingTool.Contracts.Shared.Search;
using FloodOnlineReportingTool.RiskManagement.Data.Models.Actions;
using FloodOnlineReportingTool.RiskManagement.Data.Repositories;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.QuickGrid;
using Microsoft.Identity.Web;
using System.Security.Claims;

namespace FloodOnlineReportingTool.RiskManagement.Components.Pages.Action;

public partial class ActionListDetail(
    ILogger<Index> logger,
    IActionLogRepository actionLogRepository)
{
    [CascadingParameter]
    public Task<AuthenticationState>? AuthenticationState { get; set; }

    public int CurrentPage = 0;
    public int TotalPages = 0;

    private readonly PaginationState _pagination = new() { ItemsPerPage = 10 };
    private QuickGrid<ActionLog> _grid = default!;

    public GridItemsProvider<ActionLog>? ItemsProvider { get; set; } = default!;

    private PagedResult<ActionLog> ActionLogs { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        if (AuthenticationState is not null)
        {
            var authState = await AuthenticationState;
            if (authState.User.IsAuthenticated)
            {
                ItemsProvider = ProvideSearchResults;
            }
        }
    }

    /// <summary>
    /// Provides paged action log results to the QuickGrid component.
    /// Called by QuickGrid with paging context on each render/page change.
    /// </summary>
    private async ValueTask<GridItemsProviderResult<ActionLog>> ProvideSearchResults(
        GridItemsProviderRequest<ActionLog> request)
    {
        try
        {
            var pageSize = request.Count ?? _pagination.ItemsPerPage;
            var pageNumber = (request.StartIndex / pageSize) + 1;

            // Extract sort info from the QuickGrid request
            var sortColumn = request.SortByColumn?.Title;
            var ascending = request.SortByAscending;

            var pagedResult = await actionLogRepository.GetPagedActionLogsForUser(
                pageNumber, pageSize, sortColumn, ascending, request.CancellationToken);

            return GridItemsProviderResult.From(
                pagedResult?.Results.ToList() ?? [],
                pagedResult?.TotalCount ?? 0);
        }
        catch (MicrosoftIdentityWebChallengeUserException ex)
        {
            logger.LogWarning(ex, "Consent required while fetching action logs for table view");
            return GridItemsProviderResult.From<ActionLog>([], 0);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error while fetching action logs for table view");
            return GridItemsProviderResult.From<ActionLog>([], 0);
        }
    }
}