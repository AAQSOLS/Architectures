# GitHub Copilot Instructions for ModularSaaS

Refer to [AGENTS.md](../AGENTS.md) and [docs/ARCHITECTURE.md](../docs/ARCHITECTURE.md) for complete architectural specifications.

## Architectural Boundaries
- Inward dependencies: Hosts -> Application -> Domain; Infrastructure -> Application -> Domain.
- Domain contains enterprise entities, value objects, domain enums, and invariants. It has zero external dependencies.
- Application contains use-case services, abstractions, input/result models, validators, and mappings. It never exposes Domain entities or value objects.
- `apps/Api` references only Application abstractions and models (and Domain enums). Controllers must never reference `DbContext`, repositories, or entities directly.
- Modules are vertical folders in each layer (`Identity`, `Tenancy`). Cross-module interaction happens strictly through Application abstractions, referencing aggregates by ID.

## Coding Conventions & Quality Gates
- .NET 10, C# 13, file-scoped namespaces matching directory paths.
- All classes, records, and services must be explicitly `sealed` (`internal sealed`, `public sealed`).
- Models (Input, Result, Request, Response) are `sealed record` with `required` and `init` properties.
- Do not use `DateTime.Now` or `DateTime.UtcNow`. Inject and use `IClock`.
- Multi-tenant entities implement `ITenantEntity` and are isolated via EF Core global query filters. Never invoke `IgnoreQueryFilters()` in tenant services.
- Never map input DTOs directly to domain entities using Mapster. Instantiate entities via constructors or factory methods.

## Verification
Validate changes with:
- `dotnet build ModularSaaS.slnx` (0 warnings, 0 errors)
- `dotnet test tests/Architecture.Tests/ModularSaaS.Architecture.Tests.csproj`
- `dotnet test ModularSaaS.slnx`
