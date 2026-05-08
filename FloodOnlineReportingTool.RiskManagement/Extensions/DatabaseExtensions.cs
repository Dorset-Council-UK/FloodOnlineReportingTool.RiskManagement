using FloodOnlineReportingTool.RiskManagement.Data.DbContexts;
using FloodOnlineReportingTool.RiskManagement.Data.Repositories;
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

            // Register repositories
            builder.Services.AddScoped<ICommonRepository, CommonRepository>();
            builder.Services.AddScoped<IActionLogRepository, ActionLogRepository>();

            return builder;
        }
    }

    extension(IConfigurationManager configuration)
    {
        internal string GetRiskManagementConnectionString(bool logSensitiveData)
        {
            const string connectionStringName = "FloodReportingRiskManagement";
            const string searchPathErrorTemplate = "Invalid connection string '{0}': Search Path must include the '{1}' schema. Example: SearchPath={1},{2}";

            var connectionString = configuration.GetConnectionString(connectionStringName);
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
            string[] searchPaths = connectionStringBuilder.SearchPath.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (!searchPaths.Contains(SchemaNames.RiskManagement, StringComparer.OrdinalIgnoreCase))
            {
                throw new ConfigurationMissingException(string.Format(searchPathErrorTemplate, connectionStringName, SchemaNames.RiskManagement, SchemaNames.Public));
            }
            if (!searchPaths.Contains(SchemaNames.Public, StringComparer.OrdinalIgnoreCase))
            {
                throw new ConfigurationMissingException(string.Format(searchPathErrorTemplate, connectionStringName, SchemaNames.Public, SchemaNames.RiskManagement));
            }

            return connectionStringBuilder.ToString();
        }
    }
}
