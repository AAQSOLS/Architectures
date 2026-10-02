# Windsurf Project Rules

All architectural boundaries, conventions, and verification gates are centrally defined in [AGENTS.md](../../AGENTS.md) and [docs/ARCHITECTURE.md](../../docs/ARCHITECTURE.md).

## Critical Guidelines
1. Inward dependencies: Hosts -> Application -> Domain; Infrastructure -> Application -> Domain. Domain has zero external dependencies.
2. Host boundaries: `apps/Api` references only Application abstractions and models. Domain enums are the only domain types allowed in host contracts. No controllers may reference `DbContext` or repositories.
3. Modular layout: Modules are folders inside each layer. Namespaces match folder paths. Use file-scoped namespaces.
4. Sealed types: All models, contracts, and internal classes must be `sealed` (`internal sealed`, `public sealed`).
5. Time abstraction: Never use `DateTime.Now` or `DateTime.UtcNow`. Inject and use `IClock`.
6. Multi-tenancy: Tenant entities implement `ITenantEntity`. Global query filters isolate tenant data. Never call `IgnoreQueryFilters()` in tenant services.
7. Verification: Run `dotnet build ModularSaaS.slnx` and `dotnet test tests/Architecture.Tests/ModularSaaS.Architecture.Tests.csproj` before concluding.
