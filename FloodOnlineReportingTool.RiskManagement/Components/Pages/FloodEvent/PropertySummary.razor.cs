using FloodOnlineReportingTool.RiskManagement.Models.Order;
using FloodOnlineReportingTool.RiskManagement.Models.Event;
using FloodOnlineReportingTool.RiskManagement.Authorization;
using FloodOnlineReportingTool.RiskManagement.Services;
using FloodOnlineReportingTool.Database.Repositories;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Authorization;
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
    IAuthorizationService authorizationService,
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

    private IReadOnlyCollection<(int Index, int? ParentIndex, (string Classification, int Premises) Data)>? premisesTotals;
    
    protected override async Task OnInitializedAsync()
    {
        Breadcrumbs = CreateBreadcrumbs();

        var isAuthorizedReader = await AuthenticationState.TryIsReaderAsync(authorizationService);
        if (!isAuthorizedReader)
        {
            logger.LogWarning("Attempt to access flood event property summary without reader permission");
            _isLoading = false;
            return;
        }

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
            premisesTotals = await azureApiClient.GetAddressSummary(4326, "dorset", area, _cts.Token);
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