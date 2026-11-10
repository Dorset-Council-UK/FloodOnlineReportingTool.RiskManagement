using FloodOnlineReportingTool.Database.DbContexts;
using FloodOnlineReportingTool.Database.EntitiesConfiguration;
using FloodOnlineReportingTool.Database.Models.Media;
using FloodOnlineReportingTool.Database.Models.ResultModels;
using FloodOnlineReportingTool.Database.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.Data.Common;
using System.Security.Cryptography;

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

        public async Task<Result<string>> CreatePublicSubmission(
        string uploadReference,
        PublicMediaSubmissionInput submission,
        IReadOnlyCollection<MediaItem> mediaItems,
        CancellationToken cancellationToken)
        {
            var name = PublicMediaSubmissionInput.NormalizeContact(submission.Name);
            var email = PublicMediaSubmissionInput.NormalizeContact(submission.EmailAddress);
            var errors = ValidatePublicSubmission(uploadReference, submission, mediaItems, name, email);
            if (errors.Count > 0)
            {
                return Result<string>.Failure(errors);
            }

            try
            {
                await using var strategyContext = await contextFactory.CreateDbContextAsync(cancellationToken);
                var strategy = strategyContext.Database.CreateExecutionStrategy();
                return await strategy.ExecuteAsync(async cancellationToken =>
                {
                    await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
                    var metadata = new PublicMediaSubmission
                    {
                        Id = submission.Id,
                        Name = name,
                        EmailAddress = email,
                        SubmittedAtUtc = DateTimeOffset.UtcNow,
                        SharingAgreementAccepted = true,
                    };

                    const int maxReferenceAttempts = 3;
                    for (var attempt = 0; attempt < maxReferenceAttempts; attempt++)
                    {
                        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
                        // Serialize submissions and changes to the upload link until the batch is committed.
                        var floodEvent = await context.FloodEvents
                            .FromSqlInterpolated($"SELECT * FROM \"fortriskmanagement\".\"FloodEvents\" WHERE \"MediaReference\" = {uploadReference} FOR UPDATE")
                            .IgnoreAutoIncludes()
                            .AsNoTracking()
                            .FirstOrDefaultAsync(cancellationToken);
                        if (floodEvent is null)
                        {
                            return Result<string>.Failure(["This upload link is not available"]);
                        }

                        var existing = await context.PublicMediaSubmissions
                            .AsNoTracking()
                            .IgnoreAutoIncludes()
                            .Include(s => s.MediaItems)
                            .SingleOrDefaultAsync(s => s.Id == submission.Id, cancellationToken);
                        if (existing is not null)
                        {
                            return MatchesPublicSubmission(existing, floodEvent.Id, name, email, mediaItems)
                                ? Result<string>.Success(existing.Reference)
                                : Result<string>.Failure(["This batch was already submitted with different details. Please reload the page before uploading more media"]);
                        }

                        if (floodEvent.MediaUploadLock || floodEvent.MediaReferenceExpiration is null
                            || floodEvent.MediaReferenceExpiration <= DateTimeOffset.UtcNow)
                        {
                            return Result<string>.Failure(["The media upload for this flood event has expired or is no longer available"]);
                        }

                        metadata.MediaItems = mediaItems.Select(item => new MediaItem
                        {
                            Id = item.Id,
                            UploadDateUtc = item.UploadDateUtc ?? metadata.SubmittedAtUtc,
                            URL = item.URL,
                            Title = item.Title,
                            FloodEventId = floodEvent.Id,
                            PublicMediaSubmissionId = metadata.Id,
                        }).ToList();
                        context.PublicMediaSubmissions.Add(metadata);

                        try
                        {
                            await context.SaveChangesAsync(cancellationToken);
                            await transaction.CommitAsync(cancellationToken);
                            return Result<string>.Success(metadata.Reference);
                        }
                        catch (DbUpdateException ex) when (IsSubmissionReferenceCollision(ex))
                        {
                            await transaction.RollbackAsync(cancellationToken);
                            context.ChangeTracker.Clear();
                            metadata.Reference = Convert.ToHexString(RandomNumberGenerator.GetBytes(5)).ToUpperInvariant();
                        }
                    }

                    return Result<string>.Failure(["Unable to assign a submission reference. Please try submitting again"]);
                }, cancellationToken);
            }
            catch (Exception ex) when (ex is DbUpdateException or DbException)
            {
                return Result<string>.Failure(["Unable to save your media submission. Your details and media have been retained; please try again"]);
            }
        }

        private static List<string> ValidatePublicSubmission(
            string uploadReference, PublicMediaSubmissionInput submission,
            IReadOnlyCollection<MediaItem> mediaItems, string? name, string? email)
        {
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(uploadReference) || submission.Id == Guid.Empty)
            {
                errors.Add("This upload link or submission is not valid");
            }
            if (!submission.SharingAgreementAccepted)
            {
                errors.Add("You must agree that your media can be shared before submitting");
            }
            if (name?.Length > PublicMediaSubmissionInput.MaxNameLength)
            {
                errors.Add("Name must be 200 characters or fewer");
            }
            if (email?.Length > PublicMediaSubmissionInput.MaxEmailLength || !PublicMediaSubmissionInput.IsValidEmail(email))
            {
                errors.Add("Enter a valid email address of 254 characters or fewer");
            }
            if (mediaItems.Count is 0 or > 50)
            {
                errors.Add("Upload between 1 and 50 files before submitting");
            }
            if (mediaItems.Any(m => m.Id == Guid.Empty || string.IsNullOrWhiteSpace(m.URL)
                || string.IsNullOrWhiteSpace(m.Title) || m.Title.Length > 255)
                || mediaItems.Select(m => m.Id).Distinct().Count() != mediaItems.Count)
            {
                errors.Add("One or more media items are not valid");
            }
            return errors;
        }

        private static bool MatchesPublicSubmission(PublicMediaSubmission existing, Guid floodEventId,
            string? name, string? email, IReadOnlyCollection<MediaItem> mediaItems)
        {
            return existing.SharingAgreementAccepted && existing.Name == name && existing.EmailAddress == email
                && existing.MediaItems.Count == mediaItems.Count
                && existing.MediaItems.All(saved => saved.FloodEventId == floodEventId
                    && mediaItems.Any(item => item.Id == saved.Id && item.URL == saved.URL && item.Title == saved.Title));
        }

        private static bool IsSubmissionReferenceCollision(DbUpdateException exception)
        {
            return exception.InnerException is PostgresException
            {
                SqlState: PostgresErrorCodes.UniqueViolation,
                SchemaName: SchemaNames.RiskManagement,
                TableName: "PublicMediaSubmissions",
                ConstraintName: PublicMediaSubmissionConfiguration.ReferenceIndexName,
            };
        }
    }
}
