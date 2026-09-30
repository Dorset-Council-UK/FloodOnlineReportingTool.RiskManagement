using FloodOnlineReportingTool.Database.Repositories;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace FloodOnlineReportingTool.RiskManagement.Components.Pages;

public partial class Index(ILogger<Index> logger,
                           IActionLogRepository actionLogRepository,
                           IFloodEventRepository floodEventRepository) : System.IDisposable
{
    [CascadingParameter]
    public Task<AuthenticationState>? AuthenticationState { get; set; }

    private readonly CancellationTokenSource _cts = new();
    private int _userActionCount;
    private string _userActionStyle = "success";
    private int _userPropertyCount;
    private string _userPropertyStyle = "success";
    private int _past30DaysCount;
    private string _userEventStyle = "success";
    private string _floodEventsFriendlyText = "";
    private string _floodedPropertiesFriendlyText = "";
    private string _userActionsFriendlyText = "";
    private bool _isLoading = false;
    protected override async Task OnInitializedAsync()
    {
        _isLoading = true;
        try
        {
            // Don't get any data if you don't have permission
            if (AuthenticationState is null)
            {
                logger.LogWarning("Attempt to access home page without authentication");
                return;
            }

            var authState = await AuthenticationState;
            if (!authState.User.IsAuthenticated)
            {
                logger.LogWarning("Attempt to access home page without authentication");
                return;
            }

            _userActionCount = await actionLogRepository.GetActionCountForUser(_cts.Token);
            _userPropertyCount = await actionLogRepository.GetPropertyCountForUser(_cts.Token);
            _past30DaysCount = await floodEventRepository.CountCreatedInPast30Days(_cts.Token);

            _userActionStyle = _userActionCount <= 3 ? "success" : (_userActionCount <= 5 ? "warning" : "danger");
            _userPropertyStyle = _userPropertyCount <= 3 ? "success" : (_userPropertyCount <= 6 ? "warning" : "danger");
            _userEventStyle = _past30DaysCount <= 3 ? "success" : (_past30DaysCount <= 6 ? "warning" : "danger");

            _floodEventsFriendlyText = $"{_past30DaysCount} flood events recorded in the last 30 days";
            _floodedPropertiesFriendlyText = $"{_userPropertyCount} flooded properties recorded in the last 30 days";
            _userActionsFriendlyText = $"{_userActionCount} actions waiting for you";
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "An error occurred while retrieving the data for the home page.");
            throw; // Rethrow the exception to be handled by the global error handler
        }
        finally
        {
            _isLoading = false;
        }
        

     }

    public void Dispose()
    {
        _cts.Cancel();
        _cts.Dispose();
    }
}