# AGENTS.md

Shared instructions for AI coding agents working on the ModularSaaS codebase.

## 1. Project Purpose & Technology Stack

ModularSaaS is a multi-tenant SaaS modular monolith backend built on Clean Architecture principles with folder-based modules and pluggable libraries.

- Framework: .NET 10 (`net10.0`), C# 13 (`LangVersion=latest`)
- Host: ASP.NET Core Web API (`apps/Api`)
- Persistence: Entity Framework Core 10, PostgreSQL (`Npgsql.EntityFrameworkCore.PostgreSQL`), Dapper for read-side queries
- Multi-Tenancy: Single database with `TenantId` discriminator column on `ITenantEntity`, isolated via EF Core Global Query Filters and scoped `ITenantContext`
- Security & Auth: JWT (HMAC-SHA256), refresh token rotation, BCrypt password hashing, granular 7-table RBAC with dynamic `[HasPermission]` authorization policy provider
- Mapping: Mapster with strict configuration
- Validation: FluentValidation
- Observability: OpenTelemetry (Tracing, Metrics, OTLP exporter), Serilog structured logging
- Architecture Testing: Automated Mono.Cecil reflection test suite (`tests/Architecture.Tests`)

## 2. Directory Structure & Responsibilities

```
ModularSaaS/
├── apps/
│   └── Api/                 # ASP.NET Core Web API host (Controllers, Middleware, Contracts, Program.cs DI)
├── src/
│   ├── Domain/              # Enterprise domain entities, value objects, domain enums, ITenantEntity contract
│   ├── Application/         # Use case services, interfaces (IRepository, IReader, IClock), models, validators
│   ├── Infrastructure/      # EF Core DbContext, migrations, repositories, readers, interceptors, security
│   └── Common/
│       ├── ModularSaaS.Security/       # Cross-cutting security, RBAC handler, BCrypt, and secure tokens
│       └── ModularSaaS.Observability/  # OpenTelemetry tracing, metrics, redaction, and exporter setup
├── tests/
│   └── Architecture.Tests/  # Mono.Cecil architectural boundary enforcement suite (Rules A1-A13)
├── docs/                    # Canonical architectural reference (docs/ARCHITECTURE.md, docs/migration/)
└── .agent/                  # Assistant rules and workflows (.agent/rules/, .agent/workflows/)
```

### Layer Responsibilities

- `apps/Api`: Single API host. Contains controllers, HTTP middleware, API request/response contracts, and Mapster host mapping profiles. Never references entities, repositories, `AppDbContext`, or Infrastructure types (except in `Program.cs` for DI registration).
- `src/Domain`: Enterprise core. Zero external package or project dependencies. Entities, value objects, domain enums, and domain exceptions. Tenant entities implement `ITenantEntity`.
- `src/Application`: Application business rules and orchestration. Organized into feature folders (`Identity`, `Tenancy`, `Shared`). Contains use cases, input/result models, validators, and abstraction interfaces. Public surface never exposes Domain entities or value objects.
- `src/Infrastructure`: Technical implementations. `AppDbContext` (internal), entity type configurations, migrations, repositories, readers, interceptors (`TenantInterceptor`, `AuditInterceptor`), and clock.
- `src/Common`: Cross-cutting building blocks (`ModularSaaS.Security`, `ModularSaaS.Observability`). Independent of application domain and persistence specifics.
- `tests/Architecture.Tests`: Architectural unit tests verifying layer dependencies, internal visibility, sealed models, and module isolation.

## 3. Development & Verification Commands

Run commands from the repository root:

- Build entire solution (must pass with 0 errors and 0 warnings):
  ```bash
  dotnet build ModularSaaS.slnx
  ```
- Run architecture enforcement tests (ratchet rule: no new violations):
  ```bash
  dotnet test tests/Architecture.Tests/ModularSaaS.Architecture.Tests.csproj
  ```
- Run full test suite:
  ```bash
  dotnet test ModularSaaS.slnx
  ```
