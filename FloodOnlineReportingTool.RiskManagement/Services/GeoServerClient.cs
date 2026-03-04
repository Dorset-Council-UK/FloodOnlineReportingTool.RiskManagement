using FloodOnlineReportingTool.RiskManagement.Data.Models.Lookups;
using FloodOnlineReportingTool.RiskManagement.Data.Repositories;
using GdsBlazorComponents;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Logging;
using Polly;
using System.Diagnostics;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;

namespace FloodOnlineReportingTool.RiskManagement.Services;

public class GeoServerClient(HttpClient httpClient, ILogger<GeoServerClient> logger, ICommonRepository commonRepository) : IGeoServerClient
{
    public async Task<IReadOnlyCollection<GdsOptionItem<string>>> SearchParishNameAsync(string searchTerm, CancellationToken ct = default)
    {
        // Get the Layer details from the database and fail if missing
        DownstreamApiLayer? layerConfig = await commonRepository.GetDownstreamApiLayers("parish", ct);

        var stopwatch = Stopwatch.StartNew();
        if (layerConfig == null)
        {
            logger.LogError("No GeoServer API parish layer configuration found in the database.");
            return new List<GdsOptionItem<string>>();
        }

        try
        {
            var cqlFilter = ContructSearchCql(searchTerm, layerConfig);
            string requestUri = ConstructRequestUri(layerConfig, cqlFilter, true);
            if (requestUri == string.Empty)
            {
                logger.LogError("Failed to construct valid API call for parishes layer.");
                return new List<GdsOptionItem<string>>();
            }

            // HttpClient will automatically include the Bearer token via GeoServerTokenHandler
            var httpResponse = await GetGeoServerWebRequest(requestUri, ct, null);

            if (httpResponse == null)
            {
                return new List<GdsOptionItem<string>>();
            }
            var response = await httpResponse.Content.ReadFromJsonAsync<GeoJsonFeatureCollection>(ct);
            if (response?.Features == null)
            {
                return new List<GdsOptionItem<string>>();
            }

            // Extract feature identifiers, remove duplicates, and sort
            var results = response.Features
                .Select(f => f.Properties?.TryGetProperty(layerConfig.FeatureIdentifier, out var name) == true ? name.GetString() : null)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(name => layerConfig.OrderBy, StringComparer.OrdinalIgnoreCase)
                .ToList()!;

            logger.LogInformation("GeoServer query completed parish search in {ElapsedMs}ms, returned {Count} results",
                stopwatch.ElapsedMilliseconds, results.Count);

            // Create option items with "Select an option" as the first option
            List<string> resultList = results.OfType<string>().ToList();
            var optionItems = CreateGdsOptionItems(resultList);

            return optionItems;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while searching for parish names in GeoServer.");
            return new List<GdsOptionItem<string>>();
        }
    }

    public async Task<IReadOnlyCollection<GdsOptionItem<string>>> SearchWardNameAsync(string searchTerm, CancellationToken ct = default)
    {
        // Get the Layer details from the database and fail if missing
        DownstreamApiLayer? layerConfig = await commonRepository.GetDownstreamApiLayers("ward", ct);

        var stopwatch = Stopwatch.StartNew();
        if (layerConfig == null)
        {
            logger.LogError("No GeoServer API ward layer configuration found in the database.");
            return new List<GdsOptionItem<string>>();
        }

        try
        {
            var cqlFilter = ContructSearchCql(searchTerm, layerConfig);
            string requestUri = ConstructRequestUri(layerConfig, cqlFilter, true);
            if (requestUri == string.Empty)
            {
                logger.LogError("Failed to construct valid API call for wards layer.");
                return new List<GdsOptionItem<string>>();
            }

            // HttpClient will automatically include the Bearer token via GeoServerTokenHandler
            var httpResponse = await GetGeoServerWebRequest(requestUri, ct, null);

            if (httpResponse == null)
            {
                return new List<GdsOptionItem<string>>();
            }
            var response = await httpResponse.Content.ReadFromJsonAsync<GeoJsonFeatureCollection>(ct);
            if (response?.Features == null)
            {
                return new List<GdsOptionItem<string>>();
            }

            // Extract feature identifiers, remove duplicates, and sort
            var results = response.Features
                .Select(f => f.Properties?.TryGetProperty(layerConfig.FeatureIdentifier, out var name) == true ? name.GetString() : null)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(name => layerConfig.OrderBy, StringComparer.OrdinalIgnoreCase)
                .ToList()!;

            logger.LogInformation("GeoServer query completed ward search in {ElapsedMs}ms, returned {Count} results",
                stopwatch.ElapsedMilliseconds, results.Count);

            // Create option items with "Select an option" as the first option
            List<string> resultList = results.OfType<string>().ToList();
            var optionItems = CreateGdsOptionItems(resultList);

            return optionItems;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while searching for ward names in GeoServer.");
            return new List<GdsOptionItem<string>>();
        }
    }

