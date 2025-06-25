using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System.Linq.Expressions;

namespace FloodOnlineReportingTool.RiskManagement.Components.Search;

public partial class Searchbox
{
    [CascadingParameter]
    private EditContext EditContext { get; set; } = default!;

    [Parameter, EditorRequired]
    public Expression<Func<object>> For { get; set; } = default!;

    private string? Id { get; set; }

    private FieldIdentifier FieldIdentifier => FieldIdentifier.Create(For);

    [Parameter, EditorRequired]
    public string LabelText { get; set; }

    protected override void OnInitialized()
    {
        if (Id == null)
        {
            Id = FieldIdentifier.FieldName;
        }
    }
}

