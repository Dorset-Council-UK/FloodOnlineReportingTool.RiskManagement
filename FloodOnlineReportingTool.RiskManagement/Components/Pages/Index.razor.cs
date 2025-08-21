using FloodOnlineReportingTool.RiskManagement.Services;
using GdsBlazorComponents;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System.Reflection;

namespace FloodOnlineReportingTool.RiskManagement.Components.Pages;

public partial class Index(IGdsJsInterop gdsJs, IApiService apiService, NavigationManager navigationManager)
{
    public string Title { get; set; } = "Flood Reporting Admin";
    private bool _isLoading = true;
    private EditContext _editContext = default!;
    private ValidationMessageStore _messageStore = default!;

    private Models.Searches.HomeSearch Model { get; set; } = default!;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await gdsJs.InitGds();

            _isLoading = false;
            StateHasChanged();
        }
    }

    protected override void OnInitialized()
    {
        // Setup model and edit context
        Model ??= new();
        _editContext = new(Model);
        _editContext.SetFieldCssClassProvider(new GdsFieldCssClassProvider());
        _messageStore = new ValidationMessageStore(_editContext);
    }


    private async Task OnValidSubmit()
    {
        _messageStore.Clear();
        if (string.IsNullOrEmpty(Model.SearchText))
        {
            return;
        }
        try
        {
            var report = await apiService.GetReportByIdAsync(Model.SearchText);
            if (report != null)
            {
                //redirect to report view page
                navigationManager.NavigateTo($"flood-report/details/{Uri.EscapeDataString(report.Id.ToString())}");
            }
            else
            {
                _messageStore.Add(() => Model.SearchText, [$"No flood reports could be found with the ID {Model.SearchText}"]);
            }

        }
        catch(Exception ex)
        {
            _messageStore.Add(() => Model.SearchText, ["There was an error getting the flood reports"]);
        }

    }
}
