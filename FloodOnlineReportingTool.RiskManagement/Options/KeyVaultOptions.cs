namespace FloodOnlineReportingTool.RiskManagement.Options;

public record KeyVaultOptions
{
    public const string SectionName = "KeyVault";

    public string Name { get; init; } = "";
    public string SharedVault { get; init; } = "";
    public KeyVaultAzureAdOptions AzureAd { get; init; } = new();
}
