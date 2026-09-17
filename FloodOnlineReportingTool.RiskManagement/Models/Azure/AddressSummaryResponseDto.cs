using System.Text.Json.Serialization;

namespace FloodOnlineReportingTool.RiskManagement.Models.Azure;

public sealed record AddressSummaryResponseDto
{
    [JsonPropertyName("Primary_Classifications")]
    public IReadOnlyDictionary<string, AddressSummaryCategoryDto>? PrimaryClassifications { get; init; }
}

public sealed record AddressSummaryCategoryDto
{
    [JsonPropertyName("Premises")]
    public int Premises { get; init; }

    [JsonPropertyName("Secondary_Classifications")]
    public IReadOnlyDictionary<string, AddressSummaryCategoryDto?>? Subcategories { get; init; }

    [JsonPropertyName("Tertiary_Classifications")]
    public IReadOnlyDictionary<string, int>? ChildCategoryCounts { get; init; }
}
