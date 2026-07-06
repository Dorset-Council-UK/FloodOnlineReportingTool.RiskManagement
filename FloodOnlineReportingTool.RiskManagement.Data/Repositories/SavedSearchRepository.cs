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
            .IgnoreAutoIncludes()
            .FirstOrDefaultAsync(s => s.Id == savedSearchId, cancellationToken);
    }

    public async Task<SavedSearch?> Get(string reference, CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await context.SavedSearches
            .AsNoTracking()
            .IgnoreAutoIncludes()
            .FirstOrDefaultAsync(s => s.Reference == reference, cancellationToken);
    }

    public async Task<Result<SavedSearch?>> Create(SavedSearch searchCriteria, CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        SavedSearch? existingSearch = await context.SavedSearches
            .AsNoTracking()
            .IgnoreAutoIncludes()
            .FirstOrDefaultAsync(s => s.Id == searchCriteria.Id, cancellationToken);

        if (existingSearch is not null)
        {
            return Result<SavedSearch?>.Failure([$"A saved search with ID '{searchCriteria.Id}' already exists."]);
        }

        var duplicateSavedSearch = await FindDuplicate(context, searchCriteria, cancellationToken);
        if (duplicateSavedSearch is not null)
        {
            var warnings = BuildDuplicateMessages(duplicateSavedSearch);
            return Result<SavedSearch?>.Failure(ResultSeverity.Warning, warnings);
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
            FloodEventId = searchCriteria.FloodEventId ?? existingSearch.FloodEventId,
        };

        // Now is when we check if there is a duplicate saved search matching the updated search criteria
        var duplicateSavedSearch = await FindDuplicate(context, updatedSearch, cancellationToken);
        if (duplicateSavedSearch is not null)
        {
            var warnings = BuildDuplicateMessages(duplicateSavedSearch);
            return Result<SavedSearch?>.Failure(ResultSeverity.Warning, warnings);
        }

        context.SavedSearches.Update(updatedSearch);
        await context.SaveChangesAsync(cancellationToken);

        return Result<SavedSearch?>.Success(updatedSearch);
    }

    private static List<string> BuildDuplicateMessages(SavedSearch duplicateSavedSearch)
    {
        List<string> messages = ["This is a duplicate saved search."];

        string? atPart = duplicateSavedSearch.UpdatedAt == null ? null : $" at {duplicateSavedSearch.UpdatedAt.GdsReadable()}";
        string? byPart = string.IsNullOrWhiteSpace(duplicateSavedSearch.UpdatedBy) ? null : $" by {duplicateSavedSearch.UpdatedBy}";
        if (atPart != null || byPart != null)
        {
            messages.Add($"The other saved search was last updated{atPart}{byPart}.");
        }

        messages.Add($"You can update the other saved search <a href=\"search/results/{duplicateSavedSearch.Reference}\" class=\"govuk-link govuk-link--no-visited-state\">{duplicateSavedSearch.Reference}</a>");
        messages.Add($"Or change your search criteria");

        return messages;
    }

    /// <summary>
    /// Tries to find a duplicate saved search based on the provided saved search criteria.
    /// </summary>
    /// <remarks>
    ///     <para>Uses the existing database context.</para>
    ///     <para>Can be used in creating a saved search, and updating a saved search.</para>
    /// </remarks>
    /// <returns><see cref="SavedSearch"/> of the duplicate if it is found, otherwise <see langword="null"/></returns>
    private static async Task<SavedSearch?> FindDuplicate(RiskManagementDbContext context, SavedSearch savedSearch, CancellationToken cancellationToken)
    {
        // SQL query (Multiple Where = AND queries)
        var query = context.SavedSearches
            .AsNoTracking()
            .IgnoreAutoIncludes()
            .Where(s => s.Id != savedSearch.Id) // exclude the current saved search from the comparison
            .Where(s => s.ResultsFrom == savedSearch.ResultsFrom) // same from date
            .Where(s => s.ResultsTo == savedSearch.ResultsTo); // same to date

        if (savedSearch.Geom is null)
        {
            // no geometry
            query = query.Where(s => s.Geom == null);
        }
        else
        {
            // has geometry, check for topological equality
            query = query.Where(s => s.Geom != null && s.Geom.EqualsTopologically(savedSearch.Geom));
        }

        var results = await query.ToListAsync(cancellationToken);

        // Client evaluations
        return results.FirstOrDefault(
            s => s.Postcode.Equals(savedSearch.Postcode, StringComparison.OrdinalIgnoreCase) &&
            s.FloodSources.Order().SequenceEqual(savedSearch.FloodSources.Order())
        );
    }

    public async Task<Result<SearchRedirect?>> FindLocalByReference(string reference, CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        // If there is a flood event we go there first. 
        var floodEventId = await context.FloodEvents
            .AsNoTracking()
            .Where(f => f.SavedSearch.Reference == reference)
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

        // No flood event with the reference, check for a saved search with the reference
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

        return Result<SearchRedirect?>.Failure([$"No local record found for reference '{reference}'."]);
    }
}
