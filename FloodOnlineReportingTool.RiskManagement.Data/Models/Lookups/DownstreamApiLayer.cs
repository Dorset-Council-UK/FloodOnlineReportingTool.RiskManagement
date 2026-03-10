using FloodOnlineReportingTool.RiskManagement.Data.Models.Enums;

namespace FloodOnlineReportingTool.RiskManagement.Data.Models.Lookups;

public record DownstreamApiLayer
{
    public DownstreamApiLayer(string id, bool enabled, bool secureLayer, string apiSource, string apiPath, string layerName, string featureIdentifier, RequestMethods requestMethod, RequestVersions requestVersion, string orderBy, SortOptions sortOrder)
    {
        Id = id;
        Enabled = enabled;
        SecureLayer = secureLayer;
        ApiSource = apiSource;
        ApiPath = apiPath;
        LayerName = layerName;
        FeatureIdentifier = featureIdentifier;
        RequestMethod = requestMethod;
        RequestVersion = requestVersion;
        OrderBy = orderBy;
        SortOrder = sortOrder;
    }
    public string Id { get; init; }
    public bool Enabled { get; init; } = false;
    public bool SecureLayer { get; init; } = false;
    public string ApiSource { get; init; } 
    public string ApiPath { get; init; }
    public string LayerName { get; init; }
    public string FeatureIdentifier { get; init; }
    public RequestMethods RequestMethod { get; init; } = RequestMethods.WFS;
    public RequestVersions RequestVersion { get; init; } = RequestVersions.V1_0_0;

    public string OrderBy { get; init; } = string.Empty;
    public SortOptions SortOrder { get; init; } = SortOptions.Ascending;
}
