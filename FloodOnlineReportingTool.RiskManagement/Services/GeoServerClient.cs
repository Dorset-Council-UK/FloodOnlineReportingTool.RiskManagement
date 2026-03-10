using FloodOnlineReportingTool.RiskManagement.Data.Repositories;
using GdsBlazorComponents;
using Microsoft.Identity.Abstractions;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
using System.Web;
using FloodOnlineReportingTool.RiskManagement.Data.Models.Lookups;
using FloodOnlineReportingTool.RiskManagement.Options;
using System.Security.Claims;

namespace FloodOnlineReportingTool.RiskManagement.Services;

public class GeoServerClient(
    IDownstreamApi downstreamApi,
    ICommonRepository commonRepository,
    ILogger<GeoServerClient> logger) : IGeoServerClient
{
    // GeoJSON4STJ requires JsonSerializerOptions with NTS converters
    private static readonly JsonSerializerOptions _geoJsonOptions = new()
    {
        Converters = { new NetTopologySuite.IO.Converters.GeoJsonConverterFactory() }
    };

    public async Task<IReadOnlyCollection<GdsOptionItem<string>>> SearchNameAsync(
        string apiSource, 
        string searchTerm,
        ClaimsPrincipal? user,
        CancellationToken ct)
    {
        var stopwatch = Stopwatch.StartNew();
        var layerConfig = await commonRepository.GetDownstreamApiLayers(apiSource, ct);

        if (layerConfig == null)
        {
            logger.LogError("No GeoServer API {ApiSource} layer configuration found", apiSource);
            return new List<GdsOptionItem<string>>();
        }

        if (!layerConfig.Enabled)
        {
            logger.LogWarning("GeoServer API {ApiSource} layer is disabled", apiSource);
            return new List<GdsOptionItem<string>>();
        }

        try
        {
            var cqlFilter = ConstructSearchCql(searchTerm, layerConfig, false);
            string requestPath = ConstructSearchRequestUri(layerConfig, cqlFilter, true);

            if (string.IsNullOrEmpty(requestPath))
            {
                logger.LogError("Failed to construct valid API call for {ApiSource}", apiSource);
                return new List<GdsOptionItem<string>>();
            }

            FeatureCollection? featureCollection;

            // Use authenticated or anonymous call based on SecureLayer flag
            if (layerConfig.SecureLayer && user != null)
            {
                // Authenticated call - respects Geofence user-based access control
                featureCollection = await downstreamApi.GetForUserAsync<FeatureCollection>(
                    DownstreamAPIOptions.GeoServerApi,
                    options =>
                    {
                        options.RelativePath = requestPath;
                        options.Deserializer = (content) =>
                        {
                            if (content is null) return null;
                            var stream = content.ReadAsStream(ct);
                            return JsonSerializer.Deserialize<FeatureCollection>(stream, _geoJsonOptions);
                        };
                        // Add token acquisition options with login hint
                        options.AcquireTokenOptions = new()
                        {
                            Claims = user?.FindFirst("preferred_username")?.Value ?? user?.FindFirst("upn")?.Value,
                            ForceRefresh = false
                        };
                    },
                    user: user,
                    cancellationToken: ct);
            }
            else
            {
                // Anonymous call for public layers - use public API configuration with no scopes
                var httpResponse = await downstreamApi.CallApiAsync(
                    DownstreamAPIOptions.GeoServerApiPublic,  // Changed from GeoServerApi
                    options => { options.RelativePath = requestPath; },
                    cancellationToken: ct);

                if (httpResponse?.Content == null) return new List<GdsOptionItem<string>>();
                
                var stream = await httpResponse.Content.ReadAsStreamAsync(ct);
                featureCollection = JsonSerializer.Deserialize<FeatureCollection>(stream, _geoJsonOptions);
            }

            if (featureCollection?.Count == 0)
            {
                return new List<GdsOptionItem<string>>();
            }

            var results = featureCollection
                .Select(f => f.Attributes?[layerConfig.FeatureIdentifier]?.ToString())
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                .OfType<string>()
                .ToList();

            logger.LogInformation("GeoServer {ApiSource} search completed in {ElapsedMs}ms, {Count} results",
                apiSource, stopwatch.ElapsedMilliseconds, results.Count);

            return CreateGdsOptionItems(results);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error searching {ApiSource} in GeoServer", apiSource);
            return new List<GdsOptionItem<string>>();
        }
    }

    public async Task<Geometry?> GetGeometryFromSourceBySearch(
        string apiSource, 
        string searchTerm,
        ClaimsPrincipal? user,
        CancellationToken ct)
    {
        var layerConfig = await commonRepository.GetDownstreamApiLayers(apiSource, ct);
        var stopwatch = Stopwatch.StartNew();

        if (layerConfig == null)
        {
            logger.LogError("No GeoServer API {ApiSource} layer configuration found", apiSource);
            return null;
        }

        if (!layerConfig.Enabled)
        {
            logger.LogWarning("GeoServer API {ApiSource} layer is disabled", apiSource);
            return null;
        }

        try
        {
            var cqlFilter = ConstructSearchCql(searchTerm, layerConfig, true);
            string requestPath = ConstructGeomRequestUri(layerConfig, cqlFilter);

            if (string.IsNullOrEmpty(requestPath))
            {
                logger.LogError("Failed to construct geometry API call for {ApiSource}", apiSource);
                return null;
            }

            FeatureCollection? featureCollection;

            // Use authenticated or anonymous call based on SecureLayer flag
            if (layerConfig.SecureLayer && user != null)
            {
                // Authenticated call - respects Geofence user-based access control
                featureCollection = await downstreamApi.GetForUserAsync<FeatureCollection>(
                    DownstreamAPIOptions.GeoServerApi,
                    options =>
                    {
                        options.RelativePath = requestPath;
                        options.Deserializer = (content) =>
                        {
                            if (content is null) return null;
                            var stream = content.ReadAsStream(ct);
                            return JsonSerializer.Deserialize<FeatureCollection>(stream, _geoJsonOptions);
                        };
                        // Add token acquisition options with login hint
                        options.AcquireTokenOptions = new()
                        {
                            Claims = user?.FindFirst("preferred_username")?.Value ?? user?.FindFirst("upn")?.Value,
                            ForceRefresh = false
                        };
                    },
                    user: user,
                    cancellationToken: ct);
            }
            else
            {
                // Anonymous call for public layers - use public API configuration with no scopes
                var httpResponse = await downstreamApi.CallApiAsync(
                    DownstreamAPIOptions.GeoServerApiPublic,  // Changed from GeoServerApi
                    options => { options.RelativePath = requestPath; },
                    cancellationToken: ct);

                if (httpResponse?.Content == null) return null;
                
                var stream = await httpResponse.Content.ReadAsStreamAsync(ct);
                featureCollection = JsonSerializer.Deserialize<FeatureCollection>(stream, _geoJsonOptions);
            }

            var firstFeature = featureCollection?.FirstOrDefault();
            if (firstFeature?.Geometry != null)
            {
                logger.LogInformation("GeoServer geometry found for {ApiSource}, type: {GeomType}",
                    apiSource, firstFeature.Geometry.GeometryType);

                return firstFeature.Geometry;
            }

            logger.LogWarning("No geometry found for {ApiSource} (user may lack permission)", apiSource);
            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting geometry from {ApiSource}", apiSource);
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
            string WfsPath = layerConfig.ApiPath;
            Dictionary<string, string> queryParams;
            switch (layerConfig.RequestVersion)
            {
                case Data.Models.Enums.RequestVersions.V1_0_0 or Data.Models.Enums.RequestVersions.V1_1_0:
                    // Primary difference between  1.0.0 and 1.1.0 is output format 
                    // where 1.0.0 is GML2 and 1.1.0 is GML3. 
                    // Some paging is supported in 1.1.0 but we don't use is for any version 1
                    // If you need paging, use version 2.0.0
                    queryParams = new Dictionary<string, string>(StringComparer.Ordinal)
                    {
                        ["service"] = "WFS",
                        ["version"] = (layerConfig.RequestVersion == Data.Models.Enums.RequestVersions.V1_0_0 ? "1.0.0" : "1.1.0"),
                        ["request"] = "GetFeature",
                        ["typeName"] = layerConfig.LayerName,
                        ["outputFormat"] = "application/json",
                        ["maxFeatures"] = "100",
                        ["CQL_FILTER"] = cqlFilter
                    };
                    break;
                case Data.Models.Enums.RequestVersions.V2_0_0:
                    // Input parameters are different for v2.0.0 and paging is supported.
                    queryParams = new Dictionary<string, string>(StringComparer.Ordinal)
                    {
                        ["service"] = "WFS",
                        ["version"] = "2.0.0",
                        ["request"] = "GetFeature",
                        ["typeNames"] = layerConfig.LayerName,
                        ["outputFormat"] = "application/json",
                        ["count"] = "100",
                        ["CQL_FILTER"] = cqlFilter,
                    };
                    // TODO: If we need to support paging for v2.0.0, we would need to add "startIndex" parameter and handle it in the code
                    break;
                default:
                    logger.LogError("Unsupported WFS version {Version} for search request.", layerConfig.RequestVersion);
                    return string.Empty;
            }

            if (limitToIdentifier)
            {
                // Only return the feature ID value for efficiency
                queryParams["propertyName"] = layerConfig.FeatureIdentifier;
            }

            var queryString = string.Join("&", queryParams.Select(kvp =>
                $"{kvp.Key}={HttpUtility.UrlEncode(kvp.Value)}"));

            requestUri = $"{WfsPath}?{queryString}";
        }
        else
        {
            logger.LogError("Unsupported request method {RequestMethod} for layer.", layerConfig.RequestMethod);
        }
        return requestUri;
    }

    private string ConstructGeomRequestUri(DownstreamApiLayer layerConfig, string cqlFilter)
    {
        string requestUri = string.Empty;
        if (layerConfig.RequestMethod == Data.Models.Enums.RequestMethods.WFS)
        {
            string WfsPath = $"{layerConfig.ApiPath}";
            Dictionary<string, string> queryParams;
            switch (layerConfig.RequestVersion)
            {
                case Data.Models.Enums.RequestVersions.V1_0_0 or Data.Models.Enums.RequestVersions.V1_1_0:
                    // Primary difference between  1.0.0 and 1.1.0 is output format 
                    // where 1.0.0 is GML2 and 1.1.0 is GML3. 
                    // Some paging is supported in 1.1.0 but we don't use is for any version 1
                    // If you need paging, use version 2.0.0
                    queryParams = new Dictionary<string, string>(StringComparer.Ordinal)
                    {
                        ["service"] = "WFS",
                        ["version"] = (layerConfig.RequestVersion == Data.Models.Enums.RequestVersions.V1_0_0 ? "1.0.0" : "1.1.0"),
                        ["request"] = "GetFeature",
                        ["typeName"] = layerConfig.LayerName,
                        ["outputFormat"] = "application/json",
                        ["maxFeatures"] = "1",
                        ["CQL_FILTER"] = cqlFilter,
                        ["propertyName"] = "geom"
                    };
                    break;
                case Data.Models.Enums.RequestVersions.V2_0_0:
                    // Input parameters are different for v2.0.0 and paging is supported.
                    queryParams = new Dictionary<string, string>(StringComparer.Ordinal)
                    {
                        ["service"] = "WFS",
                        ["version"] = "2.0.0",
                        ["request"] = "GetFeature",
                        ["typeNames"] = layerConfig.LayerName,
                        ["outputFormat"] = "application/json",
                        ["count"] = "1",
                        ["CQL_FILTER"] = cqlFilter,
                        ["propertyName"] = "geom"
                    };
                    // TODO: If we need to support paging for v2.0.0, we would need to add "startIndex" parameter and handle it in the code
                    break;
                default:
                    logger.LogError("Unsupported WFS version {Version} for geometry request.", layerConfig.RequestVersion);
                    return string.Empty;
            }
                

            var queryString = string.Join("&", queryParams.Select(kvp =>
            $"{kvp.Key}={HttpUtility.UrlEncode(kvp.Value)}"));

            requestUri = $"{WfsPath}?{queryString}";
            
        }
        else
        {
            logger.LogError("Unsupported request method {RequestMethod} for {ApiSource} layer.", layerConfig.RequestMethod, layerConfig.ApiSource);
        }
        return requestUri;
    }

    private List<GdsOptionItem<string>> CreateGdsOptionItems(List<string> results)
    {
        var optionItems = new List<GdsOptionItem<string>>
        {
            new("0".AsSpan(), "Select an option", string.Empty, false)
        };
        optionItems.AddRange(results.Select((o, idx) =>
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
}