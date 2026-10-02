# Gemini CLI Context & Rules

Follow the shared repository instructions in [AGENTS.md](AGENTS.md) and [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md).

## Project Overview
ModularSaaS is a .NET 10 Clean Architecture modular monolith backend featuring multi-tenancy with discriminator columns, normalized 7-table RBAC, OpenTelemetry observability, and automated Mono.Cecil architecture testing.

## Commands
- Build solution: `dotnet build ModularSaaS.slnx`
- Architecture tests: `dotnet test tests/Architecture.Tests/ModularSaaS.Architecture.Tests.csproj`
- Full test suite: `dotnet test ModularSaaS.slnx`
- Run API: `dotnet run --project apps/Api/ModularSaaS.Api.csproj`
- Pending migration check: `dotnet ef migrations has-pending-model-changes --project src/Infrastructure/ModularSaaS.Infrastructure.csproj --startup-project apps/Api/ModularSaaS.Api.csproj`
- Code formatting check: `dotnet format --verify-no-changes`

## Non-Negotiable Invariants
- Dependencies flow strictly inward: Hosts -> Application -> Domain; Infrastructure -> Application -> Domain.
- Hosts (`apps/Api`) reference only `Application/{Module}/Abstractions`, `Application/{Module}/Models`, and Domain enums.
- Modules are folders inside each layer. Namespaces match folder paths. File-scoped namespaces required.
- Write side: Aggregates + `IRepository<T>`. Read side: `IXxxReader` returning read models via EF projections or Dapper stored procedures.
- Types: `sealed record` with `required` and `init` for Input/Result/Request/Response models. `internal sealed` for services and implementations.
- Forbidden: `DateTime.Now`, `DateTime.UtcNow` (use `IClock`), and `IgnoreQueryFilters()` in tenant scope.