    private string ContructSearchCql(string searchTerm, DownstreamApiLayer layerConfig)
    {
        // Build CQL filter for case-insensitive LIKE query that matches anywhere in the string
        return string.IsNullOrWhiteSpace(searchTerm)
            ? $"{layerConfig.FeatureIdentifier} IS NOT NULL"
            : $"strToLowerCase({layerConfig.FeatureIdentifier}) LIKE '%{EscapeCql(searchTerm.ToLowerInvariant())}%'";
    }  
    
    private string ConstructRequestUri(DownstreamApiLayer layerConfig, string cqlFilter, bool limitToIdentifier)
    {
        string requestUri = string.Empty;
        if (layerConfig.RequestMethod == Data.Models.Enums.RequestMethods.WFS)
        {
            if (layerConfig.RequestVersion == Data.Models.Enums.RequestVersions.V1_0_0)
            {
                string WfsPath = $"{layerConfig.ApiPath}/ows";

                var queryParams = new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["service"] = "WFS",
                    ["version"] = "1.1.0",
                    ["request"] = "GetFeature",
                    ["typeName"] = layerConfig.LayerName,
                    ["outputFormat"] = "application/json",
                    ["maxFeatures"] = "100",
                    ["CQL_FILTER"] = cqlFilter
                };

                if (limitToIdentifier)
                {
                    // Only return the feature ID value for efficiency
                    queryParams["propertyName"] = layerConfig.FeatureIdentifier;
                }

                var queryString = string.Join("&", queryParams.Select(kvp =>
                    $"{kvp.Key}={HttpUtility.UrlEncode(kvp.Value)}"));

                requestUri = $"{WfsPath}?{queryString}";
            }
        }
        return requestUri;
    }

    private List<GdsOptionItem<string>> CreateGdsOptionItems(List<string> results)
    {
        var optionItems = new List<GdsOptionItem<string>>
            {
                new GdsOptionItem<string>("0".AsSpan(), "Select an option", string.Empty, false)
            };
        optionItems.AddRange(results.Select(static (o, idx) => 
            new GdsOptionItem<string>((idx + 1).ToString(CultureInfo.InvariantCulture).AsSpan(), o, o, false)));

        return optionItems;
    }

    private static string EscapeCql(string input)
    {
        // Escape single quotes for CQL and remove any special regex characters
        return input
            .Replace("'", "''")
            .Replace("%", "\\%")
            .Replace("_", "\\_");
    }

    private async Task<HttpResponseMessage?> GetGeoServerWebRequest(string requestUri, CancellationToken ct, object? postBody)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            HttpResponseMessage? response = null;
            if (postBody != null)
            {
                HttpContent content = JsonContent.Create(postBody);
                response = await httpClient.PostAsync(requestUri, content, ct);
            } else
            {
                response = await httpClient.GetAsync(requestUri, ct);
            }
            response.EnsureSuccessStatusCode();

            return response;
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            logger.LogWarning("GeoServer request timed out after {ElapsedMs}ms",
                stopwatch.ElapsedMilliseconds);
            return null;
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "Failed to query GeoServer");
            return null;
        }
    }

    // GeoJSON response models
    private record GeoJsonFeatureCollection(
        [property: JsonPropertyName("features")] List<GeoJsonFeature>? Features
    );
    
    private record GeoJsonFeature(
        [property: JsonPropertyName("properties")] JsonElement? Properties
    );
   
}