using FloodOnlineReportingTool.RiskManagement.Components;
using FloodOnlineReportingTool.Database.Repositories;
using FloodOnlineReportingTool.RiskManagement.Services;
using FloodOnlineReportingTool.RiskManagement.Services.SavedStates;
using FluentValidation;
using Microsoft.Identity.Web;

var builder = WebApplication.CreateBuilder(args);

// Add HttpContextAccessor for Blazor Server
builder.Services.AddHttpContextAccessor();

// Settings
builder.AddFORTAzureKeyVault();
var (riskManagementOptions, proxyOptions) = builder.AddRiskManagementOptions();

builder.AddAuthentication();
builder.AddRiskManagementHealthChecks();
builder.AddDatabase();

builder.Services.AddScoped<IReportStatusClient, ReportStatusClient>();
builder.Services.AddScoped<ISearchOptionsState, SearchOptionsState>();
builder.Services.AddScoped<IFloodEventState, FloodEventState>();
builder.Services.AddScoped<ISettingsService, SettingsService>();
builder.Services.AddScoped<IBlobStorageService, BlobStorageService>();
builder.Services.AddScoped<IMediaItemRepository, MediaItemRepository>();

// Add Blazor services
builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddMicrosoftIdentityConsentHandler();

// Add all the validation rules
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

var app = builder.Build();

app.UsePathBase($"/{riskManagementOptions?.PathBase}");

app.UseForwardedHeaders();

// Configure the HTTP request pipeline.
if (builder.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("Error", createScopeForErrors: true);
    app.UseHsts();
}

var useHttpRedirection = proxyOptions?.UseHttpsRedirection ?? true;
if (useHttpRedirection)
{
    app.UseHttpsRedirection();
}

app.MapRiskManagementHealthChecks();
app.UseAntiforgery();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorComponents<App>()
   .AddInteractiveServerRenderMode();
app.MapAuthenticationEndpoints();

await app.RunAsync();
