using FloodOnlineReportingTool.RiskManagement.Data.Models;

namespace FloodOnlineReportingTool.RiskManagement.Services
{
    public interface IApiService
    {
        Task<FloodReport?> GetReportByIdAsync(string id, CancellationToken cancellationToken = default);
    }
}