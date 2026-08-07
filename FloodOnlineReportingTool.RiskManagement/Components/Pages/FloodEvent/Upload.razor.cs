using FloodOnlineReportingTool.Database.Models.Media;
using FloodOnlineReportingTool.Database.Repositories;
using FloodOnlineReportingTool.RiskManagement.Models.Media;
using FloodOnlineReportingTool.RiskManagement.Models.Order;
using FloodOnlineReportingTool.RiskManagement.Services;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using System.Globalization;

namespace FloodOnlineReportingTool.RiskManagement.Components.Pages.FloodEvent;

[Authorize]
public partial class Upload(
    ILogger<Upload> logger,
    NavigationManager navigationManager,
    IJSRuntime JS,
    IBlobStorageService blobStorageService,
    IMediaItemRepository mediaItemRepository,
    IFloodEventRepository floodEventRepository
) : IAsyncDisposable
{
    // Page order properties
    //public string Title { get; set; } = "Add media to flood event";
    private IReadOnlyCollection<GdsBreadcrumb> Breadcrumbs { get; set; } = [];
    private string Title => EventPages.Upload.Title;

    [Parameter]
    public string? Reference { get; set; }

    [Parameter]
    public bool Overview { get; set; } = false;

    private const int MaxNumFiles = 50;
    private const int MaxFileSizeMB = 100;
    private const long MaxFileSize = MaxFileSizeMB * 1024 * 1024; // Convert MB to bytes

    private int RemainingSlots => MaxNumFiles - Model.UploadedFiles.Count;
    private static readonly string[] ImageFileTypes = [
        "image/jpeg",
        "image/jpg",
        "image/png",
        "image/gif",
    ];
    private static readonly string[] VideoFileTypes = [
        "video/mp4",
        "video/webm",
        "video/ogg",
    ];
    private static readonly string[] DocumentFileTypes = [
        "application/pdf",
        "text/csv",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
    ];
    private static readonly string[] AllowedFileTypes = [
        .. ImageFileTypes,
        .. VideoFileTypes,
        .. DocumentFileTypes,
    ];

    private static readonly string[] FriendlyFileTypes = [
        "JPG", "PNG", "GIF", "MP4", "WEBM", "OGG", "PDF", "DOCX", "CSV", "XLSX",
    ];

    private readonly CancellationTokenSource _cts = new();
    private EditContext _editContext = default!;
    private ValidationMessageStore _validationMessageStore = default!;
    private FieldIdentifier _fieldIdentifier;

    private Guid _floodEventId;
    private bool _isLoading = true;

    private readonly List<IBrowserFile> _uploadingFiles = [];
    private readonly List<RejectedFile> _rejectedFiles = [];
    private string? _uploadLimitError;

    private Models.Event.MediaItem? _renamingFile;
    private string _renameText = string.Empty;
    private string? _renameError;

    private string previousPage = $"event/details";

    private Models.Event.Media Model { get; set; } = default!;

    protected override void OnInitialized()
    {
        Breadcrumbs = CreateBreadcrumbs();

        // Setup model and edit context
        Model ??= new();
        _editContext = new(Model);
        _editContext.SetFieldCssClassProvider(new GdsFieldCssClassProvider());
        _validationMessageStore = new(_editContext);
        _fieldIdentifier = new(Model, nameof(Model.UploadedFiles));
        if (Overview)
        {
            previousPage = $"event/attachments/{Reference}";
        } else
        {
            previousPage = $"event/details/{Reference}";
        }
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            await _cts.CancelAsync();
            _cts.Dispose();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Error occurred during disposal");
        }

        GC.SuppressFinalize(this);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(Reference))
        {
            navigationManager.NavigateTo("not-found");
            return;
        }

        try
        {
            //get flood event ID by search reference
            var floodEvent = await floodEventRepository.GetBySearchReference(Reference, 27700, _cts.Token);
            if (!floodEvent.IsSuccess || floodEvent.ResultModel is null)
            {
                logger.LogWarning("Flood event could not be found by search reference when loading media items");
                navigationManager.NavigateTo("not-found");
                return;
            }

            _floodEventId = floodEvent.ResultModel.Id;
            await LoadExistingMediaItems();
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }

    private async Task LoadExistingMediaItems()
    {
        if (_floodEventId == Guid.Empty)
        {
            logger.LogWarning("Flood event ID was not available when loading media items");
            return;
        }

        var mediaItems = await mediaItemRepository.GetByEvent(_floodEventId, _cts.Token);
        Model.UploadedFiles = mediaItems
            .Select(mediaItem => new Models.Event.MediaItem
            {
                Id = mediaItem.Id,
                Name = mediaItem.Title ?? Path.GetFileName(mediaItem.URL),
                Url = mediaItem.URL,
                ContentType = GetContentTypeFromUrl(mediaItem.URL),
            })
            .ToList();
    }

    private async Task OnFilesSubmitted(IReadOnlyList<IBrowserFile>? files)
    {
        _isLoading = true;

        if (!ValidateFileUploadLimits(files))
        {
            StateHasChanged();
            _isLoading = false;
            return;
        }

        _uploadingFiles.AddRange(files!);
        foreach (var file in files!)
        {
            await ProcessSingleFileAsync(file);
            _uploadingFiles.RemoveAll(f => string.Equals(f?.Name, file.Name, StringComparison.Ordinal));
            StateHasChanged();
        }

        //finished uploading
        CheckValidationStateOfFileUploads();

        _isLoading = false;
        StateHasChanged();
    }

    private async Task<bool> ProcessSingleFileAsync(IBrowserFile file)
    {
        if (!AllowedFileTypes.Contains(file.ContentType))
        {
            _rejectedFiles.Add(new RejectedFile(file, FileRejectionReason.InvalidFileType));
            return false;
        }

        if (file.Size > MaxFileSize)
        {
            _rejectedFiles.Add(new RejectedFile(file, FileRejectionReason.FileTooLarge));
            return false;
        }

        try
        {
            var trustedFileNameForFileStorage = $"{Guid.NewGuid()}{Path.GetExtension(file.Name)}";
            await using var fileStream = file.OpenReadStream(MaxFileSize, _cts.Token);
            var blobUrl = await blobStorageService.UploadFileToBlobAsync(
                trustedFileNameForFileStorage,
                file.ContentType,
                fileStream);

            if (!string.IsNullOrWhiteSpace(blobUrl))
            {
                Model.UploadedFiles.Add(new Models.Event.MediaItem
                {
                    Name = Path.GetFileNameWithoutExtension(file.Name),
                    Url = blobUrl,
                    ContentType = file.ContentType,
                });
                return true;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while processing file {FileName}", file?.Name);
        }

        _rejectedFiles.Add(new RejectedFile(file, FileRejectionReason.UploadError));
        return false;
    }

    private static readonly Dictionary<string, string> _extensionToContentType = new(StringComparer.OrdinalIgnoreCase)
    {
        { ".jpg",  "image/jpeg" },
        { ".jpeg", "image/jpeg" },
        { ".png",  "image/png" },
        { ".gif",  "image/gif" },
        { ".mp4",  "video/mp4" },
        { ".webm", "video/webm" },
        { ".ogg",  "video/ogg" },
        { ".pdf",  "application/pdf" },
        { ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
        { ".csv",  "text/csv" },
        { ".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
    };

    private static string GetContentTypeFromUrl(string url)
    {
        var extension = Path.GetExtension(new Uri(url).LocalPath);
        return _extensionToContentType.TryGetValue(extension, out var contentType)
            ? contentType
            : string.Empty;
    }

    private void StartRename(Models.Event.MediaItem file)
    {
        _renamingFile = file;
        _renameText = file.Name;
        _renameError = null;
    }

    private void CancelRename()
    {
        _renamingFile = null;
        _renameText = string.Empty;
        _renameError = null;
    }

    private const int MaxRenameTitleLength = 255;

    private async Task SaveRename(Models.Event.MediaItem file)
    {
        _renameError = null;

        if (string.IsNullOrWhiteSpace(_renameText))
        {
            _renameError = "Enter a file name";
            return;
        }

        if (_renameText.Length > MaxRenameTitleLength)
        {
            _renameError = $"File name must be {MaxRenameTitleLength} characters or fewer";
            return;
        }

        if (file.Id.HasValue)
        {
            var result = await mediaItemRepository.UpdateTitle(file.Id.Value, _renameText, _cts.Token);
            if (!result.IsSuccess)
            {
                _renameError = string.Join(", ", result.Errors);
                return;
            }
        }

        file.Name = _renameText;
        _renamingFile = null;
        _renameText = string.Empty;
    }

    private async Task DeleteUploadedFile(Models.Event.MediaItem file)
    {
        //confirm
        bool confirmDelete = await JS.InvokeAsync<bool>("confirm", _cts.Token, "Are you sure you want to delete this file?");
        if (confirmDelete)
        {
            if (file.Id.HasValue)
            {
                var deleteMediaItem = await mediaItemRepository.Delete(file.Id.Value, _cts.Token);
                if (!deleteMediaItem.IsSuccess)
                {
                    foreach (var error in deleteMediaItem.Errors)
                    {
                        logger.LogWarning("Couldn't delete a media item: {ErrorMessage}", error);
                    }

                    _validationMessageStore.Add(_fieldIdentifier, "Sorry, something went wrong");
                    _editContext.NotifyValidationStateChanged();
                    return;
                } 
            }

            //delete from storage
            try
            {
                if (!await blobStorageService.DeleteFileFromBlobByURLAsync(file.Url))
                {
                    logger.LogWarning("An error occurred deleting media item with URL {url} from blob storage", file.Url);
                }
            } catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred deleting media item with URL {url} from blob storage", file.Url);
            }
            finally { 
                //remove from list
                Model.UploadedFiles.Remove(file);
            }
            CheckValidationStateOfFileUploads();
            StateHasChanged();
        }
    }
    private void DeleteRejectedFile(RejectedFile file)
    {
        // Remove the rejected file from the list
        _rejectedFiles.Remove(file);
        CheckValidationStateOfFileUploads();
        StateHasChanged();
    }

    private bool ValidateFileUploadLimits(IReadOnlyList<IBrowserFile>? files)
    {
        _uploadLimitError = null;

        if (files is null || files.Count == 0)
        {
            _uploadLimitError = ErrorMessageConstants.NoFileSelected;
            return false;
        }

        if (files.Count > (MaxNumFiles - Model.UploadedFiles.Count))
        {
            _uploadLimitError = string.Format(CultureInfo.CurrentCulture, ErrorMessageConstants.TooManyTotalFiles, MaxNumFiles);
            return false;
        }

        return true;
    }

    private void CheckValidationStateOfFileUploads()
    {
        // Clear any existing validation errors first
        _uploadLimitError = null;
        _validationMessageStore.Clear();

        // Add validation errors for specific fields
        if (_rejectedFiles.Count > 0)
        {
            foreach (var rejectedFile in _rejectedFiles)
            {

                _validationMessageStore.Add(_fieldIdentifier, GetErrorMessageForFile(rejectedFile));
            }
        }

        // Notify the EditContext that validation state has changed
        _editContext.NotifyValidationStateChanged();
    }

    private static string GetErrorMessageForFile(RejectedFile rejectedFile)
    {
        var errorMessage = ErrorMessageConstants.UploadError;
        switch (rejectedFile.FileRejectionReason)
        {
            case FileRejectionReason.InvalidFileType:
                errorMessage = string.Format(CultureInfo.CurrentCulture, ErrorMessageConstants.InvalidFileType, FriendlyFileTypes.JoinWithOr());
                break;
            case FileRejectionReason.FileTooLarge:
                errorMessage = string.Format(CultureInfo.CurrentCulture, ErrorMessageConstants.FileTooLarge, MaxFileSizeMB);
                break;
        }
        return errorMessage;
    }

    private async Task OnValidSubmit()
    {
        _validationMessageStore.Clear();
        _editContext.NotifyValidationStateChanged();

        if (_floodEventId == Guid.Empty)
        {
            logger.LogWarning("Flood event ID was not available when saving media items");
            _validationMessageStore.Add(_fieldIdentifier, "Sorry, something went wrong");
            _editContext.NotifyValidationStateChanged();
            return;
        }

        if (Model.UploadedFiles.Count == 0)
        {
            navigationManager.NavigateTo(previousPage);
            return;
        }

        var mediaItems = Model.UploadedFiles
            .Where(file => !file.Id.HasValue)
            .Select(file => new MediaItem
            {
                UploadDateUtc = DateTimeOffset.UtcNow,
                URL = file.Url,
                Title = file.Name,
            })
            .ToList();

        if (mediaItems.Count == 0)
        {
            navigationManager.NavigateTo(previousPage);
            return;
        }

        var createMediaItems = await mediaItemRepository.Create(_floodEventId, mediaItems, _cts.Token);
        if (!createMediaItems.IsSuccess)
        {
            foreach (var error in createMediaItems.Errors)
            {
                logger.LogWarning("Couldn't create a attachment item: {ErrorMessage}", error);
            }

            _validationMessageStore.Add(_fieldIdentifier, "Sorry, something went wrong");
            _editContext.NotifyValidationStateChanged();
            return;
        }

        navigationManager.NavigateTo(previousPage);
    }

    private IReadOnlyCollection<GdsBreadcrumb> CreateBreadcrumbs()
    {
        return
        [
            GeneralPages.Home.ToGdsBreadcrumb(),
            new ($"{EventPages.Details.Url}/{Reference}", $"Details"),
            new ($"{EventPages.Upload.Url}/{Reference}", "Upload Attachments")
        ];
    }
}
