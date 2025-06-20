using FloodOnlineReportingTool.RiskManagement.Settings;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

var assembly = typeof(Program).Assembly;

// Risk Management settings
var section = builder.Configuration.GetSection(RiskManagementSettings.SectionName);
builder.Services.Configure<RiskManagementSettings>(section);
var settings = section.Get<RiskManagementSettings>();

// Health checks
builder.Services.AddRiskManagementHealthChecks();

// Add the authentication and authorization
builder.Services.AddAuthentication();
builder.Services
    .AddCascadingAuthenticationState()
    .AddAuthorizationBuilder();

// Add Blazor services
builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddScoped<GdsBlazorComponents.IGdsJsInterop, GdsBlazorComponents.GdsJsInterop>();

// Add all the validation rules
builder.Services.AddValidatorsFromAssembly(assembly);

var app = builder.Build();

var pathBase = string.IsNullOrWhiteSpace(settings?.PathBase) ? "/" : $"/{settings.PathBase}";
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
