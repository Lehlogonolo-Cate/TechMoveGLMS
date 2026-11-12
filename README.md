# TechMoveGLMS

TechMoveGLMS is a logistics management system developed using ASP.NET Core MVC, ASP.NET Core Web API, Entity Framework Core, SQL Server, JWT authentication and Docker.

## Features

- Secure JWT-based authentication
- Client management
- Contract management
- Service request management
- Search and filtering
- Currency conversion
- Swagger API documentation
- SQL Server persistence
- Integration testing with xUnit
- Docker Compose deployment

## Project Structure

- `TechMoveGLMS.Api` – REST API and database access
- `TechMoveGLMS.Web` – ASP.NET Core MVC user interface
- `TechMoveGLMS.Tests` – API integration tests

## Requirements

- .NET 10 SDK
- Docker Desktop
- Git

## Docker Setup

1. Copy `.env.example` to `.env`.
2. Replace the placeholder values with secure values.
3. Start the application:

```powershell
docker compose up --build -d