using FloodOnlineReportingTool.RiskManagement.Data.Models.Lookups;
using FloodOnlineReportingTool.RiskManagement.Data.Repositories;
using FloodOnlineReportingTool.RiskManagement.Options;
using GdsBlazorComponents;
using Microsoft.Identity.Abstractions;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using System.Diagnostics;
using System.Globalization;
using System.Security.Claims;
using System.Text.Json;
using System.Web;

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

        if (layerConfig == null || !layerConfig.Enabled)
        {
            logger.LogError("No GeoServer API {ApiSource} layer configuration found or is disabled", apiSource);
            return new List<GdsOptionItem<string>>();
        }

        try
        {
            var cqlFilter = ConstructSearchCql(searchTerm, layerConfig, false);
            string requestPath = ConstructSearchRequestUri(layerConfig, cqlFilter, "100", true, false);

            if (string.IsNullOrEmpty(requestPath))
            {
                logger.LogError("Failed to construct valid API call for {ApiSource}", apiSource);
                return new List<GdsOptionItem<string>>();
            }

            var featureCollection = await CallGeoServerApi(requestPath, layerConfig, user, ct)
                ?? new FeatureCollection();

            var results = ExtractFeatureNames(featureCollection, layerConfig.FeatureIdentifier).ToList();

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
        var stopwatch = Stopwatch.StartNew();
        var layerConfig = await commonRepository.GetDownstreamApiLayers(apiSource, ct);

        if (layerConfig == null || !layerConfig.Enabled)
        {
            logger.LogError("No GeoServer API {ApiSource} layer configuration found or is disabled", apiSource);
            return null;
        }

        try
        {
            var cqlFilter = ConstructSearchCql(searchTerm, layerConfig, true);
            string requestPath = ConstructSearchRequestUri(layerConfig, cqlFilter, "1", false, true);

            if (string.IsNullOrEmpty(requestPath))
            {
                logger.LogError("Failed to construct geometry API call for {ApiSource}", apiSource);
                return null;
            }

            var featureCollection = await CallGeoServerApi(requestPath, layerConfig, user, ct);
            var geometry = ExtractFirstGeometry(featureCollection);

            if (geometry != null)
            {
                logger.LogInformation("GeoServer geometry found for {ApiSource} in {ElapsedMs}ms, type: {GeomType}",
                    apiSource, stopwatch.ElapsedMilliseconds, geometry.GeometryType);
            }
            else
            {
                logger.LogWarning("No geometry found for {ApiSource} (user may lack permission)", apiSource);
            }

            return geometry;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting geometry from {ApiSource}", apiSource);
            return null;
        }
    }

    private async Task<FeatureCollection?> CallGeoServerApi(
        string requestPath,
        DownstreamApiLayer layerConfig,
        ClaimsPrincipal? user,
        CancellationToken ct)
    {
        HttpResponseMessage? httpResponse;

        if (layerConfig.SecureLayer && user?.Identity?.IsAuthenticated == true)
        {
            // Authenticated call - respects Geofence user-based access control
            httpResponse = await downstreamApi.GetForUserAsync<HttpResponseMessage>(
                DownstreamAPIOptions.GeoServerApi,
                options => { options.RelativePath = requestPath; },
                user: user,
                cancellationToken: ct);
        }
        else
        {
            // Anonymous call for public layers
            httpResponse = await downstreamApi.CallApiAsync(
                DownstreamAPIOptions.GeoServerApiPublic,
                options => { options.RelativePath = requestPath; },
                cancellationToken: ct);
        }

        if (httpResponse == null)
        {
            logger.LogWarning("GeoServer API {ApiSource} returned null response", layerConfig.ApiSource);
            return null;
        }

        using (httpResponse)
        {
            return await ProcessResponse(httpResponse, layerConfig.ApiSource, ct);
        }
    }

    private async Task<FeatureCollection?> ProcessResponse(
        HttpResponseMessage httpResponse,
        string apiSource,
        CancellationToken ct)
    {
        if (!httpResponse.IsSuccessStatusCode)
        {
            logger.LogWarning("GeoServer API {ApiSource} request returned {StatusCode}",
                apiSource, httpResponse.StatusCode);
            return null;
        }

        if (httpResponse.Content == null) return null;

        var stream = await httpResponse.Content.ReadAsStreamAsync(ct);
        var featureCollection = JsonSerializer.Deserialize<FeatureCollection>(stream, _geoJsonOptions);

        if (featureCollection == null || featureCollection.Count == 0)
        {
            logger.LogWarning("GeoServer API {ApiSource} returned no data", apiSource);
            return null;
        }

        return featureCollection;
    }

    private static Geometry? ExtractFirstGeometry(FeatureCollection? featureCollection)
    {
        return featureCollection?.FirstOrDefault()?.Geometry;
    }

    private static IEnumerable<string> ExtractFeatureNames(
        FeatureCollection featureCollection,
        string featureIdentifier)
    {
        return featureCollection
            .Select(f => f.Attributes?[featureIdentifier]?.ToString())
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
            .OfType<string>();
    }

    private static string ConstructSearchCql(
        string searchTerm,
        DownstreamApiLayer layerConfig,
        bool exactMatch)
    {
        return string.IsNullOrWhiteSpace(searchTerm)
            ? $"{layerConfig.FeatureIdentifier} IS NOT NULL"
            : exactMatch
                ? $"strToLowerCase({layerConfig.FeatureIdentifier}) = '{EscapeCql(searchTerm.ToLowerInvariant())}'"
                : $"strToLowerCase({layerConfig.FeatureIdentifier}) LIKE '%{EscapeCql(searchTerm.ToLowerInvariant())}%'";
    }

    private static string ConstructSearchRequestUri(
        DownstreamApiLayer layerConfig,
        string cqlFilter,
        string maxFeatures,
        bool limitToIdentifier,
        bool limitToGeometry)
    {
        if (layerConfig.RequestMethod != Data.Models.Enums.RequestMethods.WFS)
        {
            return string.Empty;
        }

        string wfsPath = layerConfig.ApiPath;
        Dictionary<string, string> queryParams = layerConfig.RequestVersion switch
        {
            // Primary difference between  1.0.0 and 1.1.0 is output format 
            // where 1.0.0 is GML2 and 1.1.0 is GML3. 
            // Some paging is supported in 1.1.0 but we don't use is for any version 1
            // Input parameters are different for v2.0.0 and paging is supported.
            Data.Models.Enums.RequestVersions.V1_0_0 or Data.Models.Enums.RequestVersions.V1_1_0 =>
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["service"] = "WFS",
                    ["version"] = layerConfig.RequestVersion == Data.Models.Enums.RequestVersions.V1_0_0 ? "1.0.0" : "1.1.0",
                    ["request"] = "GetFeature",
                    ["typeName"] = layerConfig.LayerName,
                    ["outputFormat"] = "application/json",
                    ["maxFeatures"] = maxFeatures,
                    ["CQL_FILTER"] = cqlFilter
                },
            Data.Models.Enums.RequestVersions.V2_0_0 =>
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["service"] = "WFS",
                    ["version"] = "2.0.0",
                    ["request"] = "GetFeature",
                    ["typeNames"] = layerConfig.LayerName,
                    ["outputFormat"] = "application/json",
                    ["count"] = maxFeatures,
                    ["CQL_FILTER"] = cqlFilter
                },
            _ => throw new InvalidOperationException($"Unsupported WFS version: {layerConfig.RequestVersion}")
        };

        if (limitToIdentifier)
        {
            queryParams["propertyName"] = layerConfig.FeatureIdentifier;
        } else if (limitToGeometry)
        {
            queryParams["propertyName"] = "geom";
        }

        var queryString = string.Join("&", queryParams.Select(kvp => $"{kvp.Key}={HttpUtility.UrlEncode(kvp.Value)}"));

        return $"{wfsPath}?{queryString}";
    }

    private static List<GdsOptionItem<string>> CreateGdsOptionItems(IEnumerable<string> results)
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
        return input
            .Replace("'", "''")
            .Replace("%", "\\%")
            .Replace("_", "\\_");
    }
}