using FloodOnlineReportingTool.RiskManagement.Data.Models.Lookups;
using FloodOnlineReportingTool.RiskManagement.Data.Repositories;
using GdsBlazorComponents;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using System.Web;

namespace FloodOnlineReportingTool.RiskManagement.Services;

public class GeoServerClient(HttpClient httpClient, ILogger<GeoServerClient> logger, ICommonRepository commonRepository) : IGeoServerClient
{
    // GeoJSON4STJ requires JsonSerializerOptions with NTS converters
    private static readonly JsonSerializerOptions _geoJsonOptions = new()
    {
        Converters = { new NetTopologySuite.IO.Converters.GeoJsonConverterFactory() }
    };

    public async Task<IReadOnlyCollection<GdsOptionItem<string>>> SearchNameAsync(string apiSource, string searchTerm, CancellationToken ct)
    {
        var layerConfig = await commonRepository.GetDownstreamApiLayers(apiSource, ct);
        var stopwatch = Stopwatch.StartNew();
        
        if (layerConfig == null)
        {
            logger.LogError("No GeoServer API {ApiSource} layer configuration found in the database.", apiSource);
            return new List<GdsOptionItem<string>>();
        }

        try
        {
            var cqlFilter = ConstructSearchCql(searchTerm, layerConfig, false);
            string requestUri = ConstructSearchRequestUri(layerConfig, cqlFilter, true);
            
            if (string.IsNullOrEmpty(requestUri))
            {
                logger.LogError("Failed to construct valid API call for {ApiSource} layer.", apiSource);
                return new List<GdsOptionItem<string>>();
            }

            var httpResponse = await GetGeoServerWebRequest(requestUri, ct, null);
            if (httpResponse == null) return new List<GdsOptionItem<string>>();

            // Deserialize using NetTopologySuite
            var jsonString = await httpResponse.Content.ReadAsStringAsync(ct);
            var featureCollection = JsonSerializer.Deserialize<FeatureCollection>(jsonString, _geoJsonOptions);

            if (featureCollection == null || featureCollection.Count == 0)
            {
                return new List<GdsOptionItem<string>>();
            }

            // Extract feature identifiers
            var results = featureCollection
                .Select(f => f.Attributes?[layerConfig.FeatureIdentifier]?.ToString())
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                .ToList()!;

            logger.LogInformation("GeoServer query completed {ApiSource} search in {ElapsedMs}ms, returned {Count} results",
                apiSource, stopwatch.ElapsedMilliseconds, results.Count);

            var resultList = results.OfType<string>().ToList();
            return CreateGdsOptionItems(resultList);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while searching for {ApiSource} names in GeoServer.", apiSource);
            return new List<GdsOptionItem<string>>();
        }
    }

    public async Task<Geometry?> GetGeometryFromSourceBySearch(string apiSource, string searchTerm, CancellationToken ct)     
    {
        var layerConfig = await commonRepository.GetDownstreamApiLayers(apiSource, ct);
        var stopwatch = Stopwatch.StartNew();
        
        if (layerConfig == null)
        {
            logger.LogError("No GeoServer API {ApiSource} layer configuration found in the database.", apiSource);
            return null;
        }

        try
        {
            var cqlFilter = ConstructSearchCql(searchTerm, layerConfig, true);
            string requestUri = ConstructGeomRequestUri(layerConfig, cqlFilter);
            
            if (string.IsNullOrEmpty(requestUri))
            {
                logger.LogError("Failed to construct valid geometry API call for {ApiSource} layer.", apiSource);
                return null;
            }

            var httpResponse = await GetGeoServerWebRequest(requestUri, ct, null);
            if (httpResponse == null) return null;

            // Deserialize using NetTopologySuite
            var jsonString = await httpResponse.Content.ReadAsStringAsync(ct);
            var featureCollection = JsonSerializer.Deserialize<FeatureCollection>(jsonString, _geoJsonOptions);


            var firstFeature = featureCollection?.FirstOrDefault();
            if (firstFeature?.Geometry != null)
            {
                logger.LogInformation("GeoServer geometry found for {ApiSource}, type: {GeomType}",
                    apiSource, firstFeature.Geometry.GeometryType);

                logger.LogInformation("GeoServer geometry query completed in {ElapsedMs}ms",
                    stopwatch.ElapsedMilliseconds);

                return firstFeature.Geometry;
            }
            else
            {
                logger.LogWarning("No geometry found in GeoServer response for {ApiSource}", apiSource);
                return null;
            }
        } 
        catch (Exception ex) 
        {
            logger.LogError(ex, "An error occurred getting geometry from {ApiSource}.", apiSource);
            return null;
        }
    }

    private string ConstructSearchCql(string searchTerm, DownstreamApiLayer layerConfig, bool exactMatch)
    {
        // Build CQL filter for case-insensitive LIKE query that matches anywhere in the string
        return string.IsNullOrWhiteSpace(searchTerm)
            ? $"{layerConfig.FeatureIdentifier} IS NOT NULL"
            : exactMatch
                ? $"strToLowerCase({layerConfig.FeatureIdentifier}) = '{EscapeCql(searchTerm.ToLowerInvariant())}'"
                : $"strToLowerCase({layerConfig.FeatureIdentifier}) LIKE '%{EscapeCql(searchTerm.ToLowerInvariant())}%'";
    }  
    
    private string ConstructSearchRequestUri(DownstreamApiLayer layerConfig, string cqlFilter, bool limitToIdentifier)
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

    private string ConstructGeomRequestUri(DownstreamApiLayer layerConfig, string cqlFilter)
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
                    ["maxFeatures"] = "1",
                    ["CQL_FILTER"] = cqlFilter,
                    ["propertyName"] = "geom"
                };

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
}