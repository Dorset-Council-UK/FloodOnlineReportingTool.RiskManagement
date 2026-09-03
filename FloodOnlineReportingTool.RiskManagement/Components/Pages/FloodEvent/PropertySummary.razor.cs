using FloodOnlineReportingTool.RiskManagement.Models.Order;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text.Json;

namespace FloodOnlineReportingTool.RiskManagement.Components.Pages.FloodEvent;

public partial class PropertySummary(
    ILogger<PropertySummary> logger)
{
    private IReadOnlyCollection<GdsBreadcrumb> Breadcrumbs { get; set; } = [];
    private string Title => EventPages.PropertySummary.Title;

    [CascadingParameter]
    public Task<AuthenticationState>? AuthenticationState { get; set; }
    private bool _isLoading = true;
    [Parameter]
    public required string Reference { get; set; }

    private readonly CancellationTokenSource _cts = new();

    private IReadOnlyCollection<(int Index, int? ParentIndex, (string Classification, int Premises) Data)> premisesTotals = [];

    protected override async Task OnInitializedAsync()
    {
        Breadcrumbs = CreateBreadcrumbs();

        // Don't get any data if you don't have permission.
        AuthenticationState? authState;
        if (AuthenticationState is null)
        {
            logger.LogWarning("Attempt to access flood event property summary without authentication");
            return;
        }
        else
        {
            authState = await AuthenticationState;
            if (!authState.User.IsAuthenticated)
            {
                logger.LogWarning("Attempt to access flood event property summary without authentication");
                return;
            }
        }

        //TODO:
        //Using the bounding polygon associated with whatever model we pass in, call the GeoAPI (places/Get Address Summary) to get all affected properties as GeoJSON
        //Convert the GeoJSON into an object or dynamic.
        //From this object extract a hierarchical list of premises types and number of affected properties per type (as per below hard-coded example).
        // This should build the list such that tertiary claassifications are children of secondary classifications are children of primary classifications.

        premisesTotals = [
            (1, null, ("Dual Use", 1)),
            (2, null, ("Commercial", 20)),
            (3, 2, ("Retail", 13)),
            (4, 3, ("Shop / Showroom", 9)),
            (5, 3, ("Retail Service Agent", 1)),
            (6, 3, ("Restaurant / Cafeteria", 1)),
            (7, 3, ("Public House / Bar / Nightclub", 2)),
            (8, 2, ("Hotel / Motel / Boarding / Guest House", 5)),
            (9, 8, ("Holiday Let/Accomodation/Short-Term Let Other Than CH01", 5)),
            (10, 2, ("Leisure - Applicable to recreational sites and enterprises", 2)),
            (11, 10, ("Museum / Gallery", 2)),
            (12, null, ("Residential", 26)),
            (13, 12, ("Dwelling", 26)),
            (14, 13, ("Detached", 1)),
            (15, 13, ("Terraced", 19)),
            (16, 13, ("Semi-Detached", 1)),
            (17, 13, ("Self Contained Flat (Includes Maisonette / Apartment)", 5)),
        ];

        _isLoading = false;

        //TODO: This may be useful if we are trying to extract premises totals from a dynamic, but delete it if it doesn't turn out to be the way we go.
        int TryGetPremises(dynamic? d)
        {
            try { return d is null ? 0 : d.Premises; }
            catch (RuntimeBinderException) { return 0; }
        }
    }

    private IReadOnlyCollection<GdsBreadcrumb> CreateBreadcrumbs()
    {
        return
        [
            GeneralPages.Home.ToGdsBreadcrumb(),
            new ($"{EventPages.View.Url}/{Reference}", $"Flood event {Reference}"),
            new ($"{EventPages.PropertySummary.Url}/{Reference}", "Property Summary")
        ];
    }
}