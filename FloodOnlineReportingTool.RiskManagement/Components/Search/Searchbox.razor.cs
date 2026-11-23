using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace FloodOnlineReportingTool.RiskManagement.Components.Search;

/// <summary>
/// A self-contained omnibox-style search control: a labelled text input with a search button,
/// submitted either by pressing Enter or selecting the button. Owns its own <see cref="EditContext"/>
/// so it can be dropped onto any page without the caller needing to wrap it in an <c>EditForm</c>.
/// </summary>
public partial class Searchbox
{
    private readonly SearchboxModel _model = new();
    private EditContext _editContext = default!;

    [Parameter, EditorRequired]
    public string LabelText { get; set; } = string.Empty;

    /// <summary>
    /// Optional hint text displayed below the label.
    /// </summary>
    [Parameter]
    public string? HintText { get; set; }

    /// <summary>
    /// Unique form name used for Blazor's enhanced-navigation form submission handling.
    /// Override this if more than one <see cref="Searchbox"/> is rendered on the same page.
    /// </summary>
    [Parameter]
    public string FormName { get; set; } = "Searchbox";

    /// <summary>
    /// Additional CSS classes to add to the input control.
    /// Defaults to govuk-input--width-20 to set the width of the input
    /// </summary>
    [Parameter]
    public string InputAdditionalCssClasses { get; set; } = "govuk-input--width-20";

    [Parameter]
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Disables the input and submit button, e.g. while a search is already in progress.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Raised when the user submits a search, either by pressing Enter or selecting the search
    /// button. The submitted text is trimmed but otherwise unvalidated; callers are responsible
    /// for any further validation (e.g. ignoring an empty search).
    /// </summary>
    [Parameter]
    public EventCallback<string> OnSearch { get; set; }

    private string InputClass => $"searchbox__input govuk-input {InputAdditionalCssClasses}{(string.IsNullOrWhiteSpace(ErrorMessage) ? string.Empty : " govuk-input--error")}";

    protected override void OnInitialized()
    {
        _editContext = new EditContext(_model);
    }

    private async Task HandleSubmitAsync()
    {
        if (OnSearch.HasDelegate)
        {
            await OnSearch.InvokeAsync(_model.SearchText?.Trim() ?? string.Empty);
        }
    }

    private sealed class SearchboxModel
    {
        public string? SearchText { get; set; }
    }
}

