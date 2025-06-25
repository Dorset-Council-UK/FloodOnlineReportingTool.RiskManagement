using Microsoft.AspNetCore.Components;

namespace FloodOnlineReportingTool.RiskManagement.Components.Layout;

public partial class GridCard
{
    [Parameter]
    public string HeaderText { get; set; } = "";

    [Parameter]
    public string BodyText { get; set; } = "";

    [Parameter, EditorRequired]
    public string CardSize { get; set; }

    private string? SizeOption { get; set; }

    internal virtual string CssClass(string _cssType)
    {
        return _cssType switch
        {
            "OneHalf" => "govuk-grid-column-one-half",
            "OneThird" => "govuk-grid-column-one-third",
            _ => string.Empty,
        };
    }

    private static class InputSizeCssClasses
    {
        public const string OneHalf = "govuk-grid-column-one-half";
        public const string OneThird = "govuk-grid-column-one-third";
    }

    protected override void OnParametersSet()
    {
        if (!string.IsNullOrEmpty(CardSize))
        {
            SizeOption = CssClass(CardSize);
        } else
        {
            SizeOption = CssClass("OneThird");
        }
    }
}