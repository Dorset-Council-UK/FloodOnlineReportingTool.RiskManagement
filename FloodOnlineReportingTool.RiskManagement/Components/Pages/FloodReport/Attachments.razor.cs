using FloodOnlineReportingTool.RiskManagement.Models.Order;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Identity.Web;
using System.Security.Claims;

namespace FloodOnlineReportingTool.RiskManagement.Components.Pages.FloodReport;

public partial class Attachments(
    ILogger<Attachments> logger,
    MicrosoftIdentityConsentAndConditionalAccessHandler consentHandler)
{
    private IReadOnlyCollection<GdsBreadcrumb> Breadcrumbs { get; set; } = [];

    [CascadingParameter]
    public Task<AuthenticationState>? AuthenticationState { get; set; }
    private bool _isLoading = true;
    [Parameter]
    public required string Reference { get; set; }

    private readonly CancellationTokenSource _cts = new();

    protected override async Task OnInitializedAsync()
    {
        Breadcrumbs = CreateBreadcrumbs();

        // Don't get any data if you don't have permission.
        AuthenticationState? authState;
        if (AuthenticationState is null)
        {
            logger.LogWarning("Attempt to access flood report attachments without authentication");
            return;
        }
        else
        {
            authState = await AuthenticationState;
            if (!authState.User.IsAuthenticated)
            {
                logger.LogWarning("Attempt to access flood report attachments without authentication");
                return;
            }
        }

        _isLoading = false;
    }

    private IReadOnlyCollection<GdsBreadcrumb> CreateBreadcrumbs()
    {
        return
        [
            GeneralPages.Home.ToGdsBreadcrumb(),
            new ($"{ReportPages.Details.Url}/{Reference}", ReportPages.Details.Title),
            new ($"{ReportPages.Attachments.Url}/{Reference}", ReportPages.Attachments.Title)
        ];
    }
}