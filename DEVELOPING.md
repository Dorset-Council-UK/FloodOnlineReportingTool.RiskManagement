
# Developing for Flood Online Reporting Tool - Risk Management

Thank you for your interest in contributing to the Flood Online Reporting Tool - Risk Management! This guide will help you set up your development environment and understand the basic components of the project.

## Dependencies

To run Flood Online Reporting Tool - Risk Management with minimal modification, you will need the following:

- **.NET 9**: Ensure you have the .NET 9 SDK installed.
- **PostgreSQL 13+**: This is the default database provider. The project uses Entity Framework, making it adaptable to other providers with minimal effort.
- **Message system (optional)**: The project includes a messaging system using Azure Service Bus. You can enable or disable this system through the `Messaging:Enabled` setting in your configuration.

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
    or with PowerShell command:
    ```powershell
    Update-Database -Project "FloodOnlineReportingTool.RiskManagement.Data" -StartupProject "FloodOnlineReportingTool.RiskManagement"
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

- **Messaging**: Used for the optional messaging system.
- **ConnectionStrings**: Required for database access.

Example secrets file:
```json
{
  "ConnectionStrings": {
    "RiskManagement": "Host=localhost;Port=5432;Database=YourDatabaseName;Username=YourUserName;Password=YourPassword;SearchPath=riskmanagement"
  },
  "Messaging": {
    "Enabled": "true",
    "ConnectionString": "sb://your-namespace.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=your-key-here",
    "QueueName": "fortmessaging-queue",
    "TopicName": "fortmessaging-topic",
    "SubscriptionName": "fortmessaging-subscription"
  }
}
```

## Further Help

For further help, please reach out to the maintainers or consult the documentation. 
