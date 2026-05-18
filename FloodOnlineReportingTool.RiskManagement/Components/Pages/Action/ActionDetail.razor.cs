using FloodOnlineReportingTool.RiskManagement.Data.Models.Actions;
using FloodOnlineReportingTool.RiskManagement.Data.Repositories;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace FloodOnlineReportingTool.RiskManagement.Components.Pages.Action;

public partial class ActionDetail(
    ILogger<Index> logger,
    IActionLogRepository actionLogRepository)
{
    [CascadingParameter]
    public Task<AuthenticationState>? AuthenticationState { get; set; }

    [Parameter]
    public Guid ActionID { get; set; }

    private ActionLog? actionLog { get; set; } = null;

    protected override async Task OnInitializedAsync()
    {
        // Don't get any data if you don't have permission
        if (AuthenticationState is not null)
        {
            var authState = await AuthenticationState;
            if (authState.User.IsAuthenticated)
            {
                var action = await actionLogRepository.GetActionLogsByID(ActionID, CancellationToken.None);
                if (action is not null && action.IsSuccess)
                {
                    if (action.ResultModel is not null)
                    {
                        actionLog = action.ResultModel;
                    } else
                    {
                        logger.LogWarning("Action not found");
                    }
                }
            }

        }
    }
}