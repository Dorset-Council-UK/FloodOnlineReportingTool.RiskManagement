using FloodOnlineReportingTool.RiskManagement.Data.Models.Lookups;
using FloodOnlineReportingTool.RiskManagement.Data.Models.SavedSearches;
using FloodOnlineReportingTool.RiskManagement.Data.Repositories;
using FloodOnlineReportingTool.RiskManagement.Extensions;
using FloodOnlineReportingTool.RiskManagement.Models.Order;
using FloodOnlineReportingTool.RiskManagement.Models.Search;
using FloodOnlineReportingTool.RiskManagement.Services.SavedStates;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using System.Security.Claims;

namespace FloodOnlineReportingTool.RiskManagement.Components.Pages.Search;

public partial class Options(
    ILogger<Options> logger,
    ICommonRepository commonRepository,
    ISavedSearchRepository savedSearchRepository,
    ISearchOptionsState searchOptionsState,
    NavigationManager navigationManager
) : IPageOrder, IAsyncDisposable
{
    public string Title { get; set; } = SearchPages.Options.Title;

    public IReadOnlyCollection<GdsBreadcrumb> Breadcrumbs { get; set; } = [
        GeneralPages.Home.ToGdsBreadcrumb(),
        SearchPages.Home.ToGdsBreadcrumb(),
        SearchPages.Options.ToGdsBreadcrumb(),
    ];

    [CascadingParameter]
    public Task<AuthenticationState>? AuthenticationState { get; set; }

    private SearchOptionsViewModel Model { get; set; } = default!;

    private EditContext EditContext = default!;
    private readonly CancellationTokenSource _cts = new();
    private ValidationMessageStore _messageStore = default!;
    private bool _isLoading = true;
    private bool _canEdit;

    [Parameter]
    public string? SearchReference { get; set; }

    private IReadOnlyCollection<GdsOptionItem<Guid>> _floodSourceOptions = [];

    protected override void OnInitialized()
    {
        // Setup model and edit context
        if (Model is null)
        {
            Model = new();
            EditContext = new(Model);
            EditContext.SetFieldCssClassProvider(new GdsFieldCssClassProvider());
            _messageStore = new(EditContext);
        }
    }

    protected override async Task OnParametersSetAsync()
    {
        // Configure option lists once on page load - No selections these are fixed option lists
        _floodSourceOptions = _floodSourceOptions.Count == 0 ? await CreateFloodSourceOptions() : _floodSourceOptions;

        // GdsInputDate caches the GdsDate object reference on first render, so the existing
        // objects must be mutated in-place rather than replaced to keep the component in sync.
        var options = searchOptionsState.Options;
        if (SearchReference is not null && !string.Equals(options?.SearchReference, SearchReference, StringComparison.OrdinalIgnoreCase))
        {
            // Replace the stored model with the details from the saved search by reference
            var savedSearch = await savedSearchRepository.Get(SearchReference, _cts.Token);
            if (savedSearch is not null)
            {
                options = savedSearch.ToSearchOptionsViewModel();
            }
        }
        if (options is not null)
        {
            Model = options;
            EditContext = new(Model);
            EditContext.SetFieldCssClassProvider(new GdsFieldCssClassProvider());
            Model.SearchReference = SearchReference;
            _canEdit = options.OverrideLock || !options.IsSavedSearchLocked;
        }

        SetGdsOptions();
        _isLoading = false;
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            await _cts.CancelAsync();
            _cts.Dispose();
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "An unexpected error occurred while disposing {ComponentName}.", nameof(Options));
        }

        GC.SuppressFinalize(this);
    }

    private async Task OnSubmit()
    {
        if (!_canEdit)
        {
            var errorField = EditContext.Field(nameof(Model.SearchReference));
            _messageStore.Add(errorField, "This search is locked and cannot be modified");
            EditContext.NotifyValidationStateChanged();
            return;
        }

        if (EditContext.Validate())
        {
            await OnValidSubmit();
        }
    }

    private async Task OnValidSubmit()
    {
        // Update flood cause selection from checkboxes
        Model.ByFloodSource = [.. _floodSourceOptions.Where(o => o.Selected).Select(o => o.Value)];
        
        // Persist in state for page navigation
        searchOptionsState.SetOptions(Model);
        
        // Convert ViewModel → Entity
        var savedSearchEntity = Model.OptionsViewToSavedSearch(searchTitle: null);
            
        // Get or create SavedSearch in database
        if (Model.SearchReference is null)
        {
            var createResult = await savedSearchRepository.Create(savedSearchEntity, _cts.Token);
            if (!createResult.IsSuccess)
            {
                var errorField = EditContext.Field(nameof(Model.SearchReference));
                foreach (var error in createResult.Errors)
                {
                    _messageStore.Add(errorField, error);
                }
                EditContext.NotifyValidationStateChanged();
                return;
            }

            navigationManager.NavigateTo($"{SearchPages.Results.Url}/{createResult.Value.Reference}");
            return;
        }

        savedSearchEntity.UpdatedBy = await GetUserName();
        var updateResult = await savedSearchRepository.Update(Model.SearchReference, savedSearchEntity, _cts.Token);
        if (!updateResult.IsSuccess)
        {
            var errorField = EditContext.Field(nameof(Model.SearchReference));
            foreach (var error in updateResult.Errors)
            {
                _messageStore.Add(errorField, error);
            }
            EditContext.NotifyValidationStateChanged();
            return;
        }

        navigationManager.NavigateTo($"{SearchPages.Results.Url}/{updateResult.Value.Reference}");
    }

    private async Task<string?> GetUserName()
    {
        if (AuthenticationState is null)
        {
            return null;
        }

        var authState = await AuthenticationState;
        return authState.User.DisplayName;
    }

    private void GoToMap()
    {
        Model.ByFloodSource = [.. _floodSourceOptions.Where(o => o.Selected).Select(o => o.Value)];
        searchOptionsState.SetOptions(Model);

        if (SearchReference is null)
        {
            navigationManager.NavigateTo(SearchPages.OptionsMap.Url);
            return;
        }

        navigationManager.NavigateTo($"{SearchPages.OptionsMap.Url}/{SearchReference}");
    }

    private void SetGdsOptions()
    {
        foreach(var options in _floodSourceOptions)
        {
            if (Model.ByFloodSource.Contains(options.Value))
            {
                options.Selected = true;
            }
            else
            {
                options.Selected = false;
            }
        }
    }

    private async Task<IReadOnlyCollection<GdsOptionItem<Guid>>> CreateFloodSourceOptions()
    {
        const string idPrefix = "flood-source";
        var floodProblems = await commonRepository.GetFloodProblems("Primary", _cts.Token);
        return [.. floodProblems.Select((o, idx) => CreateFloodOption(o, idPrefix))];
    }

    private static GdsOptionItem<Guid> CreateFloodOption(FloodProblem floodProblem, string idPrefix)
    {
        var id = $"{idPrefix}-{floodProblem.Id}".AsSpan();
        var label = floodProblem.TypeName.AsSpan();

        return new GdsOptionItem<Guid>(id, label, floodProblem.Id, false);
    }

}
