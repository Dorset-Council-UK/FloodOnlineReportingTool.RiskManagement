using FloodOnlineReportingTool.Database.DbContexts;
using FloodOnlineReportingTool.Database.Models.Media;
using FloodOnlineReportingTool.Database.Models.ResultModels;
using FloodOnlineReportingTool.Database.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace FloodOnlineReportingTool.Database.Repositories
{
    public class MediaItemRepository(
        ILogger<MediaItemRepository> logger,
        IDbContextFactory<RiskManagementDbContext> contextFactory,
        IBlobStorageService blobStorageService
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

        public async Task<bool> RotateMediaImage(Guid mediaItemId, int rotationDegrees, CancellationToken ct = default)
        {
            try
            {
                const string thumbnailSuffix = ".thumbnail.jpg";

                await using var context = await contextFactory.CreateDbContextAsync(ct);
                var mediaItem = await context.MediaItems.Where(m => m.Id == mediaItemId).FirstOrDefaultAsync(ct);

                if (mediaItem is null)
                {
                    logger.LogWarning("Media item with ID {MediaItemId} not found", mediaItemId);
                    return false;
                }

                var mediaRotated = await RotateImageByUrlAsync(mediaItem.URL, rotationDegrees, ct);
                if (!mediaRotated)
                {
                    return false;
                }

                var thumbnailUrl = $"{mediaItem.URL}{thumbnailSuffix}";
                var thumbnailRotated = await RotateImageByUrlAsync(thumbnailUrl, rotationDegrees, ct, isOptional: true);
                if (thumbnailRotated)
                {
                    logger.LogInformation("Successfully rotated thumbnail for media item {MediaItemId}", mediaItemId);
                }
                else
                {
                    logger.LogInformation("Thumbnail not found for media item {MediaItemId}; skipping thumbnail rotation", mediaItemId);
                }

                logger.LogInformation("Successfully rotated media item {MediaItemId} by {RotationDegrees} degrees", mediaItemId, rotationDegrees);
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred rotating media item {MediaItemId}", mediaItemId);
                return false;
            }

            async Task<bool> RotateImageByUrlAsync(string imageUrl, int degrees, CancellationToken cancellationToken, bool isOptional = false)
            {
                var imageStream = await blobStorageService.DownloadFileFromBlobByURLAsync(imageUrl);
                if (imageStream is null)
                {
                    if (!isOptional)
                    {
                        logger.LogWarning("Failed to download image from blob storage for media item {MediaItemId}", mediaItemId);
                    }

                    return false;
                }

                using (imageStream)
                {
                    using var image = await Image.LoadAsync(imageStream, cancellationToken);
                    image.Mutate(x => x.Rotate(degrees));

                    using var rotatedStream = new MemoryStream();
                    var uriForMedia = new Uri(imageUrl);
                    var fileNameForMedia = System.IO.Path.GetFileName(uriForMedia.AbsolutePath);
                    var extension = System.IO.Path.GetExtension(fileNameForMedia).ToLowerInvariant();
                    var contentType = string.Equals(extension, ".png", StringComparison.Ordinal) ? "image/png" : "image/jpeg";

                    if (string.Equals(extension, ".png", StringComparison.Ordinal))
                    {
                        await image.SaveAsPngAsync(rotatedStream, cancellationToken);
                    }
                    else
                    {
                        await image.SaveAsJpegAsync(rotatedStream, cancellationToken);
                    }

                    rotatedStream.Position = 0;
                    await blobStorageService.UploadFileToBlobAsync(fileNameForMedia, contentType, rotatedStream);
                }

                return true;
            }
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
