namespace FloodOnlineReportingTool.RiskManagement.Options;

public record KeyVaultOptions
{
    public const string SectionName = "KeyVault";

    public string VaultName { get; init; } = "";
    public string SharedVaultName { get; init; } = "";
    public KeyVaultAzureAdOptions AzureAd { get; init; } = new();
}
