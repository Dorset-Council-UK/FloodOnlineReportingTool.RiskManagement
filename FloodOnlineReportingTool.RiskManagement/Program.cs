using FloodOnlineReportingTool.RiskManagement.Data.Data;
using FloodOnlineReportingTool.RiskManagement.Options;
using FloodOnlineReportingTool.RiskManagement.Settings;
using FloodOnlineReportingTool.RiskManagement.Services;
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

// API settings
var apiSection = builder.Configuration.GetSection(ApiSettings.SectionName);
builder.Services.Configure<ApiSettings>(apiSection);
var apiSettings = apiSection.Get<ApiSettings>() ?? new ApiSettings { BaseUrl = "https://localhost:7065/" };

// Configure all the settings.
var (riskManagementSettings, messagingSettings) = builder.Services.AddRiskManagementSettings(builder.Configuration);

// Health checks
builder.Services.AddRiskManagementHealthChecks(messagingSettings);

// Message system
builder.Services.AddMessageSystem(messagingSettings);

//API client - improved configuration with resilience and configurable settings
builder.Services.AddHttpClient("ApiClient", client =>
{
    client.BaseAddress = new Uri(apiSettings.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(apiSettings.TimeoutSeconds);
    client.DefaultRequestHeaders.Add("User-Agent", apiSettings.UserAgent);
})
.AddStandardResilienceHandler();

// Register API service
builder.Services.AddScoped<IApiService, ApiService>();

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
