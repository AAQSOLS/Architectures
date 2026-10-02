# Claude Code Instructions

Always read and follow [AGENTS.md](AGENTS.md) and [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) before making changes.

## Quick Commands
- Build solution: `dotnet build ModularSaaS.slnx`
- Run architecture tests: `dotnet test tests/Architecture.Tests/ModularSaaS.Architecture.Tests.csproj`
- Run all tests: `dotnet test ModularSaaS.slnx`
- Run API host: `dotnet run --project apps/Api/ModularSaaS.Api.csproj`
- Check EF model changes: `dotnet ef migrations has-pending-model-changes --project src/Infrastructure/ModularSaaS.Infrastructure.csproj --startup-project apps/Api/ModularSaaS.Api.csproj`
- Verify formatting: `dotnet format --verify-no-changes`

## Non-Negotiable Rules
1. Inward dependencies: Hosts -> Application -> Domain; Infrastructure -> Application -> Domain. Domain references nothing.
2. Hosts (apps/Api) reference only `Application/{Module}/Abstractions`, `Application/{Module}/Models`, and Domain enums. Never reference entities, `DbContext`, or repositories.
3. Modules are folders inside each layer, not separate projects. Namespaces must strictly match folder paths using file-scoped namespaces.
4. All DTO models, contracts, and internal classes must be `sealed` with explicit accessibility modifiers (`internal sealed`, `public sealed`).
5. Never use `DateTime.Now`, `DateTime.UtcNow`, or `DateTimeOffset.UtcNow`. Always inject `IClock`.
6. Multi-tenant entities implement `ITenantEntity`. Never call `IgnoreQueryFilters()` in tenant code.
7. Verification gate: `dotnet build ModularSaaS.slnx` and `dotnet test tests/Architecture.Tests/ModularSaaS.Architecture.Tests.csproj` must pass after every change.
