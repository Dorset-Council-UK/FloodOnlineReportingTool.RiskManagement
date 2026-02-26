namespace FloodOnlineReportingTool.RiskManagement.Services;

public interface ITokenService
{
    Task<string?> GetGeoServerTokenAsync(CancellationToken ct = default);
}