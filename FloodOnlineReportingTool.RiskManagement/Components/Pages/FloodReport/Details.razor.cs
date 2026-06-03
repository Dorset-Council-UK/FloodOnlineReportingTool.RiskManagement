using FloodOnlineReportingTool.Contracts.Shared.Search;
using FloodOnlineReportingTool.RiskManagement.Services;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Identity.Web;

namespace FloodOnlineReportingTool.RiskManagement.Components.Pages.FloodReport;

public partial class Details(
    ILogger<Details> logger,
    IReportStatusClient reportStatusClient,
    MicrosoftIdentityConsentAndConditionalAccessHandler consentHandler)
{
    private IReadOnlyCollection<GdsBreadcrumb> Breadcrumbs { get; set; } = [
        new("", "Home"),
    ];

    [CascadingParameter]
    public Task<AuthenticationState>? AuthenticationState { get; set; }

    private bool _gettingToken;
    private bool _notPermitted = false;
    private bool _notFound = false;

    [Parameter]
    public required string Reference { get; set; }

    private readonly CancellationTokenSource _cts = new();
    private SearchResultFloodReport? ViewModel { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        // Don't get any data if you don't have permission
        if (AuthenticationState is null)
        {
            _notPermitted = true;
            logger.LogWarning("Attempt to access flood report details without authentication");
            return;
        }

        try
        {
            _gettingToken = false;

            // GetFloodReportDetail returns SearchResultFloodReport, fully mapped and ready for display.
            // SearchFloodReportByReference returns FloodReportWithSourceReferences (raw API shape) and
            // should not be used here as it does not contain the full data needed for display.
            var reportResult = await reportStatusClient.GetFloodReportDetail(Reference, true, _cts.Token);

            if (reportResult.IsSuccess)
            {
                ViewModel = reportResult.ResultModel;
                logger.LogInformation("Flood report detail loaded successfully for reference");
            }
            else
            {
                _notFound = true;
                logger.LogWarning("Flood report detail not found for reference");
            }
        }
        catch (Exception ex)
        {
            // Handles MsalUiRequiredException / MicrosoftIdentityWebChallengeUserException.
            // consentHandler.HandleException will trigger a redirect to re-authenticate
            // or prompt for incremental consent, then return the user to this page.
            _gettingToken = true;
            consentHandler.HandleException(ex);
        }
    }
}