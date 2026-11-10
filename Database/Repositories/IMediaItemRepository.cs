using FloodOnlineReportingTool.Database.Models.Media;
using FloodOnlineReportingTool.Database.Models.ResultModels;

namespace FloodOnlineReportingTool.Database.Repositories
{
    public interface IMediaItemRepository
    {
        /// <summary>
        /// Gets a count of all media items associated with a flood report.
        /// </summary>
        /// <returns>Integer indicating total number of media items</returns>
        Task<int> GetCountByEvent(Guid floodEventId, CancellationToken cancellationToken);
        /// <summary>
        /// Get all media items associated with a flood event.
        /// </summary>
        Task<IReadOnlyCollection<MediaItem>> GetByEvent(Guid floodEventId, CancellationToken cancellationToken);

        /// <summary>
        /// Create media items and associate them with a flood event.
        /// </summary>
        /// <returns>A result pattern with the created media items, or a list of errors.</returns>
        Task<Result<IReadOnlyCollection<MediaItem>>> Create(Guid floodEventId, IReadOnlyCollection<MediaItem> mediaItems, CancellationToken cancellationToken);

        Task<Result<string>> CreatePublicSubmission(string uploadReference, PublicMediaSubmissionInput submission, IReadOnlyCollection<MediaItem> mediaItems, CancellationToken cancellationToken);

        /// <summary>
        /// Update the title of a media item.
        /// </summary>
        /// <returns>A result pattern with the updated media item, or a list of errors.</returns>
        Task<Result<MediaItem>> UpdateTitle(Guid mediaItemId, string title, CancellationToken cancellationToken);

        /// <summary>
        /// Delete a media item by ID.
        /// </summary>
        /// <returns>A result pattern indicating success, or a list of errors.</returns>
        Task<DeleteResult<MediaItem>> Delete(Guid mediaItemId, CancellationToken cancellationToken);

        /// <summary>
        /// Rotates a media item image by the specified degrees (90, 180, 270).
        /// </summary>
        /// <returns>A result pattern indicating success, or a list of errors.</returns>
        Task<bool> RotateMediaImage(Guid mediaItemId, int rotationDegrees, CancellationToken ct = default);
    }
}
