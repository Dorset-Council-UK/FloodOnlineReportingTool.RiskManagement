using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace FloodOnlineReportingTool.RiskManagement.Components.Search;

public partial class Searchbox
{
    // Fields
    private FieldIdentifier? _fieldIdentifier;
    private string? _id;

    // Properties
    [CascadingParameter]
    private EditContext? EditContext { get; set; }

    [Parameter, EditorRequired]
    public string LabelText { get; set; }

    private string? SearchText { get; set; }
}

