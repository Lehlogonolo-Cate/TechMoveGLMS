# TechMoveGLMS

TechMoveGLMS is an ASP.NET Core MVC application for managing clients, service contracts, and service requests. It uses Entity Framework Core with SQL Server for data access and includes an xUnit test project for automated testing.

## Project links

- **GitHub repository:** https://github.com/Lehlogonolo-Cate/TechMoveGLMS
- **Application walkthrough:** https://youtu.be/gKglXAWc_9o

## Features

- Create, view, update, search, and manage client records.
- Manage contracts, including client links, contract status, and date filters.
- Upload and download PDF contract agreements with file validation.
- Create and manage service requests linked to contracts.
- Restrict service requests to contracts that are currently active.
- Retrieve the USD-to-ZAR exchange rate and calculate converted costs.
- Run automated unit tests with xUnit and GitHub Actions.

## Technology

- .NET 10 and ASP.NET Core MVC
- Entity Framework Core 10
- Microsoft SQL Server
- xUnit
- GitHub Actions
- Frankfurter API for USD-to-ZAR exchange rates

## Design overview

The application separates MVC request handling, business logic, and data access. EF Core maps the `Client`, `Contract`, and `ServiceRequest` entities and their relationships. The design follows the patterns identified in the project UML:

- **State:** contract status behavior.
- **Strategy:** currency conversion providers.
- **Factory Method:** service-request creation.

## Requirements

- .NET 10 SDK
- SQL Server or SQL Server LocalDB
- Internet access when retrieving a live exchange rate from the Frankfurter API

## Get started

Clone the repository and move into its folder:

```bash
git clone https://github.com/Lehlogonolo-Cate/TechMoveGLMS.git
cd TechMoveGLMS
```

Restore and build the solution:

```bash
dotnet restore
dotnet build
```

### Configure the database

Set the SQL Server connection string in `TechMoveGLMS.Web/appsettings.json` using the connection-string key expected by the application. Use a local development value and do not commit passwords or other private credentials.

The repository includes `DatabaseMigration.sql`. Review the script, confirm it targets your intended local database, and run it with SQL Server Management Studio (SSMS) or another SQL Server query tool.

### Run the application

```bash
dotnet run --project TechMoveGLMS.Web
```

Open the local URL printed by the application in your terminal.

## Run the tests

Run the full test project from the repository root:

```bash
dotnet test TechMoveGLMS.Tests/TechMoveGLMS.Tests.csproj
```

The tests cover service behavior, contract eligibility rules, PDF validation, and currency conversion cases. The repository also contains a GitHub Actions workflow that builds the solution and runs the tests when changes are pushed.

## Repository structure

```text
TechMoveGLMS/
├── TechMoveGLMS.Web/       # ASP.NET Core MVC application
├── TechMoveGLMS.Tests/     # xUnit automated tests
├── .github/workflows/      # GitHub Actions build and test workflow
└── DatabaseMigration.sql   # SQL Server database migration script
```

## Notes

- PDF agreement files are handled by the web application and validated before they are accepted.
- Currency conversion uses a live external API, so an internet connection is needed for that feature.
- Automated tests use an in-memory EF Core database for service testing; run the SQL migration separately to prepare a local SQL Server database.
