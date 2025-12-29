#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.AspNetCore.Components.Forms;
#pragma warning restore IDE0130 // Namespace does not match folder structure

internal static class EditContextExtensions
{
    /// <summary>
    /// Returns the aria-describedby attribute value for a form field, combining hint and error IDs as appropriate.
    /// Follows the GOV.UK Design System pattern for accessible form fields.
    /// </summary>
    /// <param name="editContext">The edit context containing form state.</param>
    /// <param name="fieldName">The name of the field.</param>
    /// <param name="hasHint">Whether the field has a GDS hint element.</param>
    /// <returns>
    /// A space-separated string of IDs for hint and/or error elements, or <c>null</c> if neither applies.
    /// </returns>
    public static string? DescribedBy(this EditContext editContext, string fieldName, bool hasHint = false)
    {
        var fieldIdentifier = editContext.Field(fieldName);
        return DescribedBy(editContext, fieldIdentifier, hasHint);
    }

    /// <summary>
    /// Returns the aria-describedby attribute value for a form field, combining hint and error IDs as appropriate.
    /// Follows the GOV.UK Design System pattern for accessible form fields.
    /// </summary>
    /// <param name="editContext">The edit context containing form state.</param>
    /// <param name="fieldIdentifier">The field identifier for the field.</param>
    /// <param name="hasHint">Whether the field has a GDS hint element.</param>
    /// <returns>
    /// A space-separated string of IDs for hint and/or error elements, or <c>null</c> if neither applies.
    /// </returns>
    public static string? DescribedBy(this EditContext editContext, FieldIdentifier fieldIdentifier, bool hasHint = false)
    {
        ArgumentNullException.ThrowIfNull(editContext);

        var fieldName = fieldIdentifier.FieldName;
        var hasErrors = !editContext.IsValid(fieldIdentifier);

        return (hasHint, hasErrors) switch
        {
            (true, true) => $"{fieldName}-hint {fieldName}-error",
            (true, false) => $"{fieldName}-hint",
            (false, true) => $"{fieldName}-error",
            _ => null
        };
    }
}
