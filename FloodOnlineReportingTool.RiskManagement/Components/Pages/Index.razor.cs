using FloodOnlineReportingTool.RiskManagement.Data.Repositories;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace FloodOnlineReportingTool.RiskManagement.Components.Pages;

public partial class Index(ILogger<Index> logger,
                           IActionLogRepository actionLogRepository,
                           IFloodEventRepository floodEventRepository) 
{
    [CascadingParameter]
    public Task<AuthenticationState>? AuthenticationState { get; set; }

    private readonly CancellationTokenSource _cts = new();
    private int _userActionCount;
    private string _userActionStyle = "success";
    private int _userPropertyCount;
    private string _userPropertyStyle = "success";
    private int _userEventCount;
    private string _userEventStyle = "success";

    protected override async Task OnInitializedAsync()
    {
        if (AuthenticationState is not null)
        {
            var authState = await AuthenticationState;
            if (authState.User.IsAuthenticated)
            {
                _userActionCount = await actionLogRepository.GetActionCountForUser(_cts.Token);
                _userPropertyCount = await actionLogRepository.GetPropertyCountForUser(_cts.Token);
                _userEventCount = await floodEventRepository.GetEventCountForUser(_cts.Token);

                _userActionStyle = _userActionCount <= 3 ? "success" : (_userActionCount <= 5 ? "warning" : "danger");
                _userPropertyStyle = _userPropertyCount <= 3 ? "success" : (_userPropertyCount <= 6 ? "warning" : "danger");
                _userEventStyle = _userEventCount <= 3 ? "success" : (_userEventCount <= 6 ? "warning" : "danger");
            }
        }    
    }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }
}