- Run API host:
  ```bash
  dotnet run --project apps/Api/ModularSaaS.Api.csproj
  ```
- Check pending EF Core model changes:
  ```bash
  dotnet ef migrations has-pending-model-changes --project src/Infrastructure/ModularSaaS.Infrastructure.csproj --startup-project apps/Api/ModularSaaS.Api.csproj
  ```
- Add a new database migration:
  ```bash
  dotnet ef migrations add <MigrationName> --project src/Infrastructure/ModularSaaS.Infrastructure.csproj --startup-project apps/Api/ModularSaaS.Api.csproj --output-dir Persistence/Migrations
  ```
- Verify code formatting and style rules:
  ```bash
  dotnet format --verify-no-changes
  ```

## 4. Architecture & Dependency Boundaries

Strict inward dependency flow:
- `apps/Api` -> `src/Application` -> `src/Domain`
- `src/Infrastructure` -> `src/Application` -> `src/Domain`
- `src/Domain` references nothing.

### Core Invariants

1. Host boundaries:
   - Controllers and host contracts reference only `Application/{Module}/Abstractions` and `Application/{Module}/Models`.
   - Domain enums (namespace ending in `.Enums`) are the only Domain types permitted in host contracts.
   - Controllers NEVER reference entities, `DbContext`, repositories, or Infrastructure services.
2. Modular organization:
   - Modules are organized as vertical folders within each layer, not separate projects.
   - Namespaces must strictly match the folder path. File-scoped namespaces are mandatory.
   - Modules communicate across boundaries exclusively through Application `Abstractions`.
   - Cross-module entity references must use identifier values (`Guid`), never foreign entity navigation properties.
3. Read vs Write segregation:
   - Writes go through aggregate roots and `IRepository<T>` (one repository per aggregate root). Repositories do not expose `IQueryable` and do not call `SaveChanges` directly.
   - Reads (grids, lists, lookups, reports) go through dedicated readers (`IXxxReader`) implemented using EF Core read projections or Dapper stored procedures returning read models.
   - There is no MediatR, no message bus, and no domain event dispatcher. Multi-module workflows use explicit application services orchestrated inside an `IUnitOfWork` transaction.
4. Multi-tenancy isolation:
   - All tenant-owned entities implement `ITenantEntity`.
   - Tenant data is partitioned using EF Core global query filters keyed by scoped `ITenantContext`.
   - Bypassing query filters (`IgnoreQueryFilters()`) is forbidden by compiler analyzer (RS0030) except inside platform-scoped handlers.
   - Code operating in tenant scope must fail closed (`TenantRequiredException`) if no tenant is resolved.

## 5. Coding, Naming, and Formatting Conventions

Enforced through `Directory.Build.props`, `.editorconfig`, `stylecop.json`, and `BannedSymbols.txt`:

- Build rules: `TreatWarningsAsErrors=true`, `EnforceCodeStyleInBuild=true`, `Nullable=enable`, `ImplicitUsings=enable`.
- Namespaces: File-scoped namespaces only (`namespace ModularSaaS.Application.Identity.Services;`).
- Accessibility: Explicit accessibility modifiers required everywhere. Use `internal sealed` for services, validators, readers, and repositories.
- Sealed types: All DTO models, API request/response contracts, and internal classes must be `sealed` (compiler rule CA1852, architecture rule A12).
- Type shapes:
  - Input, Result, ListItem, Filter, Request, and Response models must be `sealed record` with `required` and `init` properties.
  - No DTO inheritance hierarchies.
- Time abstraction:
  - NEVER call `DateTime.Now`, `DateTime.UtcNow`, or `DateTimeOffset.UtcNow`. These are banned symbols (RS0030).
  - Always inject and use `IClock`.
- Mappings:
  - Strict Mapster mappings.
  - Entity to Result mapping belongs in Application `Mapping/`.
  - Contract to Model and Model to Response mapping belongs in Host `Mapping/`.
  - NEVER map input DTOs directly to domain entities using an automated mapper. Use domain constructors or factory methods.
