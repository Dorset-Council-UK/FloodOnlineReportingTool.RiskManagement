using FloodOnlineReportingTool.RiskManagement.Data.DbContexts;
using FloodOnlineReportingTool.RiskManagement.Data.Models.Enums;
using FloodOnlineReportingTool.RiskManagement.Data.Models.ResultModels;
using FloodOnlineReportingTool.RiskManagement.Data.Models.SavedSearches;
using Microsoft.EntityFrameworkCore;

namespace FloodOnlineReportingTool.RiskManagement.Data.Repositories;

public class SavedSearchRepository(IDbContextFactory<RiskManagementDbContext> contextFactory) : ISavedSearchRepository
{
    public async Task<int> Count(CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.SavedSearches.CountAsync(cancellationToken);
    }

    public async Task<bool> Exists(Guid savedSearchId, CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.SavedSearches
            .AsNoTracking()
            .IgnoreAutoIncludes()
            .AnyAsync(s => s.Id == savedSearchId, cancellationToken);
    }

    public async Task<bool> Exists(string reference, CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.SavedSearches
            .AsNoTracking()
            .IgnoreAutoIncludes()
            .AnyAsync(s => s.Reference == reference, cancellationToken);
    }

    public async Task<SavedSearch?> Get(Guid savedSearchId, CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.SavedSearches
            .AsNoTracking()
            .Include(s => s.FloodEvent)
            .FirstOrDefaultAsync(s => s.Id == savedSearchId, cancellationToken);
    }

    public async Task<SavedSearch?> Get(string reference, CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.SavedSearches
            .AsNoTracking()
            .Include(s => s.FloodEvent)
            .FirstOrDefaultAsync(s => s.Reference == reference, cancellationToken);
    }

    public async Task<Result<SavedSearch?>> Create(SavedSearch searchCriteria, CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        SavedSearch? existingSearch = await context.SavedSearches
            .AsNoTracking()
            .IgnoreAutoIncludes()
            .FirstOrDefaultAsync(s => s.Id == searchCriteria.Id, cancellationToken);

        if (existingSearch is null)
        {
            return Result<SavedSearch?>.Failure([$"A saved search with ID '{searchCriteria.Id}' already exists."]);
        }

        var duplicateSavedSearch = await FindDuplicate(context, existingSearch, cancellationToken);
        if (duplicateSavedSearch is not null)
        {
            var errors = BuildDuplicateErrors(duplicateSavedSearch);
            return Result<SavedSearch?>.Failure(errors);
        }

        context.SavedSearches.Add(searchCriteria);
        await context.SaveChangesAsync(cancellationToken);

        return Result<SavedSearch?>.Success(searchCriteria);
    }

    public async Task<Result<SavedSearch?>> Update(string reference, SavedSearch searchCriteria, CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var existingSearch = await context.SavedSearches
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Reference == reference, cancellationToken);

        if (existingSearch is null)
        {
            return Result<SavedSearch?>.Failure([$"No saved search found for reference '{reference}'."]);
        }

