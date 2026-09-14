using Microsoft.Identity.Web;

namespace FloodOnlineReportingTool.RiskManagement.Options;

public record KeyVaultAzureAdOptions
{
    public const string SectionName = Constants.AzureAd;

    public bool UseDefaultCredential { get; init; }
    public string ApplicationId { get; init; } = "";
    public string CertificateThumbprint { get; init; } = "";
    public string DirectoryId { get; init; } = "";
}
