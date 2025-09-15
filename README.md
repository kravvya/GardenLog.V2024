# GardenLog.V2024

A comprehensive garden management application built with modern .NET technologies. It provides tools for plant cataloging, harvest tracking, user management, growing conditions monitoring, and image management for gardening enthusiasts and professionals.

## Architecture

- **Platform**: .NET 9.0
- **Frontend**: Blazor WebAssembly
- **Architecture**: Clean Architecture with Domain-Driven Design (DDD)
- **Database**: MongoDB
- **Authentication**: ASP.NET Core Identity with OAuth2/OpenID Connect
- **Infrastructure**: Azure services integration (Key Vault, Blob Storage)
- **Containerization**: Docker support for all API services

## Getting Started

### Prerequisites

- .NET 9.0 SDK
- Visual Studio 2022 or VS Code
- Docker Desktop (for containerized services)
- MongoDB (local or cloud instance)

### Development Setup

1. Clone the repository
2. Configure your development settings in `appsettings.Development.json` files
3. Set up MongoDB connection strings
4. Configure authentication providers for local development
5. Build and run the solution

```bash
# Build the entire solution
dotnet build GardenLogV2024.sln

# Run the Blazor WebAssembly frontend
dotnet run --project src/GardenLogWeb/GardenLogWeb.csproj

# Run tests
dotnet test
```

## GitHub Copilot Configuration

This repository includes comprehensive GitHub Copilot instructions in `.copilot-instructions.md` to help AI assistants understand the project structure, domain concepts, and development practices. The instructions cover:

- Project architecture and technologies
- Domain knowledge for gardening applications
- Coding standards and patterns
- Testing strategies
- Common development scenarios

When working with GitHub Copilot or other AI coding assistants, refer to the `.copilot-instructions.md` file for context-aware assistance.

## Chrome localhost Certificate (Development)

To force CHROME to "trust" localhost certificate:

Simply visit this link in your Chrome:

chrome://flags/#temporary-unexpire-flags-m118
You should see highlighted text saying:

Temporarily unexpire flags that expired as of M118. These flags will be removed soon. – Mac, Windows, Linux, ChromeOS, Android, Fuchsia, Lacros

Click Enable Then relaunch Chrome.