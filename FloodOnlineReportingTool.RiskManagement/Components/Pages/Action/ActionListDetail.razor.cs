using FloodOnlineReportingTool.RiskManagement.Data.Models.Actions;
using FloodOnlineReportingTool.RiskManagement.Data.Repositories;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace FloodOnlineReportingTool.RiskManagement.Components.Pages.Action;

public partial class ActionListDetail(
    ILogger<Index> logger,
    IActionLogRepository actionLogRepository)
{
    [CascadingParameter]
    public Task<AuthenticationState>? AuthenticationState { get; set; }

    private List<ActionLog> actionLogs { get; set; } = [];

    protected override async Task OnInitializedAsync()
    {
        // Don't get any data if you don't have permission
        if (AuthenticationState is not null)
        {
            var authState = await AuthenticationState;
            if (authState.User.IsAuthenticated)
            {
                var userActions = await actionLogRepository.GetActionLogsForUser(CancellationToken.None);
                actionLogs.AddRange(userActions);

                if (actionLogs.Count == 0)
                {
                    logger.LogInformation("No action logs found for user");
                }
            }
        }
    }
}