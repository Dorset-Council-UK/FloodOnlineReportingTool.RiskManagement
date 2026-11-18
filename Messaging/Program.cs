using Azure.Identity;
using Azure.Messaging.ServiceBus;
using FloodOnlineReportingTool.Database.DbContexts;
using FloodOnlineReportingTool.Database.Repositories;
using Messaging;
using Messaging.Consumers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;
using Messaging.Options;

var builder = Host.CreateApplicationBuilder(args);

var section = builder.Configuration
                .GetSection(KeyVaultOptions.SectionName);

var keyVaultOptions = section.Get<KeyVaultOptions>();

if (!string.IsNullOrWhiteSpace(keyVaultOptions?.VaultName))
{
    builder.Configuration.AddAzureKeyVault(
                new Uri($"https://{keyVaultOptions.VaultName}.vault.azure.net/"),
                new DefaultAzureCredential());
}

builder.Services
    .AddScoped<IActionLogRepository, ActionLogRepository>();

// Consumers
builder.Services
    .AddSingleton<IInboxConsumer, FloodReportCreatedConsumer>();

builder.Services.AddHostedService<OutboxWorker>();
builder.Services.AddHostedService<InboxWorker>();

// add database
string? databaseConnectionString = builder.Configuration.GetConnectionString("FloodReportingRiskManagement");
if (string.IsNullOrWhiteSpace(databaseConnectionString))
{
    throw new InvalidOperationException("Missing configuration setting: The database connection string 'FloodReportingRiskManagement' is missing.");
}
builder.Services.AddDbContextFactory<RiskManagementDbContext>(options => 
    options.UseNpgsql(databaseConnectionString, npgsqlOptions => npgsqlOptions.UseNetTopologySuite()));

// add service bus
string? serviceBusConnectionString = builder.Configuration.GetConnectionString("service-bus");
if (string.IsNullOrWhiteSpace(serviceBusConnectionString))
{
    throw new InvalidOperationException("Missing configuration setting: The service bus connection string 'service-bus' is missing.");
}
builder.Services.AddAzureClients(clientBuilder =>
{
    clientBuilder
        .AddServiceBusClient(serviceBusConnectionString)
        .ConfigureOptions(options =>
        {
            options.TransportType = ServiceBusTransportType.AmqpWebSockets;
        });
});

await builder.Build().RunAsync();