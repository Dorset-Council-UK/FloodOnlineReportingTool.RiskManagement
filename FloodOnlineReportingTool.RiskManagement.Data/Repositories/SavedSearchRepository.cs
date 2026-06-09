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
        var exists = await context.SavedSearches
            .AsNoTracking()
            .IgnoreAutoIncludes()
            .AnyAsync(s => s.Id == searchCriteria.Id, cancellationToken);

        if (exists)
        {
            return Result<SavedSearch?>.Failure([$"A saved search with ID '{searchCriteria.Id}' already exists."]);
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
    /// Logic to update a saved search, used by both Update methods
    /// </summary>
    private static async Task<Result<SavedSearch?>> Update(RiskManagementDbContext context, SavedSearch existingSearch, SavedSearch searchCriteria, CancellationToken cancellationToken)
    {
        if (existingSearch.IsDuplicateOf(searchCriteria))
        {
            return Result<SavedSearch?>.Failure([
                "This is a duplicate saved search.",
                "Update the existing search or change the criteria.",
            ]);
        }

        var updatedSearch = existingSearch with
        {
            //Id = searchCriteria.Id,
            //Reference = searchCriteria.Reference,
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

    public async Task<Result<SearchRedirect?>> FindLocalByReference(string reference, CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var savedSearchId = await context.SavedSearches
            .AsNoTracking()
            .IgnoreAutoIncludes()
            .Where(s => s.Reference == reference)
            .Select(s => s.Id)
            .FirstOrDefaultAsync(cancellationToken);

        var floodEventId = await context.FloodEvents
            .AsNoTracking()
            .IgnoreAutoIncludes()
            .Where(f => f.Reference == reference)
            .Select(f => f.Id)
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
