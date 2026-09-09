using FloodOnlineReportingTool.RiskManagement.Models.Order;
using FloodOnlineReportingTool.RiskManagement.Models.Event;
using FloodOnlineReportingTool.RiskManagement.Services;
using FloodOnlineReportingTool.Database.Repositories;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.CSharp.RuntimeBinder;
using System.Security.Claims;
using NetTopologySuite.IO.Converters;
using System.Text.Json;

namespace FloodOnlineReportingTool.RiskManagement.Components.Pages.FloodEvent;

public partial class PropertySummary(
    IFloodEventRepository floodEventRepository,
    IAzureApiClient azureApiClient,
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
    private EventOverviewViewModel? ViewModel { get; set; }

    private readonly JsonSerializerOptions _jsonSerializerOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new GeoJsonConverterFactory() },
    };

    private IReadOnlyCollection<(int Index, int? ParentIndex, (string Classification, int Premises) Data)> premisesTotals = [];
    private IReadOnlyCollection<(int Index, int? ParentIndex, (string Classification, int Premises) Data)> fakePremisesTotals =
    [
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
        // This should build the list such that tertiary classifications are children of secondary classifications are children of primary classifications.

        try
        {
            var eventResult = await floodEventRepository.GetBySearchReference(Reference, 4326, _cts.Token);
            if (!eventResult.IsSuccess || eventResult.ResultModel is null)
            {
                logger.LogWarning("No flood event found for reference {Reference}", Reference);
                return;
            }

            ViewModel = new EventOverviewViewModel
            {
                SearchReference = eventResult.ResultModel.SavedSearch.Reference,
                Geom = eventResult.ResultModel.SavedSearch.Geom
            };

            if (ViewModel.Geom is null)
            {
                logger.LogWarning("Flood event {Reference} has no geometry to summarise", Reference);
                return;
            }

            var area = JsonSerializer.Serialize(ViewModel.Geom, _jsonSerializerOptions);
            premisesTotals = await azureApiClient.GetAddressSummary(4326, "dorset", area, _cts.Token) ?? [];
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Failed to call address summary API for {Reference}", Reference);
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "Failed to serialize geometry or deserialize address summary for {Reference}", Reference);
        }
        finally
        {
            _isLoading = false;
        }

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