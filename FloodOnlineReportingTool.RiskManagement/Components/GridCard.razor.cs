using Microsoft.AspNetCore.Components;

namespace FloodOnlineReportingTool.RiskManagement.Components;

public partial class GridCard
{
    [Parameter]
    public string HeaderText { get; set; } = "";

    [Parameter]
    public string BodyText { get; set; } = "";

    [Parameter, EditorRequired]
    public CardSizes CardSize { get; set; }

    [Parameter]
    public string LinkURL { get; set; } = "#";

    [Parameter]
    public string CardStyle { get; set; } = "info";

    private string? SizeOption { get; set; }
    private string? SelectedStyle { get; set; }
    private string? CardTag { get; set; }

    public enum CardSizes
    {
        None,
        OneHalf,
        OneThird,
    }

    internal virtual string CssClass(CardSizes _cssType)
    {
        return _cssType switch
        {
            CardSizes.OneHalf => "govuk-grid-column-one-half",
            CardSizes.OneThird => "govuk-grid-column-one-third",
            _ => string.Empty,
        };
    }

    internal virtual string CardOptions(string _inputStyle)
    {
        return _inputStyle switch
        {
            "info" => "info-panel-info",
            "success" => "info-panel-success",
            "warning" => "info-panel-warning",
            "danger" => "info-panel-danger",
            _ => string.Empty,
        };
    }

    internal virtual string CardTags(string _inputStyle)
    {
        return _inputStyle switch
        {
            "success" => "Low",
            "warning" => "Medium",
            "danger" => "High",
            _ => string.Empty,
        };
    }

    protected override void OnParametersSet()
    {
        SizeOption = CssClass(CardSize);
        SelectedStyle = CardOptions(CardStyle);
        CardTag = CardTags(CardStyle);
    }
}