        return await Update(context, existingSearch, searchCriteria, cancellationToken);
    }

    public async Task<Result<SavedSearch?>> Update(Guid savedSearchId, SavedSearch searchCriteria, CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var existingSearch = await context.SavedSearches
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == savedSearchId, cancellationToken);

        if (existingSearch is null)
        {
            return Result<SavedSearch?>.Failure([$"No saved search found for ID '{savedSearchId}'."]);
        }

        return await Update(context, existingSearch, searchCriteria, cancellationToken);
    }

    /// <summary>
    /// Logic to update a saved search, used by both Update by ID and Update by reference methods
    /// </summary>
    private static async Task<Result<SavedSearch?>> Update(RiskManagementDbContext context, SavedSearch existingSearch, SavedSearch searchCriteria, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(searchCriteria.UpdatedBy))
        {
            return Result<SavedSearch?>.Failure(["'UpdatedBy' is required to update a saved search."]);
        }

        var duplicateSavedSearch = await FindDuplicate(context, existingSearch, cancellationToken);
        if (duplicateSavedSearch is not null)
        {
            var errors = BuildDuplicateErrors(duplicateSavedSearch);
            return Result<SavedSearch?>.Failure(errors);
        }

        var updatedSearch = existingSearch with
        {
            // Id
            // Reference
            // CreatedAt
            UpdatedAt = DateTimeOffset.UtcNow,
            UpdatedBy = searchCriteria.UpdatedBy,
            SearchTitle = searchCriteria.SearchTitle,
            ResultsFrom = searchCriteria.ResultsFrom,
            ResultsTo = searchCriteria.ResultsTo,
            FloodSources = searchCriteria.FloodSources,
            UPRN = searchCriteria.UPRN,
            USRN = searchCriteria.USRN,
            Postcode = searchCriteria.Postcode,
            ReporterString = searchCriteria.ReporterString,
            Geom = searchCriteria.Geom,
            PagingSize = searchCriteria.PagingSize,
            SortBy = searchCriteria.SortBy,
            SortOrder = searchCriteria.SortOrder,
            LimitByOrganisation = searchCriteria.LimitByOrganisation,
            LimitByStatus = searchCriteria.LimitByStatus,
            ShowResultsAs = searchCriteria.ShowResultsAs,
        };

        context.SavedSearches.Update(updatedSearch);
        await context.SaveChangesAsync(cancellationToken);

        return Result<SavedSearch?>.Success(updatedSearch);
    }

    private static List<string> BuildDuplicateErrors(SavedSearch duplicateSavedSearch)
    {
        List<string> errors = ["This is a duplicate saved search."];

        string? atPart = duplicateSavedSearch.UpdatedAt == null ? null : $" at {duplicateSavedSearch.UpdatedAt.GdsReadable()}";
        string? byPart = string.IsNullOrWhiteSpace(duplicateSavedSearch.UpdatedBy) ? null : $" by {duplicateSavedSearch.UpdatedBy}";

        if (atPart != null || byPart != null)
        {
            errors.Add($"The other saved search was last updated{atPart}{byPart}.");
        }

        errors.Add($"Update the other saved search reference {duplicateSavedSearch.Reference} or change the search criteria.");

        return errors;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="context"></param>
    /// <param name="existingSearch"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private static async Task<SavedSearch?> FindDuplicate(RiskManagementDbContext context, SavedSearch existingSearch, CancellationToken cancellationToken)
    {
        // SQL query (Multiple Where = AND queries)
        var results = await context.SavedSearches
            .AsNoTracking()
            .IgnoreAutoIncludes()
            .Where(s => s.Id != existingSearch.Id) // exclude the current saved search from the comparison
            .Where(s => s.ResultsFrom == existingSearch.ResultsFrom) // same from date
            .Where(s => s.ResultsTo == existingSearch.ResultsTo) // same to date
            .Where(s => (s.Geom == null && existingSearch.Geom == null)
                     || (s.Geom != null && existingSearch.Geom != null && s.Geom.EqualsTopologically(existingSearch.Geom))
            ) // same geometry (both null or geometries are topologically equal)
            .ToListAsync(cancellationToken);

        // client evaluations
        //var incomingSourceIds = existingSearch.FloodSources.Order().ToList();
        return results.FirstOrDefault(
            s => s.Postcode.Equals(existingSearch.Postcode, StringComparison.OrdinalIgnoreCase) &&
            s.FloodSources.Order().SequenceEqual(existingSearch.FloodSources.Order())
        );
    }

    ///// <summary>
    /////     <para>Using business logic, is this saved search a duplicate of another saved search?</para>
    /////     <para>
    /////         This is used to prevent creating duplicate saved searches.
    /////         We can use the same logic to check for duplicates when updating a saved search, but we need to exclude the current saved search from the comparison.
    /////     </para>
    ///// </summary>
    ///// <param name="otherSavedSearch">The saved search to compare against.</param>
    ///// <returns><see langword="true" /> if this saved search is considered a duplicate of <paramref name="otherSavedSearch"/></returns>
    //private SavedSearchDuplicateCheck IsDuplicateOf(SavedSearch otherSavedSearch)
    //{
    //    bool isDuplicate = savedSearch.ResultsFrom == otherSavedSearch.ResultsFrom &&
    //        savedSearch.ResultsTo == otherSavedSearch.ResultsTo &&
    //        string.Equals(savedSearch.Postcode, otherSavedSearch.Postcode, StringComparison.OrdinalIgnoreCase) &&
    //        savedSearch.FloodSources.Count == otherSavedSearch.FloodSources.Count &&
    //        (
    //            (savedSearch.Geom == null && otherSavedSearch.Geom == null) ||
    //            (savedSearch.Geom != null && otherSavedSearch.Geom != null && savedSearch.Geom.EqualsTopologically(otherSavedSearch.Geom))
    //        );

    //    return isDuplicate
    //        ? SavedSearchDuplicateCheck.Duplicate(savedSearch)
    //        : SavedSearchDuplicateCheck.NotDuplicate();
    //}

    public async Task<Result<SearchRedirect?>> FindLocalByReference(string reference, CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var savedSearchId = await context.SavedSearches
            .AsNoTracking()
            .IgnoreAutoIncludes()
            .Where(s => s.Reference == reference)
            .Select(s => s.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (savedSearchId != Guid.Empty)
        {
            return Result<SearchRedirect?>.Success(new SearchRedirect
            {
                Id = savedSearchId,
                Reference = reference,
                RedirectType = RecordTypes.Search,
            });
        }

        var floodEventId = await context.FloodEvents
            .AsNoTracking()
            .IgnoreAutoIncludes()
            .Where(f => f.Reference == reference)
            .Select(f => f.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (floodEventId != Guid.Empty)
        {
            return Result<SearchRedirect?>.Success(new SearchRedirect
            {
                Id = floodEventId,
                Reference = reference,
                RedirectType = RecordTypes.Event,
            });
        }

        return Result<SearchRedirect?>.Failure([$"No local record found for reference '{reference}'."]);
    }
}
