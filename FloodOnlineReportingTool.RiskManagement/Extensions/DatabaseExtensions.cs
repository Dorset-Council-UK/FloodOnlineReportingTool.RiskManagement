using FloodOnlineReportingTool.RiskManagement.Data.DbContexts;
using FloodOnlineReportingTool.RiskManagement.Exceptions;
using Microsoft.EntityFrameworkCore;
using Npgsql;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.AspNetCore.Builder;
#pragma warning restore IDE0130 // Namespace does not match folder structure

internal static class DatabaseExtensions
{
    internal static TBuilder AddDatabase<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        var logSensitiveData = builder.Environment.IsDevelopment();
        var connectionString = builder.Configuration.GetRiskManagementConnectionString(logSensitiveData);

        // Add the database context
        builder.Services.AddDbContextPool<RiskManagementDbContext>(options =>
        {
            options.UseNpgsql(connectionString, o =>
            {
                o.EnableRetryOnFailure(3);
                o.MigrationsHistoryTable("__EFMigrationsHistory", SchemaNames.RiskManagement);
            });
            options.EnableSensitiveDataLogging(logSensitiveData);
        });

        return builder;
    }

    private static string GetRiskManagementConnectionString(this IConfigurationManager configuration, bool logSensitiveData)
    {
        var connectionString = configuration.GetConnectionString("FloodReportingRiskManagement");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ConfigurationMissingException("Missing configuration setting: The connection string for the database is missing.");
        }

        var connectionStringBuilder = new NpgsqlConnectionStringBuilder(connectionString)
        {
            IncludeErrorDetail = logSensitiveData,
        };

        // Double check that a schema has been set
        if (string.IsNullOrWhiteSpace(connectionStringBuilder.SearchPath))
        {
            throw new ConfigurationMissingException("Missing configuration setting: The connection string for the database needs a schema set in the Search Path parameter.");
        }
        if (!connectionStringBuilder.SearchPath.Equals(SchemaNames.RiskManagement, StringComparison.Ordinal))
        {
            throw new ConfigurationMissingException($"Invalid configuration setting: The connection string for the database has an invalid schema set in the Search Path parameter. Expected '{SchemaNames.RiskManagement}', but found '{connectionStringBuilder.SearchPath}'.");
        }

        return connectionStringBuilder.ToString();
    }
}
