using FloodOnlineReportingTool.RiskManagement.Models;

namespace FloodOnlineReportingTool.RiskManagement.Services
{
    public interface ISettingsService
    {
        Task AddRecentWork(string entityId, string entityType, string entityDescription, string url);
        Task<IList<RecentWork>> GetRecentWork();
    }
}