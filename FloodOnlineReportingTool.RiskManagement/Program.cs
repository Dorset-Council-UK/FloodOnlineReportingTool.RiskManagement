using System.Configuration;
using FloodOnlineReportingTool.RiskManagement.Data.Data;
using FloodOnlineReportingTool.RiskManagement.Settings;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var assembly = typeof(Program).Assembly;



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

// Add the authentication and authorization
builder.Services.AddAuthentication();
builder.Services
    .AddCascadingAuthenticationState()
    .AddAuthorizationBuilder();

// Configure the database context
var connectionString = builder.Configuration.GetConnectionString("RiskManagement");
if (string.IsNullOrEmpty(connectionString))
{
    throw new ConfigurationErrorsException("Connection string was empty");
}

builder.Services.AddDbContextPool<RiskManagementDbContext>(options =>
{
    options.UseNpgsql(connectionString, x =>
    {
        x.MigrationsHistoryTable("__EFMigrationsHistory", "riskmanagement");
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

app.UseHttpsRedirection();
app.MapRiskManagementHealthChecks();
app.UseRouting();
app.UseAntiforgery();
app.UseAuthentication();
app.UseAuthorization();
app.MapStaticAssets();
app.MapRazorComponents<FloodOnlineReportingTool.RiskManagement.Components.App>()
   .AddInteractiveServerRenderMode();

await app.RunAsync()
         .ConfigureAwait(false);
