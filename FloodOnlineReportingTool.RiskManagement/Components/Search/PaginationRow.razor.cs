using Microsoft.AspNetCore.Components;

namespace FloodOnlineReportingTool.RiskManagement.Components.Search;

public partial class PaginationRow
{
    [Parameter, EditorRequired]
    public int CurrentPage { get; set; }

    [Parameter, EditorRequired]
    public int LastPage { get; set; }

    [Parameter, EditorRequired]
    public EventCallback<int> OnPageChanged { get; set; }

    private IReadOnlyList<int?> _pageNumbers = [];

    protected override void OnParametersSet()
    {
        _pageNumbers = GeneratePageNumbers();
    }

    private async Task OnPreviousPage()
    {
        var pageNumber = Math.Max(1, CurrentPage - 1);
        await OnPageChanged.InvokeAsync(pageNumber);
    }

    private async Task OnNextPage()
    {
        var pageNumber = Math.Min(LastPage, CurrentPage + 1);
        await OnPageChanged.InvokeAsync(pageNumber);
    }

    /// <summary>
    /// Work out the page numbers and ellipses to show.
    /// <see href="https://design-system.service.gov.uk/components/pagination/" />
    /// </summary>
    /// <example>
    /// [1] 2 … 100
    /// 1 [2] 3 … 100
    /// 1 2 [3] 4 … 100
    /// 1 2 3 [4] 5 … 100
    /// 1 … 4 [5] 6 … 100
    /// 1 … 97 [98] 99 100
    /// 1 … 98 [99] 100
    /// 1 … 99 [100]
    /// </example>
    private IReadOnlyList<int?> GeneratePageNumbers()
    {
        if (LastPage <= 1)
        {
            return LastPage == 1 ? [1] : [];
        }

        // Always show first page
        ICollection<int?> pages = [1];

        // Calculate the range of pages to show around current page
        int rangeStart = Math.Max(2, CurrentPage - 1);
        int rangeEnd = Math.Min(LastPage - 1, CurrentPage + 1);

        // Add leading ellipsis if there's a gap between page 1 and the range
        if (rangeStart > 2)
        {
            pages.Add(null);
        }

        // Add the range of pages around current page
        for (int i = rangeStart; i <= rangeEnd; i++)
        {
            pages.Add(i);
        }

        // Add trailing ellipsis if there's a gap between the range and last page
        if (rangeEnd < LastPage - 1)
        {
            pages.Add(null);
        }

        // Always show last page (if it's different from first page)
        if (LastPage > 1)
        {
            pages.Add(LastPage);
        }

        return [.. pages];
    }
}
