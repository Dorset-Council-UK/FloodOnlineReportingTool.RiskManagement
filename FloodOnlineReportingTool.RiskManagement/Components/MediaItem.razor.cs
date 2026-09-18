using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace FloodOnlineReportingTool.RiskManagement.Components
{
    public partial class MediaItem
    {
        [Parameter]
        public required FloodOnlineReportingTool.Database.Models.Media.MediaItem Media { get; set; }
        [Parameter]
        public bool IsEditable { get; set; }
        [Parameter]
        public EventCallback OnRefreshComponent { get; set; }
        private const string ThumbnailSuffix = ".thumbnail.jpg";
        private readonly CancellationTokenSource _cts = new();
        private long _cacheBuster = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        private bool _isRotating = false;

        private static string GetThumbnailBlobUrl(string imageBlobUrl)
        {
            return $"{imageBlobUrl}{ThumbnailSuffix}";
        }

        private string GetBlobReadUrl(string blobUrl, bool withCacheBust)
        {
            var readSasToken = blobStorageOptions.Value.ReadSASToken;
            var urlWithSasToken = string.IsNullOrWhiteSpace(readSasToken)
                ? blobUrl
                : $"{blobUrl}?{readSasToken.TrimStart('?')}";
            if (withCacheBust)
            {
                var separator = urlWithSasToken.Contains('?') ? "&" : "?";
                return $"{urlWithSasToken}{separator}v={_cacheBuster}";
            }
            return urlWithSasToken;
        }

        private string GetThumbnailReadUrl(string imageBlobUrl)
        {
            return GetBlobReadUrl(GetThumbnailBlobUrl(imageBlobUrl), withCacheBust: true);
        }

        private string GetImagePreviewErrorFallback(string originalImageBlobUrl)
        {
            var fallbackUrl = GetBlobReadUrl(originalImageBlobUrl, true).Replace("'", "\\'", StringComparison.Ordinal);
            return $"this.onerror=null;this.src='{fallbackUrl}';";
        }

        private async Task DeleteMedia()
        {
            bool ConfirmDelete = await JS.InvokeAsync<bool>("confirm", "Are you sure you want to delete this file?");
            if (ConfirmDelete)
            {
                try
                {
                    await mediaItemRepository.Delete(Media.Id, _cts.Token);

                    if (OnRefreshComponent.HasDelegate)
                    {
                        await OnRefreshComponent.InvokeAsync();
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred deleting a media item");
                }
            }
        }

        private async Task RotateImage(int degrees)
        {
            _isRotating = true;
            StateHasChanged();

            try
            {
                var success = await mediaItemRepository.RotateMediaImage(Media.Id, degrees);
                if (success)
                {
                    // Update cache buster to force image reload
                    _cacheBuster = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                    // Refresh the component to show the rotated image
                    if (OnRefreshComponent.HasDelegate)
                    {
                        await OnRefreshComponent.InvokeAsync();
                    }
                }
                else
                {
                    logger.LogWarning("Failed to rotate image for media item {MediaId}", Media.Id);
                    await JS.InvokeVoidAsync("alert", "Failed to rotate image. Please try again.");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred rotating a media item");
                await JS.InvokeVoidAsync("alert", "An error occurred while rotating the image. Please try again.");
            }
            finally
            {
                _isRotating = false;
                StateHasChanged();
            }
        }
    }
}