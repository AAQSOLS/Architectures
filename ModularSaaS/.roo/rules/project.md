# Roo Code Project Rules

Read and adhere to [AGENTS.md](../../AGENTS.md) and [docs/ARCHITECTURE.md](../../docs/ARCHITECTURE.md).

## Non-Negotiable Architecture Invariants
- Dependencies point strictly inward: Hosts -> Application -> Domain; Infrastructure -> Application -> Domain.
- Hosts (`apps/Api`) reference only `Application/{Module}/Abstractions` and `Application/{Module}/Models` (and Domain enums). Never expose entities, `DbContext`, or repositories to hosts.
- Modules are vertical folders, not separate projects. Namespaces strictly match folder paths using file-scoped namespaces.
- Write operations go through aggregate roots via `IRepository<T>`. Read operations go through `IXxxReader` returning read models via EF projections or Dapper stored procedures.
- All DTO models, contracts, and internal classes must be `sealed` with explicit accessibility modifiers (`internal sealed`, `public sealed`).
- Banned APIs: Never call `DateTime.Now` or `DateTime.UtcNow`. Inject and use `IClock`. Never call `IgnoreQueryFilters()` in tenant code.
- Multi-tenancy: Entities with tenant affinity implement `ITenantEntity` and are isolated via EF Core global query filters.

## Verification Gate
Before finishing any step:
- `dotnet build ModularSaaS.slnx`
- `dotnet test tests/Architecture.Tests/ModularSaaS.Architecture.Tests.csproj`
- `dotnet test ModularSaaS.slnx`
