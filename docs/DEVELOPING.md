
# Developing for Flood Online Reporting Tool - Risk Management

Thank you for your interest in contributing to the Flood Online Reporting Tool - Risk Management! This guide will help you set up your development environment and understand the basic components of the project.

## Dependencies

To run Flood Online Reporting Tool - Risk Management with minimal modification, you will need the following:

- **.NET**: Ensure you have the .NET 10 SDK installed.
- **PostgreSQL 13+**: This is the default database provider. The project uses Entity Framework, making it adaptable to other providers with minimal effort.
- **PostGIS**: Database extension required for spatial data support in PostgreSQL. Using the public schema.
- **Message system**: The project includes a messaging system using Azure Service Bus. 

## Getting Started

1. **Clone the repository**:
    ```shell
    git clone <repository-url>
    cd <repository-folder>
    ```
2. **Create a new branch**:
    ```shell
    git checkout -b <branch-name>
    ```
3. **Install front-end dependencies**:

    Run the following command in the root folder:
    ```shell
    npm install
    ```
4. **Set up your secrets**:

    Configure the user secrets file for development. See the "User Secrets and Configuration" section below for details.

5. **Run migrations and seed the database**:

    The database schema and seed data are handled by the data project.
    Navigate to the solution folder and run the following command:
    ```shell
    dotnet ef database update --project "FloodOnlineReportingTool.RiskManagement.Data" --startup-project "FloodOnlineReportingTool.RiskManagement"
    ```
    or if using the Package Manager Console in Visual Studio, set the default project to FloodOnlineReportingTool.RiskManagement.Data and run:
   ```shell
   Update-Database -StartupProject FloodOnlineReportingTool.RiskManagement
   ```

## Update standards module

To update the standards module, navigate to the `FloodOnlineReportingTool.RiskManagement` solution folder and run:
```shell
git submodule update
```

## User Secrets and Configuration

You must set up user secrets for development. Run the following command in the terminal within the project folder:
- Using Visual Studio:

  Right click the project and select "Manage User Secrets".
- Using .NET CLI:
  ```shell
  dotnet user-secrets init
  ```
Then add your configuration:

- **ConnectionStrings**: Required for database access.

Example secrets file:
```json
{
  //These options may come from your secrets vault when deployed
  "ProxyOptions": {
    "UseHttpsRedirection": true,
    "UseForwardedHeadersMiddleware": false,
    "AllowedHosts": "*",
    "KnownProxies": []
  },
  "AzureAd": {
    // Configure Azure AD settings here if using Azure AD authentication
  },
  "ConnectionStrings": {
    "FloodReportingRiskManagement": "Host=localhost;Port=5432;Database=YourDatabaseName;Username=YourUserName;Password=YourPassword;SearchPath=fortriskmanagement,public"
  }
}
```
Then add your configuration for the Messaging project:

Example Messaging project secrets file:
```json
{
  "ConnectionStrings": {
    "FloodReportingRiskManagement": "Host=localhost;Port=5432;Database=YourDatabaseName;Username=YourUserName;Password=YourPassword;SearchPath=fortriskmanagement,public",
    "service-bus": "Endpoint=sb://your-namespace.servicebus.windows.net/;SharedAccessKeyName=your-shared-access-key-name;SharedAccessKey=your-shared-access-key"
  },
  "Messaging:TopicSuffix": "-name",
  "Inbox:SubscriptionName": "subscription-name"
}
```

## Further Help

For further help, please reach out to the maintainers or consult the documentation. 
