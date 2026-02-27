using FloodOnlineReportingTool.RiskManagement.Data.DbContexts;
using FloodOnlineReportingTool.RiskManagement.Exceptions;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Microsoft.AspNetCore.Builder;

internal static class DatabaseExtensions
{
    extension(IHostApplicationBuilder builder)
    {
        internal IHostApplicationBuilder AddDatabase()
        {
            var logSensitiveData = builder.Environment.IsDevelopment();
            var connectionString = builder.Configuration.GetRiskManagementConnectionString(logSensitiveData);

            // Add the database context
            builder.Services.AddDbContextFactory<RiskManagementDbContext>(options =>
            {
                options.UseNpgsql(connectionString, o =>
                {
                    o.EnableRetryOnFailure(3);
                    o.MigrationsHistoryTable("__EFMigrationsHistory", SchemaNames.RiskManagement);
                    o.UseNetTopologySuite();
                });
                options.EnableSensitiveDataLogging(logSensitiveData);
            });

            return builder;
        }
    }

    extension(IConfigurationManager configuration)
    {
        internal string GetRiskManagementConnectionString(bool logSensitiveData)
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
            if (!connectionStringBuilder.SearchPath.Contains(SchemaNames.RiskManagement, StringComparison.Ordinal))
            {
                throw new ConfigurationMissingException($"Invalid configuration setting: The connection string for the database has an invalid schema set in the Search Path parameter. Expected '{SchemaNames.RiskManagement}', but found '{connectionStringBuilder.SearchPath}'.");
            }

            return connectionStringBuilder.ToString();
        }
    }
}
