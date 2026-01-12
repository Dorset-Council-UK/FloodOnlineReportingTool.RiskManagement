using Microsoft.AspNetCore.Components;

namespace FloodOnlineReportingTool.RiskManagement.Components.Search;

public partial class PagingNavigation(ILogger<PagingNavigation> logger)
{
    // Fields
    private bool _isFirstPage;
    private bool _isLastPage;
    private int _previousPageNumber;
    private int _nextPageNumber;

    // Properties
    [Parameter, EditorRequired]
    public int CurrentPage { get; set; }

    [Parameter, EditorRequired]
    public int TotalPages { get; set; }

    [Parameter, EditorRequired]
    public EventCallback<int> OnPageChanged { get; set; }

    protected override void OnParametersSet()
    {
        CurrentPage = Math.Max(1, CurrentPage);
        TotalPages = Math.Max(1, TotalPages);

        _isFirstPage = CurrentPage == 1;
        _isLastPage = CurrentPage >= TotalPages;

        _previousPageNumber = Math.Max(1, CurrentPage - 1);
        _nextPageNumber = Math.Min(TotalPages, CurrentPage + 1);
    }

    private async Task PreviousPage()
    {
        logger.LogInformation("Going to previous page: {PreviousPageNumber} of {TotalPages}", _previousPageNumber, TotalPages);
        await OnPageChanged.InvokeAsync(_previousPageNumber);
    }

    private async Task NextPage()
    {
        logger.LogInformation("Going to next page: {NextPageNumber} of {TotalPages}", _nextPageNumber, TotalPages);
        await OnPageChanged.InvokeAsync(_nextPageNumber);
    }
}
