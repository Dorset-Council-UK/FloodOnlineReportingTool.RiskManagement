namespace FloodOnlineReportingTool.RiskManagement.Data.Models.SavedSearches;

internal static class SavedSearchesExtensions
{
    extension(SavedSearch savedSearch)
    {
        /// <summary>
        ///     <para>Using business logic, is this saved search a duplicate of another saved search?</para>
        ///     <para>
        ///         This is used to prevent creating duplicate saved searches.
        ///         We can use the same logic to check for duplicates when updating a saved search, but we need to exclude the current saved search from the comparison.
        ///     </para>
        /// </summary>
        /// <param name="other">The saved search to compare against.</param>
        /// <returns><see langword="true" /> if this saved search is considered a duplicate of <paramref name="other"/></returns>
        internal SavedSearchDuplicateCheck IsDuplicateOf(SavedSearch other)
        {
            bool isDuplicate = savedSearch.ResultsFrom == other.ResultsFrom &&
                savedSearch.ResultsTo == other.ResultsTo &&
                string.Equals(savedSearch.Postcode, other.Postcode, StringComparison.OrdinalIgnoreCase) &&
                savedSearch.FloodSources.Count == other.FloodSources.Count &&
                (
                    (savedSearch.Geom == null && other.Geom == null) ||
                    (savedSearch.Geom != null && other.Geom != null && savedSearch.Geom.EqualsTopologically(other.Geom))
                );

            return isDuplicate
                ? SavedSearchDuplicateCheck.Duplicate(savedSearch)
                : SavedSearchDuplicateCheck.NotDuplicate();
        }
    }
}
