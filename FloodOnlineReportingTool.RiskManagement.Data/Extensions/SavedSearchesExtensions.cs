namespace FloodOnlineReportingTool.RiskManagement.Data.Models.SavedSearches;

public static class SavedSearchesExtensions
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
        /// <param name="otherSavedSearch">The saved search to compare against.</param>
        /// <returns><see langword="true" /> if this saved search is considered a duplicate of <paramref name="otherSavedSearch"/></returns>
        internal SavedSearchDuplicateCheck IsDuplicateOf(SavedSearch otherSavedSearch)
        {
            bool isDuplicate = savedSearch.ResultsFrom == otherSavedSearch.ResultsFrom &&
                savedSearch.ResultsTo == otherSavedSearch.ResultsTo &&
                string.Equals(savedSearch.Postcode, otherSavedSearch.Postcode, StringComparison.OrdinalIgnoreCase) &&
                savedSearch.FloodSources.Count == otherSavedSearch.FloodSources.Count &&
                (
                    (savedSearch.Geom == null && otherSavedSearch.Geom == null) ||
                    (savedSearch.Geom != null && otherSavedSearch.Geom != null && savedSearch.Geom.EqualsTopologically(otherSavedSearch.Geom))
                );

            return isDuplicate
                ? SavedSearchDuplicateCheck.Duplicate(savedSearch)
                : SavedSearchDuplicateCheck.NotDuplicate();
        }

        /// <summary>
        ///     <para>Is this saved search locked?</para>
        ///     <para>A saved search is considered locked if it is associated with a flood event, meaning it cannot be modified or deleted.</para>
        /// </summary>
        public bool IsLocked => savedSearch.FloodEventId is not null;
    }
}
