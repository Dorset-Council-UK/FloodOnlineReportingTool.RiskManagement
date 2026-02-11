using GdsBlazorComponents;

namespace FloodOnlineReportingTool.RiskManagement.Models.Order;

internal interface IPageOrder
{
    string Title { get; set; }
    IReadOnlyCollection<GdsBreadcrumb> Breadcrumbs { get; set; }
}
