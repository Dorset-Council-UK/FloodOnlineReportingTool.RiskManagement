using FloodOnlineReportingTool.Database.Models.FloodEvents;
using FloodOnlineReportingTool.Database.Models.Media;
using FloodOnlineReportingTool.Database.Repositories;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace FloodOnlineReportingTool.RiskManagement.Components
{
    public partial class MediaList(IFloodEventRepository floodEventRepository, IJSRuntime JS, ILogger<MediaList> logger) : IAsyncDisposable
    {
        [Parameter]
        public FloodEvent? FloodEvent { get; set; }
        [Parameter]
        public bool IsEditable { get; set; }
        [Parameter]
        public required string Reference { get; set; }
        private readonly CancellationTokenSource _cts = new();

        private ICollection<MediaType>? MediaTypes;
        private IJSObjectReference? _jsModule;

        protected override async Task OnInitializedAsync()
        {
            //get flood event ID by search reference
            var floodEvent = await floodEventRepository.GetBySearchReference(Reference, 27700, _cts.Token);
            if (!floodEvent.IsSuccess || floodEvent.ResultModel is null)
            {
                logger.LogWarning("Flood event could not be found by search reference when loading media items");
                return;
            }

            FloodEvent = floodEvent.ResultModel;
            MediaTypes = [.. Enum.GetValues<MediaType>().Where(mt => mt != MediaType.Unknown)];

            await base.OnInitializedAsync();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            _jsModule ??= await JS.InvokeAsync<IJSObjectReference>("import", "./Components/MediaList.razor.js");

            await _jsModule.InvokeVoidAsync("initGallery");

            await base.OnAfterRenderAsync(firstRender);
        }

        //private async Task AddMediaToFloodEvent(List<MediaItem> UploadedMedia, MediaType mediaType)
        //{
        //    if (FloodEvent is not null && UploadedMedia.Count != 0)
        //    {
        //        try
        //        {
        //            await mediaItemReposirory.AddMediaToFloodEvent(FloodEvent, mediaType, UploadedMedia);
        //        }
        //        catch (Exception ex)
        //        {
        //            await RefreshComponent();
        //            logger.LogError(ex, "An error occurred adding media to a flood event");
        //        }
        //    }
        //}

        private async Task RefreshComponent()
        { 
            if (FloodEvent is not null)
            {
                FloodEvent = await floodEventRepository.GetFloodEventById(FloodEvent.Id, _cts.Token);
                StateHasChanged();
            }
        }

        async ValueTask IAsyncDisposable.DisposeAsync()
        {
            try
            {
                if (_jsModule != null)
                {
                    await _jsModule.DisposeAsync();
                }
            }
            catch (JSDisconnectedException)
            {
                // Ignore as it doesn't matter
            }
            finally
            {
                GC.SuppressFinalize(this);
            }
        }
    }
}
