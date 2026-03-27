# AstraLab Backend - Architecture & Folder Structure Guide

## Overview

The AstraLab backend is a multi-tenant SaaS platform built on ASP.NET Boilerplate (ABP). It follows a layered modular-monolith structure that separates domain logic, application orchestration, persistence, shared web infrastructure, and the runnable API host.

This document is the canonical backend structure guide for this repository. It defines where new backend features belong and how they should be added without renaming the existing `AstraLab.*` projects.

## Solution Structure

```text
aspnet-core/
├── src/
│   ├── AstraLab.Core/                        # Domain layer
│   ├── AstraLab.Application/                 # Application service layer
│   ├── AstraLab.EntityFrameworkCore/         # Data access / infrastructure layer
│   ├── AstraLab.Web.Core/                    # Shared web infrastructure
│   ├── AstraLab.Web.Host/                    # API host / startup project
│   └── AstraLab.Migrator/                    # Standalone migration runner
├── test/
│   ├── AstraLab.Tests/                       # Unit and integration tests
│   └── AstraLab.Web.Tests/                   # Web / HTTP-level tests
└── AstraLab.sln
```

## Layer Dependencies

Dependency direction is one-way:

```text
Web.Host
  └── Web.Core
        └── Application
              ├── Core
              └── EntityFrameworkCore
                    └── Core
```

Rules:

- No project may reference a layer above it.
- Project renaming is out of scope and unnecessary.
- We adopt the structure and conventions described here without renaming existing assemblies, namespaces, solution files, or modules.

## Layer Breakdown

### 1. `AstraLab.Core` - Domain Layer

Purpose:

- Owns business entities, value objects, domain services, invariants, and cross-cutting domain concerns.
- Must not depend on application services, EF Core infrastructure, or HTTP concerns.

Current solution areas already present here include:

- `Authorization/`
- `Configuration/`
- `Editions/`
- `Features/`
- `Identity/`
- `Localization/`
- `MultiTenancy/`
- `Timing/`
- `Validation/`


Rules:

- Keep domain logic in the domain layer, not in app services.
- Prefer entity and domain-service boundaries that map to product capabilities.
- Put shared domain validation here when it is not transport-specific.
- Multi-tenancy must be considered from the start for new domain entities and workflows.

### 2. `AstraLab.Application` - Application Service Layer

Purpose:

- Orchestrates use cases.
- Exposes application services to the web layer.
- Holds DTOs, mapping profiles, and permission-protected use-case workflows.

Current solution areas already present here include:

- `Authorization/Accounts/`
- `Configuration/`
- `MultiTenancy/`
- `Roles/`
- `Sessions/`
- `Users/`

Future product-domain work should be organized, for example, under:

```text
AstraLab.Application/
├── Services/
│   ├── Analytics/
│   ├── Billing/
│   ├── Bots/
│   ├── Channels/
│   ├── Collaboration/
│   ├── Deployments/
│   ├── Integrations/
│   ├── Knowledge/
│   ├── Runtime/
│   ├── Templates/
│   └── Transcripts/
```

Feature placement rules:

- Domain entities and domain services live in `Core/Domains/<Capability>/`.
- App services and DTOs live under `Application/Services/<Capability>/`.
- A typical feature shape is:

```text
Application/Services/Bots/BotDefinitionService/
├── IBotDefinitionAppService.cs
├── BotDefinitionAppService.cs
└── Dto/
    ├── BotDefinitionDto.cs
    ├── CreateBotDefinitionDto.cs
    └── UpdateBotDefinitionDto.cs
```

Rules:

- Every app service should have a matching interface.
- App services orchestrate use cases and permissions, but should not own business invariants that belong in the domain.
- DTOs must stay out of the domain layer.
- Persistence should be accessed through repository abstractions, not directly from controllers.

### 3. `AstraLab.EntityFrameworkCore` - Data Access / Infrastructure Layer

Purpose:

- Owns EF Core persistence, migrations, seeding, and database configuration.
- Registers database access for domain entities.

Current key files:

- `EntityFrameworkCore/AstraLabDbContext.cs`
- `EntityFrameworkCore/AstraLabDbContextConfigurer.cs`
- `EntityFrameworkCore/AstraLabDbContextFactory.cs`
- `EntityFrameworkCore/AbpZeroDbMigrator.cs`
- `EntityFrameworkCore/Repositories/`
- `EntityFrameworkCore/Seed/`
- `Migrations/`

Rules:

- Each persisted domain entity must be registered in `AstraLabDbContext`.
- Use `OnModelCreating` for configuration that should not live in annotations.
- New migrations belong here.
- This layer supports PostgreSQL-backed persistence and can later include infrastructure extensions such as Redis-backed caching where appropriate.

### 4. `AstraLab.Web.Core` - Shared Web Infrastructure

Purpose:

- Provides shared web concerns used by the host and tests.
- Handles token auth, shared controller base classes, request models, and web infrastructure plumbing.

Current key areas:

- `Authentication/External/`
- `Authentication/JwtBearer/`
- `Controllers/`
- `Models/`

Rules:

- Keep transport and auth plumbing here.
- Do not put business rules here.
- Shared request/response transport models may live here when they are specifically web-facing.

### 5. `AstraLab.Web.Host` - Presentation / API Host Layer

Purpose:

- Startup project and runnable host.
- Configures middleware, CORS, Swagger, SignalR, and application startup.

Current key pieces:

- `Startup/Program.cs`
- `Startup/Startup.cs`
- `Startup/AstraLabWebHostModule.cs`
- `Controllers/`
- `appsettings.json`
- `wwwroot/swagger/ui/`

Current runtime concerns already present:

- JWT authentication
- Swagger / OpenAPI
- SignalR
- CORS
- ABP startup and middleware wiring

Rules:

- No business logic belongs here.
- Controllers and startup code should delegate to application services and infrastructure.
- Configuration changes should be environment-aware and not hardcode deployment secrets.

### 6. `AstraLab.Migrator` - Database Migration Runner

Purpose:

- Applies pending database migrations without running the full web host.

Current key files:

- `Program.cs`
- `AstraLabMigratorModule.cs`
- `appsettings.json`

### 7. Test Projects

```text
test/
├── AstraLab.Tests/
└── AstraLab.Web.Tests/
```

Rules:

- Domain and application behavior should be covered in `AstraLab.Tests`.
- HTTP and web-host behavior should be covered in `AstraLab.Web.Tests`.
- New feature tests should mirror the feature area they validate.