- Indentation: 4 spaces for C# and SQL; 2 spaces for JSON, YAML, XML, and project files.
- Line endings: LF for source code and markdown; CRLF for SQL.

## 6. Testing Expectations

- Architecture tests (`tests/Architecture.Tests`) enforce rules A1 through A13 using Mono.Cecil:
  - A1: Domain has zero outward dependencies.
  - A2: Application does not depend on Infrastructure or Hosts.
  - A3: Infrastructure does not depend on Hosts.
  - A5/A5b: Hosts use Infrastructure only in `Program.cs`, and use only Application Abstractions, Models, Permissions, and Exceptions.
  - A6: Hosts access Domain exclusively through Enums.
  - A7: Hosts do not reference EF Core directly.
  - A8: Application public surface exposes no Domain types except Enums.
  - A9a/A9b: Cross-module calls use Abstractions/Models; never foreign Domain entities.
  - A10: Domain modules reference each other by ID only.
  - A11: Services, validators, and mappings are internal.
  - A12: All models and contracts are sealed.
  - A13: Domain enums reside exclusively in `.Enums` namespaces.
- Ratchet discipline: Baseline files in `tests/Architecture.Tests/Baseline/` allow legacy exceptions. You may only remove entries from baseline files. Never add new entries to make tests pass.

## 7. Configuration and Environment Variables

Configuration is loaded hierarchically from `apps/Api/appsettings.json`, environment variables, and user secrets.

Key settings schema:
- `ConnectionStrings:DefaultConnection` -> Environment: `ConnectionStrings__DefaultConnection`
- `Jwt:Secret` (minimum 32 characters / 256 bits) -> Environment: `Jwt__Secret`
- `Jwt:Issuer` -> Environment: `Jwt__Issuer`
- `Jwt:Audience` -> Environment: `Jwt__Audience`
- `Jwt:ExpiryMinutes` -> Default: `60`
- `Jwt:RefreshTokenDays` -> Default: `7`
- `Seed:PlatformAdminEmail`, `Seed:PlatformAdminPassword` -> Seed credentials for platform super-admin
- `Seed:DefaultTenantAdminEmail`, `Seed:DefaultTenantAdminPassword` -> Seed credentials for default tenant admin
- `Observability:Exporter` (`Console`, `Otlp`, `AzureMonitor`)
- `Observability:Otlp:Endpoint` -> Default: `http://localhost:4317`

Never commit secrets, production connection strings, or private keys to the repository.

## 8. Common Mistakes & Examples

1. Banned clock usage:
   - Bad: `var created = DateTime.UtcNow;`
   - Good: Inject `IClock clock;` and use `var created = clock.UtcNow;`
2. Leaking domain entities from Application:
   - Bad: `public Task<User?> GetUserAsync(Guid id);`
   - Good: `public Task<UserResult?> GetUserAsync(Guid id);`
3. Cross-module entity navigation properties:
   - Bad: `public Tenant Tenant { get; set; }` inside `User.cs`
   - Good: `public Guid TenantId { get; private init; }`
4. Direct DbContext or Repository usage in Controllers:
   - Bad: `public UsersController(AppDbContext db)` or `public UsersController(IUserRepository repo)`
   - Good: `public UsersController(IUserService userService, IIdentityReader identityReader)`
5. Unsealed DTOs or public classes:
   - Bad: `public record UserResponse { ... }`
   - Good: `public sealed record UserResponse { ... }`
6. Mapper abuse on write entities:
   - Bad: `var user = input.Adapt<User>();`
   - Good: `var user = User.Create(tenantId, input.Email, input.FirstName, input.LastName, passwordHash);`
7. Bypassing tenant filters:
   - Bad: Calling `.IgnoreQueryFilters()` inside regular tenant service queries.
   - Good: Rely on the global query filter. Query filters are automatically suppressed when `ITenantContext.IsPlatformScope` is true for platform administrators.
