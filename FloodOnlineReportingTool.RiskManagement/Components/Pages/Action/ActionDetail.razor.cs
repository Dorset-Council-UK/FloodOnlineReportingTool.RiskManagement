using FloodOnlineReportingTool.RiskManagement.Data.Models.Actions;
using FloodOnlineReportingTool.RiskManagement.Data.Repositories;
using FloodOnlineReportingTool.RiskManagement.Models.Order;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace FloodOnlineReportingTool.RiskManagement.Components.Pages.Action;

public partial class ActionDetail(
    ILogger<Index> logger,
    IActionLogRepository actionLogRepository,
    NavigationManager navigationManager)
{
    [CascadingParameter]
    public Task<AuthenticationState>? AuthenticationState { get; set; }

    [Parameter]
    public required string ActionRef { get; set; }

    private ActionLog? actionLog { get; set; } = null;

    protected override async Task OnInitializedAsync()
    {
        // Don't get any data if you don't have permission
        if (AuthenticationState is not null)
        {
            var authState = await AuthenticationState;
            if (authState.User.IsAuthenticated)
            {
                if (!Guid.TryParse(ActionRef, out var actionId)) { logger.LogWarning("Invalid action id '{ActionRef}'", ActionRef); return; }
                var action = await actionLogRepository.GetActionLogById(actionId, CancellationToken.None);
                if (action is not null && action.IsSuccess)
                {
                    if (action.ResultModel is not null)
                    {
                        actionLog = action.ResultModel;
                        if(actionLog.Action.ActionType == Data.Models.Enums.ActionTypes.Redirect)
                        {
                            // We always just pass this on to the search as a search by ID result
                            var nextPage = $"{SearchPages.Results.Url}/{actionLog.ReportReference}";
                            navigationManager.NavigateTo(nextPage);
                        }
                    } else
                    {
                        logger.LogWarning("Action not found");
                    }
                }
            }

        }
    }
}