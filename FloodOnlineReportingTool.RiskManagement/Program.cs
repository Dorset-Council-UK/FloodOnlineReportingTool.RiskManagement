using FloodOnlineReportingTool.RiskManagement.Components;
using FluentValidation;
using Microsoft.Identity.Web;

var builder = WebApplication.CreateBuilder(args);

// Settings
builder.AddFORTAzureKeyVault();
var proxySettings = builder.AddProxySettings();
var riskManagementSettings = builder.AddSettings();
var mesasgingSettings = builder.AddMessagingSettings();

builder.AddAuthentication();
builder.AddRiskManagementHealthChecks();
builder.AddMessageSystem(mesasgingSettings);
builder.AddDatabase();

// Add Blazor services
builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddMicrosoftIdentityConsentHandler();
builder.Services.AddScoped<GdsBlazorComponents.IGdsJsInterop, GdsBlazorComponents.GdsJsInterop>();

// Add all the validation rules
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

var app = builder.Build();

app.UsePathBase($"/{riskManagementSettings?.PathBase}");

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

var useHttpRedirection = proxySettings?.UseHttpsRedirection ?? true;
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

await app.RunAsync()
         .ConfigureAwait(false);
