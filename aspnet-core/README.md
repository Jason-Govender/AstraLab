# AstraLab Backend

## Overview

The AstraLab backend is the API and application core for authentication, dataset management, profiling, transformations, AI integration, machine learning orchestration, analytics summaries, reporting, and export storage. It is implemented as a layered ASP.NET Boilerplate solution with clear separation between domain logic, application workflows, infrastructure, and web hosting.

## Solution Layout

The main backend projects are:

- `AstraLab.Web.Host`
  - Runnable API host and primary startup project.
- `AstraLab.Web.Core`
  - Shared web infrastructure, controllers, and web-layer integration code.
- `AstraLab.Application`
  - Application services, DTOs, orchestration logic, and use-case workflows.
- `AstraLab.Core`
  - Domain entities, domain services, and business rules.
- `AstraLab.EntityFrameworkCore`
  - Entity Framework Core persistence, mappings, migrations, and repositories.
- `AstraLab.Migrator`
  - Standalone migration runner for database initialization and updates.

For the canonical folder and layering rules, see [BACKEND_STRUCTURE.md](./BACKEND_STRUCTURE.md).

## Startup and Migration Paths

### API host

Primary host project:

- `src/AstraLab.Web.Host/AstraLab.Web.Host.csproj`

### Migrator

Database migration runner:

- `src/AstraLab.Migrator/AstraLab.Migrator.csproj`

Use the migrator when you need to apply schema changes before starting the host.

## Configuration Overview

Backend configuration is primarily defined in `src/AstraLab.Web.Host/appsettings.json` and environment-specific overrides.

Important configuration areas include:

- `ConnectionStrings`
  - Database connection settings for the main application database.
- `App`
  - Root URLs and CORS-related application settings.
- `Authentication`
  - JWT bearer authentication configuration for protected API access.
- `DatasetStorage`
  - Raw dataset storage provider and local filesystem fallback settings.
- `ObjectStorage`
  - S3-compatible object storage settings for dataset files, ML artifacts, and analytics exports.
- `MLExecution`
  - ML executor base URL, callback settings, shared secret, and artifact storage defaults.
- `AI`
  - AI provider settings such as base URL, API key, model selection, timeout, and generation controls.
- `Swagger`
  - Swagger and API documentation behavior.

Describe these settings in local configuration or environment variables, but do not commit real secrets or production credentials.

## Local Development

### Visual Studio workflow

1. Open `AstraLab.sln`.
2. Set `AstraLab.Web.Host` as the startup project.
3. Run `AstraLab.Migrator` when schema updates need to be applied.
4. Start the host under IIS Express or the standard project runner.

### CLI workflow

Build the solution:

```bash
dotnet build AstraLab.sln
```

Run the migrator:

```bash
dotnet run --project src/AstraLab.Migrator/AstraLab.Migrator.csproj
```

Run the API host:

```bash
dotnet run --project src/AstraLab.Web.Host/AstraLab.Web.Host.csproj
```

## Architecture Guidance

The backend follows the repository-wide architecture rules documented in [BACKEND_STRUCTURE.md](./BACKEND_STRUCTURE.md). When adding new backend capabilities:

- keep `AstraLab.*` project names unchanged
- place domain entities and domain services under `Core/Domains/<Capability>/`
- place app services and DTOs under `Application/Services/<Capability>/`
- keep business logic out of `Web.Core` and `Web.Host`

## Related Documentation

- [Root README](../README.md)
- [Frontend README](../frontend/README.md)
- [ML Executor README](../ml-executor/README.md)
