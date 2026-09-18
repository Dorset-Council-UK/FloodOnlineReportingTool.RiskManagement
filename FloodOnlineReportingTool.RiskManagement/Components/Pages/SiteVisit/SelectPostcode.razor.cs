using FloodOnlineReportingTool.RiskManagement.Models.Order;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace FloodOnlineReportingTool.RiskManagement.Components.Pages.SiteVisit;

public partial class SelectPostcode(
    ILogger<SelectPostcode> logger,
    NavigationManager navigationManager,
    IAuthorizationService authorizationService,
    ProtectedSessionStorage protectedSessionStorage
) : IAsyncDisposable
{
    // Page order properties
    public string Title { get; set; } = "Select by Postcode";

    [SupplyParameterFromQuery]
    private bool FromSummary { get; set; }
    private PageInfo NextPage => FromSummary
        ? VisitPages.Summary
        : Model.PostcodeKnown == true ? VisitPages.Address : VisitPages.Location;
    private PageInfo PreviousPage => FromSummary
        ? VisitPages.Summary
        : VisitPages.Home;

    private Models.Visit.SelectPostcode Model { get; set; } = default!;

    private EditContext _editContext = default!;
    [CascadingParameter]
    public Task<AuthenticationState>? AuthenticationState { get; set; }
    private readonly CancellationTokenSource _cts = new();
    private bool _isLoading = true;
    private IReadOnlyCollection<GdsOptionItem<bool>> _postcodeKnownOptions = [
        new("postcode-known-yes", "Yes", value: true),
        new("postcode-known-no", "No", value: false),
    ];

    protected override async Task OnInitializedAsync()
    {
        var isAuthorizedReader = await AuthenticationState.TryIsWriterAsync(authorizationService);
        if (!isAuthorizedReader)
        {
            logger.LogWarning("Attempt to access a page without correct permission");
            _isLoading = false;
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
            _isLoading = false;
            return;
        }

        if (firstRender)
        {
            // Set any previously entered data
            //var createExtraData = await GetCreateExtraData();
            //Model.Postcode = createExtraData.Postcode;
            if (Model.Postcode != null)
            {
                Model.PostcodeKnown = true;
                _postcodeKnownOptions.Single(o => o.Value).Selected = true;
            }

            _isLoading = false;
            StateHasChanged(); 
        }
    }

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

    private async Task OnSubmit()
    {
        if (_editContext.Validate())
        {
            await OnValidSubmit();
        }
    }

    private async Task OnValidSubmit()
    {
        // Save the postcode
        //var createExtraData = await GetCreateExtraData();
        //var updatedExtraData = createExtraData with
        //{
        //    Postcode = Model.Postcode?.ToUpperInvariant(),
        //};

        //await protectedSessionStorage.SetAsync(SessionConstants.EligibilityCheck_ExtraData, updatedExtraData);

        // Go to the next page or back to the summary
        navigationManager.NavigateTo(NextPage.Url);
    }

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

    //    logger.LogWarning("Eligibility Check > Extra Data was not found in the protected storage.");
    //    return new();
    //}
}
