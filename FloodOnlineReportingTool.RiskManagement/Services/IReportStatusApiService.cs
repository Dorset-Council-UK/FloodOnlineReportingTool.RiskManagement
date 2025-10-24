namespace FloodOnlineReportingTool.RiskManagement.Services;

public interface IReportStatusApiService
{
    Task Test(CancellationToken cancellationToken = default);
}