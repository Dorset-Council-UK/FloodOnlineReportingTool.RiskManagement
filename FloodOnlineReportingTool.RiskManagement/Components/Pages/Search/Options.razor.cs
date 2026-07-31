using FloodOnlineReportingTool.Database.Models.Lookups;
using FloodOnlineReportingTool.Database.Models.ResultModels;
using FloodOnlineReportingTool.Database.Repositories;
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
    private string[] _warnings = [];

    [Parameter]
    public string? SearchReference { get; set; }

    private IList<FloodProblem> _primaryFloodProblems = [];
    private Dictionary<string, bool> _selectedFloodSources = [];

    protected override async Task OnInitializedAsync()
    {
        // Setup model and edit context
        if (Model is null)
        {
            Model = new();
            EditContext = new(Model);
            EditContext.SetFieldCssClassProvider(new GdsFieldCssClassProvider());
            _messageStore = new(EditContext);
        }

        _primaryFloodProblems = await commonRepository.GetFloodProblems("Primary", _cts.Token);
        UpdateSelectedFloodSources();
    }

    protected override async Task OnParametersSetAsync()
    {
        // 1. data from repository
        bool hasReferenceChanged = SearchReference is not null && !string.Equals(searchOptionsState.Options?.SearchReference, SearchReference, StringComparison.OrdinalIgnoreCase);
        if (hasReferenceChanged)
        {
            var savedSearch = await savedSearchRepository.Get(SearchReference!, _cts.Token);
            if (savedSearch is not null)
            {
                Model.CopyFrom(savedSearch);
                searchOptionsState.SetOptions(Model);
                UpdateSelectedFloodSources();
            }
        }

        // 2. State - for example returning from the options map page
        if (!hasReferenceChanged && searchOptionsState.Options is not null)
        {
            Model.CopyFrom(searchOptionsState.Options);
            UpdateSelectedFloodSources();
        }

        // 3. Page parameters
        Model.SearchReference = SearchReference;
        _canEdit = Model.OverrideLock || !Model.IsSavedSearchLocked;

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
        _messageStore.Clear();
        _warnings = [];

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
                if (createResult.Severity == ResultSeverity.Warning)
                {
                    logger.LogWarning("Create saved search {SearchReference} completed with warnings: {Errors}", Model.SearchReference, createResult.Errors);
                    _warnings = [.. createResult.Errors];
                    return;
                }

                logger.LogError("Create saved search {SearchReference} failed with errors: {Errors}", Model.SearchReference, createResult.Errors);
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
            if (updateResult.Severity == ResultSeverity.Warning)
            {
                logger.LogWarning("Update of saved search {SearchReference} completed with warnings: {Errors}", Model.SearchReference, updateResult.Errors);
                _warnings = [.. updateResult.Errors];
                return;
            }

            logger.LogError("Update of saved search {SearchReference} failed with errors: {Errors}", Model.SearchReference, updateResult.Errors);
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
        searchOptionsState.SetOptions(Model);

        if (SearchReference is null)
        {
            navigationManager.NavigateTo(SearchPages.OptionsMap.Url);
            return;
        }

        navigationManager.NavigateTo($"{SearchPages.OptionsMap.Url}/{SearchReference}");
    }

    private void OnFloodSourceChanged(bool isChecked, Guid floodSourceId)
    {
        // update the model
        if (isChecked)
        {
            if (!Model.ByFloodSource.Contains(floodSourceId))
            {
                Model.ByFloodSource.Add(floodSourceId);
            }
        }
        else
        {
            Model.ByFloodSource.Remove(floodSourceId);
        }
    }

    /// <summary>
    /// Set up the selected flood sources (string, bool dictionary)
    /// </summary>
    private void UpdateSelectedFloodSources()
    {
        _selectedFloodSources = _primaryFloodProblems.ToDictionary(o => o.Id.ToString("N"), o => Model.ByFloodSource.Contains(o.Id), StringComparer.Ordinal);
    }
}
