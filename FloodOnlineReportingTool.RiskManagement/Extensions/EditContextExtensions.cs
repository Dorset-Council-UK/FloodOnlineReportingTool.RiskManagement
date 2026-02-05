namespace Microsoft.AspNetCore.Components.Forms;

internal static class EditContextExtensions
{
    extension(EditContext editContext)
    {
        /// <summary>
        /// Returns the aria-describedby attribute value for a form field, combining hint and error IDs as appropriate.
        /// Follows the GOV.UK Design System pattern for accessible form fields.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="hasHint">Whether the field has a GDS hint element.</param>
        /// <returns>
        /// A space-separated string of IDs for hint and/or error elements, or <see langword="null"/> if neither applies.
        /// </returns>
        public string? DescribedBy(string fieldName, bool hasHint = false)
        {
            var fieldIdentifier = editContext.Field(fieldName);
            return editContext.DescribedBy(fieldIdentifier, hasHint);
        }

        /// <summary>
        /// Returns the aria-describedby attribute value for a form field, combining hint and error IDs as appropriate.
        /// Follows the GOV.UK Design System pattern for accessible form fields.
        /// </summary>
        /// <param name="fieldIdentifier">The field identifier for the field.</param>
        /// <param name="hasHint">Whether the field has a GDS hint element.</param>
        /// <returns>
        /// A space-separated string of IDs for hint and/or error elements, or <see langword="null"/> if neither applies.
        /// </returns>
        public string? DescribedBy(FieldIdentifier fieldIdentifier, bool hasHint = false)
        {
            var fieldName = fieldIdentifier.FieldName;
            var hasErrors = editContext.IsValid(fieldIdentifier);

            return (hasHint, hasErrors) switch
            {
                (true, true) => $"{fieldName}-hint {fieldName}-error",
                (true, false) => $"{fieldName}-hint",
                (false, true) => $"{fieldName}-error",
                _ => null,
            };
        }
    }
}
