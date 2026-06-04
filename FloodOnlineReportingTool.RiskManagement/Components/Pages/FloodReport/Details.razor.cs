using FloodOnlineReportingTool.Contracts.Shared.Search;
using FloodOnlineReportingTool.RiskManagement.Data.Repositories;
using FloodOnlineReportingTool.RiskManagement.Services;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Identity.Web;
using System.Security.Claims;

namespace FloodOnlineReportingTool.RiskManagement.Components.Pages.FloodReport;

public partial class Details(
    ILogger<Details> logger,
    IReportStatusClient reportStatusClient,
    ICommonRepository commonRepository,
    MicrosoftIdentityConsentAndConditionalAccessHandler consentHandler)
{
    private IReadOnlyCollection<GdsBreadcrumb> Breadcrumbs { get; set; } = [
        new("", "Home"),
    ];

    [CascadingParameter]
    public Task<AuthenticationState>? AuthenticationState { get; set; }

    private bool _gettingToken;
    private bool _isLoading = true;
    private bool _notFound = false;
    private string ReportStatus { get; set; }

    [Parameter]
    public required string Reference { get; set; }

    private readonly CancellationTokenSource _cts = new();
    private SearchResultFloodReport? ViewModel { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        // Don't get any data if you don't have permission
        AuthenticationState? authState;
        if (AuthenticationState is null)
        {
            logger.LogWarning("Attempt to access flood report details without authentication");
            return;
        }
        else
        {
            authState = await AuthenticationState;
            if (!authState.User.IsAuthenticated)
            {
                logger.LogWarning("Attempt to access flood report details without authentication");
                return;
            }
        }

        try
        {
            _gettingToken = false;

            // GetFloodReportDetail returns SearchResultFloodReport, fully mapped and ready for display.
            // SearchFloodReportByReference returns FloodReportWithSourceReferences (raw API shape) and
            // should not be used here as it does not contain the full data needed for display.
            var reportResult = await reportStatusClient.GetFloodReportDetail(Reference, true, _cts.Token);

            if (reportResult.IsSuccess && reportResult.ResultModel is not null)
            {
                var reportStatuses = await commonRepository.GetReportStatuses(_cts.Token);
                ViewModel = reportResult.ResultModel;
                ReportStatus = reportStatuses.Where(rs => rs.Id == ViewModel.RecordStatusId).Select(rs => rs.Text).FirstOrDefault() ?? "";
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
        _isLoading = false;
    }
}