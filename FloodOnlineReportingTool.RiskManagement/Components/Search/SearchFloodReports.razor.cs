using FloodOnlineReportingTool.RiskManagement.Models;
using FloodOnlineReportingTool.RiskManagement.Models.Search;
using FloodOnlineReportingTool.RiskManagement.Settings;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Identity.Abstractions;
using Microsoft.Identity.Web;
using System.Globalization;

namespace FloodOnlineReportingTool.RiskManagement.Components.Search;

public partial class SearchFloodReports(
    ILogger<SearchFloodReports> logger,
    IGdsJsInterop gdsJs,
    IDownstreamApi downstreamApi,
    MicrosoftIdentityConsentAndConditionalAccessHandler consentHandler
) {
    private const string TryAgain = "Please try again and if the problem persists, contact support.";

    private SearchFloodReportViewModel Model { get; set; } = default!;
    private EditContext _editContext = default!;
    private bool _isLoading = true;
    private SearchResult? _searchResult;
    private SearchState _searchState = SearchState.NotStarted;

    protected override void OnInitialized()
    {
        Model ??= new();
        _editContext = new(Model);
        _editContext.SetFieldCssClassProvider(new GdsFieldCssClassProvider());
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await gdsJs.InitGds();

            _isLoading = false;
            StateHasChanged();
        }
    }

    [Authorize]
    [AuthorizeForScopes(ScopeKeySection = "RiskManagement:ReportStatusApi:Scopes")]
    private async Task OnSearchFloodReport()
    {
        var validated = _editContext.Validate();
        if (!validated)
        {
            return;
        }

        _searchState = SearchState.Searching;

        if (Model.Uprn is not null)
        {
            long uprn = Model.Uprn.Value;
            logger.LogInformation("Searching food reports with UPRN {Uprn}", uprn);

            try
            {
                _searchResult = await SearchFloodReportsByUprn(uprn);
            }
            catch (MicrosoftIdentityWebChallengeUserException ex)
            {
                consentHandler.HandleException(ex);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Search flood reports by UPRN {Uprn} failed.", uprn);
                _searchResult = SearchResult.Failure([
                    "I am sorry but there was a problem searching using that UPRN.",
                    ex.Message,
                    TryAgain,
                ]);
            }
        }

        _searchState = CurrentSearchState();

        return;
    }

    private SearchState CurrentSearchState()
    {
        if (_searchResult is null)
        {
            return SearchState.NotStarted;
        }

        if (_searchResult.Errors.Count > 0)
        {
            return SearchState.Error;
        }

        return _searchResult.FloodReports.Count > 0
            ? SearchState.ResultsFound
            : SearchState.NoResults;
    }

    private async Task<SearchResult> SearchFloodReportsByUprn(long uprn)
    {
        var floodReports = await downstreamApi
            .CallApiForUserAsync<IReadOnlyCollection<TEMP_FloodReport>>(RiskManagementSettings.ReportStatusApi, options =>
            {
                options.RelativePath = string.Create(CultureInfo.InvariantCulture, $"/api/search/uprn?uprn={uprn}");
            })
            .ConfigureAwait(false);

        return SearchResult.Success(floodReports ?? []);
    }
}
