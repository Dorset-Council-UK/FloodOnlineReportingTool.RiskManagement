using FloodOnlineReportingTool.RiskManagement.Models.Azure;
using FloodOnlineReportingTool.RiskManagement.Models.Order;
using FloodOnlineReportingTool.RiskManagement.Services;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace FloodOnlineReportingTool.RiskManagement.Components.Pages.SiteVisit;

public partial class Address(
    ILogger<Address> logger,
    IAzureApiClient azureApiClient,
    IAuthorizationService authorizationService,
    ProtectedSessionStorage protectedSessionStorage,
    NavigationManager navigationManager
) : IAsyncDisposable
{
    // Page order properties
    public string Title { get; set; } = "Select an Address";

    [CascadingParameter]
    public Task<AuthenticationState>? AuthenticationState { get; set; }

    [SupplyParameterFromQuery]
    private bool FromSummary { get; set; }
    private PageInfo NextPage => FromSummary
        ? VisitPages.Summary
        : VisitPages.PropertyType;
    private PageInfo PreviousPage => FromSummary
        ? VisitPages.Summary
        : VisitPages.Postcode;
    private Models.Visit.Address Model { get; set; } = default!;

    private EditContext _editContext = default!;
    private readonly CancellationTokenSource _cts = new();
    private bool _isSearching = true;
    private IList<ApiAddress> _addresses = [];

    public async ValueTask DisposeAsync()
    {
        try
        {
            await _cts.CancelAsync();
            _cts.Dispose();
        }
        catch (Exception)
        {
        }

        GC.SuppressFinalize(this);
    }

    protected override async Task OnInitializedAsync()
    {
        var isAuthorizedReader = await AuthenticationState.TryIsWriterAsync(authorizationService);
        if (!isAuthorizedReader)
        {
            logger.LogWarning("Attempt to access a page without correct permission");
            _isSearching = false;
            return;
        }

        // Setup model and edit context
        Model ??= new();
        _editContext = new(Model);
        _editContext.SetFieldCssClassProvider(new GdsFieldCssClassProvider());
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        var isAuthorizedReader = await AuthenticationState.TryIsWriterAsync(authorizationService);
        if (!isAuthorizedReader)
        {
            logger.LogWarning("Attempt to access a page without correct permission");
            _isSearching = false;
            return;
        }

        if (firstRender)
        {
            // Set any previously entered data
            //var eligibilityCheck = await GetEligibilityCheck();
            //var createExtraData = await GetCreateExtraData();

            //Model.Postcode = createExtraData.Postcode;
            //Model.Easting = eligibilityCheck.Easting == 0 ? null : eligibilityCheck.Easting;
            //Model.Northing = eligibilityCheck.Northing == 0 ? null : eligibilityCheck.Northing;
            //Model.UPRN = eligibilityCheck.Uprn == 0 ? null : eligibilityCheck.Uprn;
            //Model.IsAddress = eligibilityCheck.IsAddress;
            //Model.LocationDesc = eligibilityCheck.LocationDesc;

            Model.Postcode = "DT1 1XJ";
            _addresses = await AddressSearch();

            StateHasChanged();
        }
    }

    private async Task OnSubmit()
    {
        if (_editContext.Validate())
        {
            await OnValidSubmit();
        }
    }

    private async Task OnValidSubmit()
    {
        // Remember the selected address
        var apiAddress = _addresses.FirstOrDefault(o => o.UPRN == Model.UPRN);
        if (apiAddress != null)
        {
            //var eligibilityCheck = await GetEligibilityCheck();
            //var createExtraData = await GetCreateExtraData();

            //var updatedEligibilityCheck = eligibilityCheck with
            //{
            //    Uprn = apiAddress.UPRN,
            //    Easting = apiAddress.Easting,
            //    Northing = apiAddress.Northing,
            //    LocationDesc = apiAddress.ConcatenatedAddress,
            //};

            //var updatedExtraData = createExtraData with
            //{
            //    Postcode = apiAddress.Postcode.ToUpperInvariant(),
            //    PrimaryClassification = apiAddress.PrimaryClassification,
            //    SecondaryClassification = apiAddress.SecondaryClassification,
            //};

            //await protectedSessionStorage.SetAsync(SessionConstants.EligibilityCheck, updatedEligibilityCheck);
            //await protectedSessionStorage.SetAsync(SessionConstants.EligibilityCheck_ExtraData, updatedExtraData);

            // Go to the next page or back to the summary
            navigationManager.NavigateTo(NextPage.Url);
        }
    }

    //private async Task<EligibilityCheckDto> GetEligibilityCheck()
    //{
    //    var data = await protectedSessionStorage.GetAsync<EligibilityCheckDto>(SessionConstants.EligibilityCheck);
    //    if (data.Success)
    //    {
    //        if (data.Value != null)
    //        {
    //            return data.Value;
    //        }
    //    }

    //    logger.LogWarning("Eligibility Check was not found in the protected storage.");
    //    return new EligibilityCheckDto();
    //}

    //private async Task<ExtraData> GetCreateExtraData()
    //{
    //    var data = await protectedSessionStorage.GetAsync<ExtraData>(SessionConstants.EligibilityCheck_ExtraData);
    //    if (data.Success)
    //    {
    //        if (data.Value != null)
    //        {
    //            return data.Value;
    //        }
    //    }

    //    logger.LogDebug("Eligibility Check > Extra Data was not found in the protected storage.");
    //    return new();
    //}

    /// <summary>
    /// Search for addresses based on the postcode.
    /// </summary>
    private async Task<IList<ApiAddress>> AddressSearch()
    {
        if (string.IsNullOrWhiteSpace(Model.Postcode) && !Model.IsAddress)
        {
            logger.LogDebug("Non address query so not searching");
            _isSearching = false;
            return [];
        }

        if (string.IsNullOrWhiteSpace(Model.Postcode))
        {
            logger.LogDebug("No postcode, not searching");
            _isSearching = false;
            return [];
        }

        try
        {
            logger.LogDebug("Searching for addresses with postcode {Postcode}", Model.Postcode);

            _isSearching = true;
            var referer = navigationManager.ToAbsoluteUri("");
            var result = await azureApiClient.AddressSearch(Model.Postcode, SearchAreaOptions.dorset, referer, _cts.Token);
            return result?.ToList() ?? new List<ApiAddress>();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "There was an unexpected problem getting the address information");
        }
        finally
        {
            _isSearching = false;
        }

        return [];
    }

    private async Task SearchAgain()
    {
        Model.UPRN = null;
        _addresses = await AddressSearch();
    }
}
