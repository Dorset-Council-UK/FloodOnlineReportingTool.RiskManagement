using FloodOnlineReportingTool.RiskManagement.Exceptions;
using FloodOnlineReportingTool.RiskManagement.Settings;

namespace Microsoft.AspNetCore.Builder;

internal static class RiskManagementSettingsExtensions
{
    public static (RiskManagementSettings, MessagingSettings) AddRiskManagementSettings(this IServiceCollection services, IConfigurationManager configuration)
    {
        // Sections
        var riskManagementSection = configuration.GetSection(RiskManagementSettings.SectionName);
        var messagingSection = configuration.GetSection(MessagingSettings.SectionName);
        if (riskManagementSection == null)
        {
            throw new ConfigurationMissingException($"Configuration section '{RiskManagementSettings.SectionName}' is missing.");
        }
        if (messagingSection == null)
        {
            throw new ConfigurationMissingException($"Configuration section '{MessagingSettings.SectionName}' is missing.");
        }

        // Configure settings
        services.Configure<RiskManagementSettings>(riskManagementSection);
        services.Configure<MessagingSettings>(messagingSection);

        // Settings
        var riskManagementSettings = riskManagementSection.Get<RiskManagementSettings>();
        var messagingSettings = messagingSection.Get<MessagingSettings>();
        if (riskManagementSettings == null)
        {
            throw new ConfigurationMissingException($"Configuration section '{RiskManagementSettings.SectionName}' is not properly defined.");
        }
        if (messagingSettings == null)
        {
            throw new ConfigurationMissingException($"Configuration section '{MessagingSettings.SectionName}' is not properly defined.");
        }

        return (riskManagementSettings, messagingSettings);
    }
}
