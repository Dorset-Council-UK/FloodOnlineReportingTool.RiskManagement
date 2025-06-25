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

    [Parameter]
    public string CardStyle { get; set; } = "info";

    private string? SizeOption { get; set; }
    private string? SelectedStyle { get; set; }

    internal virtual string CssClass(string _cssType)
    {
        return _cssType switch
        {
            "OneHalf" => "govuk-grid-column-one-half",
            "OneThird" => "govuk-grid-column-one-third",
            _ => string.Empty,
        };
    }

    internal virtual string CardOptions(string _inputStyle)
    {
        return _inputStyle switch
        {
            "info" => ".info_panel__item__default",
            "success" => ".info_panel__item__success",
            "warning" => ".info_panel__item__warning",
            "danger" => ".info_panel__item__danger",
            _ => string.Empty,
        };
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

        SelectedStyle = CardOptions(CardStyle);
    }
}