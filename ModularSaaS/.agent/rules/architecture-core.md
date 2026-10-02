---
trigger: always_on
description: Non-negotiable architecture rules for this repository
---

# Core architecture rules (full detail: docs/ARCHITECTURE.md)

1. Dependencies point inward: Hosts -> Application -> Domain; Infrastructure -> Application -> Domain. Domain references nothing.
2. `Client.Api` and `Admin.Mvc` never reference each other. They share one Application, Domain, Infrastructure.
3. Hosts use ONLY `Application/{Module}/Abstractions` and `Application/{Module}/Models`. Never entities, repositories, `DbContext`, or Infrastructure types. Only exception: Domain enums (namespace ending `.Enums`).
4. Modules are FOLDERS inside each layer, not projects. Namespace = folder path. File-scoped namespaces.
5. Modules call each other only through `Abstractions` and reference each other's aggregates by ID (no cross-module navigation properties).
6. Writes: aggregates + `IRepository<T>` (one repository per aggregate root; no `IQueryable`, no `SaveChanges`). Reads (grids, lists, reports, lookups): `IXxxReader` implemented with stored procedures via Dapper or EF projections. There is no "Query" concept and no CQRS.
7. Multi-tenant: tenant entities implement `ITenantEntity` (global filter). Every SP outside `database/StoredProcedures/Platform/` declares and filters `@TenantId`. Tenant-scoped code throws when no tenant is resolved. `IgnoreQueryFilters()` and platform SPs only in `Persistence/Readers/Platform/`.
8. Application public surface (Abstractions, Models) never exposes entities or value objects. Use primitives (`decimal Amount`, `string Currency`).
9. Types: `sealed record` with `required` + `init` for Input/Result/ListItem/Filter/Request/Response and display view models; `sealed class` for MVC `XxxFormModel`. No DTO inheritance. `internal sealed` for service implementations, validators, mappings.
10. Files: Domain = one type per file, enums one per file inside `Enums/`. Application Models/Validators, API Contracts, MVC ViewModels = one file per USE CASE / SCREEN (`PlaceOrder.cs` holds `PlaceOrderInput`, its child inputs, `PlaceOrderResult`).
11. Mapping with Mapster (strict). Entity -> Result in Application `Mapping/`; Contract/ViewModel <-> Input/Result in host `Mapping/`. Never map Input -> entity with a mapper; use constructors/factory/behavior methods.
12. No MediatR, no message bus, no domain events. Cross-module workflows are explicit named services inside one `IUnitOfWork` transaction.
13. Use `IClock`, never `DateTime.Now/UtcNow`. Expected failures return `Result<T>`; invariant violations throw `DomainException`.
14. Do not invent structure. If a rule is missing or ambiguous, stop and ask.

# Definition of done for any change
`dotnet build` (0 errors, and no analyzer warnings in the paths you touched), `dotnet test tests/Architecture.Tests`, and `dotnet test` all pass. See AGENTS.md for exact commands.
