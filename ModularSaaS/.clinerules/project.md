# Cline Project Rules

Consult [AGENTS.md](../AGENTS.md) and [docs/ARCHITECTURE.md](../docs/ARCHITECTURE.md) for full architecture definitions and coding standards.

## Architecture Boundaries
- Inward dependency flow: `apps/Api` -> `src/Application` -> `src/Domain`; `src/Infrastructure` -> `src/Application` -> `src/Domain`.
- Domain has zero external dependencies.
- `apps/Api` controllers reference only Application abstractions and models. Never reference entities, repositories, or `DbContext`.
- Modules are vertical folders in each layer. Namespaces must match folder paths.
- Writes use aggregate roots and `IRepository<T>`. Reads use `IXxxReader` via EF projections or Dapper stored procedures.

## Coding Conventions
- .NET 10, C# 13, file-scoped namespaces, explicit accessibility.
- Types: `sealed record` with `required` and `init` for Input/Result/Request/Response models. `internal sealed` for services and implementations.
- Banned APIs: Do not use `DateTime.Now`, `DateTime.UtcNow` (use `IClock`), or `IgnoreQueryFilters()` in tenant scope.
- Multi-tenancy: Tenant-owned entities implement `ITenantEntity`.

## Verification Commands
- `dotnet build ModularSaaS.slnx`
- `dotnet test tests/Architecture.Tests/ModularSaaS.Architecture.Tests.csproj`
- `dotnet test ModularSaaS.slnx`
