using FloodOnlineReportingTool.RiskManagement.Data.DbContexts;
using FloodOnlineReportingTool.RiskManagement.Data.Models;
using FloodOnlineReportingTool.RiskManagement.Data.Models.FloodEvents;
using FloodOnlineReportingTool.RiskManagement.Data.Models.ResultModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace FloodOnlineReportingTool.RiskManagement.Data.Repositories
{
    public class MediaItemRepository(
        ILogger<MediaItemRepository> logger,
        IDbContextFactory<RiskManagementDbContext> contextFactory
    ) : IMediaItemRepository
    {
        public async Task<int> GetCountByEvent(Guid floodEventId, CancellationToken cancellationToken)
        {
            logger.LogInformation("Getting media item count for flood event ID: {FloodEventId}", floodEventId);

            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
            return await context.FloodEvents
                .AsNoTracking()
                .IgnoreAutoIncludes()
                .Where(fr => fr.Id == floodEventId)
                .Select(fr => fr.MediaItems.Count)
                .SingleOrDefaultAsync(cancellationToken);
        }
        public async Task<IReadOnlyCollection<MediaItem>> GetByEvent(Guid floodEventId, CancellationToken cancellationToken)
        {
            logger.LogInformation("Getting media items for flood event ID: {FloodEventId}", floodEventId);

            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
            var floodEvent = await context.FloodEvents
                .AsNoTracking()
                .IgnoreAutoIncludes()
                .Include(fr => fr.MediaItems)
                .FirstOrDefaultAsync(fr => fr.Id == floodEventId, cancellationToken);

            var mediaItems = floodEvent?.MediaItems
                .OrderByDescending(mi => mi.UploadDateUtc)
                .ToList();

            return mediaItems ?? [];
        }

        public async Task<Result<IReadOnlyCollection<MediaItem>>> Create(Guid floodEventId, IReadOnlyCollection<MediaItem> mediaItems, CancellationToken cancellationToken)
        {
            logger.LogInformation("Creating media items for flood event ID: {FloodEventId}", floodEventId);

            if (floodEventId == Guid.Empty)
            {
                logger.LogInformation("Empty flood event ID provided for media items");
                return Result<IReadOnlyCollection<MediaItem>>.Failure(["Cannot create media items with empty flood event ID"]);
            }

            if (mediaItems.Count == 0)
            {
                logger.LogInformation("No media items provided for flood event ID: {FloodEventId}", floodEventId);
                return Result<IReadOnlyCollection<MediaItem>>.Success([]);
            }

            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
            var floodEventExists = await context.FloodEvents
                .AsNoTracking()
                .IgnoreAutoIncludes()
                .AnyAsync(fr => fr.Id == floodEventId, cancellationToken);

            if (!floodEventExists)
            {
                logger.LogInformation("Flood event {FloodEventId} not found for media items", floodEventId);
                return Result<IReadOnlyCollection<MediaItem>>.Failure([$"Cannot create media items, no flood event found for ID {floodEventId}"]);
            }

            var createdMediaItems = mediaItems
                .Select(mediaItem => new MediaItem
                {
                    Id = mediaItem.Id,
                    UploadDateUtc = mediaItem.UploadDateUtc ?? DateTimeOffset.UtcNow,
                    URL = mediaItem.URL,
                    Title = mediaItem.Title,
                    FloodEventId = floodEventId,
                })
                .ToList();

            context.MediaItems.AddRange(createdMediaItems);

            await context.SaveChangesAsync(cancellationToken);

            return Result<IReadOnlyCollection<MediaItem>>.Success(createdMediaItems);
        }

        public async Task<DeleteResult<MediaItem>> Delete(Guid mediaItemId, CancellationToken cancellationToken)
        {
            logger.LogInformation("Deleting media item ID: {MediaItemId}", mediaItemId);

            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
            var mediaItem = await context.MediaItems
                .FirstOrDefaultAsync(mi => mi.Id == mediaItemId, cancellationToken);

            if (mediaItem is null)
            {
                return DeleteResult<MediaItem>.Failure([$"An error occurred deleting the media item"]);
            }

            context.MediaItems.Remove(mediaItem);
            await context.SaveChangesAsync(cancellationToken);

            return DeleteResult<MediaItem>.Success();
        }

        public async Task<Result<MediaItem>> UpdateTitle(Guid mediaItemId, string title, CancellationToken cancellationToken)
        {
            logger.LogInformation("Updating title for media item ID: {MediaItemId}", mediaItemId);

            await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
            var mediaItem = await context.MediaItems
                .FirstOrDefaultAsync(mi => mi.Id == mediaItemId, cancellationToken);

            if (mediaItem is null)
            {
                logger.LogWarning(message: "Couldn't rename media item. No media item found for ID {mediaItemId}", mediaItemId);
                return Result<MediaItem>.Failure([$"An error occurred renaming the media item"]);
            }

            mediaItem.Title = title;
            await context.SaveChangesAsync(cancellationToken);

            return Result<MediaItem>.Success(mediaItem);
        }
    }
}
