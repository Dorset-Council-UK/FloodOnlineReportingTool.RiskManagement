using FloodOnlineReportingTool.RiskManagement.Data.Data;
using FloodOnlineReportingTool.RiskManagement.Options;
using FloodOnlineReportingTool.RiskManagement.Settings;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using System.Configuration;

var builder = WebApplication.CreateBuilder(args);

builder
    .AddFORTAzureKeyVault()
    .AddFORTOptions();    

var assembly = typeof(Program).Assembly;

builder.Services.AddRiskManagementAuthentication(builder.Configuration);

// Risk Management settings
var section = builder.Configuration.GetSection(RiskManagementSettings.SectionName);
builder.Services.Configure<RiskManagementSettings>(section);
var settings = section.Get<RiskManagementSettings>();
// Configure all the settings.
var (riskManagementSettings, messagingSettings) = builder.Services.AddRiskManagementSettings(builder.Configuration);

// Health checks
builder.Services.AddRiskManagementHealthChecks(messagingSettings);

// Message system
builder.Services.AddMessageSystem(messagingSettings);

// Configure the database context
var connectionString = builder.Configuration.GetConnectionString("FloodReportingRiskManagement");
if (string.IsNullOrEmpty(connectionString))
{
    throw new ConfigurationErrorsException("Connection string was empty");
}

builder.Services.AddDbContextPool<RiskManagementDbContext>(options =>
{
    options.UseNpgsql(connectionString, x =>
    {
        x.MigrationsHistoryTable("__EFMigrationsHistory", "fortriskmanagement");
    });

    options.EnableSensitiveDataLogging(builder.Environment.IsDevelopment());
});

// Add Blazor services
builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddScoped<GdsBlazorComponents.IGdsJsInterop, GdsBlazorComponents.GdsJsInterop>();

// Add all the validation rules
builder.Services.AddValidatorsFromAssembly(assembly);

var app = builder.Build();

var pathBase = string.IsNullOrWhiteSpace(riskManagementSettings?.PathBase) ? "/" : $"/{riskManagementSettings.PathBase}";
app.UsePathBase(pathBase);

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

var sectionProxySettings = builder.Configuration.GetSection(ProxyOptions.SectionName);
var proxyOptions = sectionProxySettings.Get<ProxyOptions>();
if (proxyOptions is not null && proxyOptions.UseHttpsRedirection)
{
    app.UseHttpsRedirection();
}

app.MapRiskManagementHealthChecks();
app.UseAntiforgery();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorComponents<FloodOnlineReportingTool.RiskManagement.Components.App>()
   .AddInteractiveServerRenderMode();

await app.RunAsync()
         .ConfigureAwait(false);
