# Architecture Rules: Clean Architecture Modular Monolith

> **Audience:** developers and AI agents working in this repository.
> **Status:** rule of thumb. Follow it by default. Deviations need an ADR in `docs/adr/`.
> Keywords **MUST**, **MUST NOT**, **SHOULD**, **MAY** are used in the RFC 2119 sense.
> Replace `ModularSaaS` with the real root namespace.

---

## 0. Golden rules (read this first)

1. Dependencies point inward: `Hosts → Application → Domain`. `Infrastructure → Application → Domain`. Domain depends on nothing.
2. Two hosts (`Client.Api`, `Admin.Mvc`) share one Application, one Domain, one Infrastructure. Hosts MUST NOT reference each other.
3. Hosts talk only to `Application/{Module}/Abstractions` and `Application/{Module}/Models`. They never see entities, repositories, `DbContext`, or Infrastructure types. The single exception is Domain **enums**.
4. Every module is a folder inside each layer, not a project. Module boundaries are enforced by architecture tests.
5. Modules call each other only through `Abstractions` interfaces and reference each other's aggregates **by ID**.
6. Writes go through aggregates and repositories. Reads (grids, lists, reports, lookups) go through **Readers** (`IXxxReader`), implemented with stored procedures (Dapper) or EF projections.
7. The app is multi-tenant. Every tenant-scoped SP takes `@TenantId`. Every tenant-scoped entity implements `ITenantEntity`. Tenant-scoped code fails closed when no tenant is resolved.
8. Data shapes are `sealed record`s (mutable form models are `sealed class`es). Files are **per use case**, not per type. Domain is one type per file.
9. No CQRS library, no mediator, no message bus, no domain events. Cross-module workflows are orchestrated explicitly in named services.
10. Do not invent new top-level folders, projects, or patterns. Extend the structure below.

---

## 1. Context

| Item | Decision |
|---|---|
| Product | SaaS: online store (Angular SSR) + admin portal (MVC) for physical stores, inventory, warehouses, POS, walk-in customers, store management |
| Tenancy | Multi-tenant SaaS. Platform admin (no tenant), tenant admin, tenant users |
| Hosts | `Client.Api` (website, mobile, external clients), `Admin.Mvc` (platform + tenant back office) |
| Database | SQL Server, one database, one `DbContext`, one schema per module |
| Data access | EF Core (writes, simple reads) + Dapper calling stored procedures (complex reads, reports) |
| Schema management | EF Core migrations for tables; SPs as re-runnable `CREATE OR ALTER` scripts |
| Mapping | Mapster (strict) |
| Validation | FluentValidation |
| Documents | PDF (QuestPDF), Excel (ClosedXML or EPPlus) behind Application abstractions |
| Email | Per-tenant SMTP settings configured in the admin portal |
| Third-party integrations | None now. Added later as `Infrastructure/ExternalServices/{Provider}/` behind Application abstractions |
| Not used | CQRS/MediatR, message bus, domain events, outbox |

---

## 2. Architectural style

This is a **layered monolith with modular folders and enforced boundaries** ("modular-ready"). It is deliberately **not** a full modular monolith (no project per module, no DbContext per module) because POS, stock, and orders need single-transaction consistency and there is no team or deployment pressure to isolate modules.

Modular-ready means:

- schema per module,
- cross-module references by ID,
- cross-module calls only via `Abstractions`,
- module-boundary architecture tests,
- one `AddXxxModule()` registration per module.

If a module ever needs extraction (Reporting and Notifications are the likeliest), these conventions make it mechanical. Revisit this decision only if multiple teams own modules or a module needs independent scaling or deployment.

---

## 3. Solution layout

```
ModularSaaS/
├── ModularSaaS.sln
├── Directory.Build.props              # Nullable, analyzers, TreatWarningsAsErrors
├── Directory.Packages.props           # central package versions
├── .editorconfig                      # analyzer relaxations (see §8.6)
├── README.md
│
├── docs/
│   ├── ARCHITECTURE.md                # this file
│   └── adr/                           # architecture decision records
│
├── database/                          # SQL assets kept under source control
│   ├── StoredProcedures/
│   │   ├── Platform/                  # usp_Platform_*  (no @TenantId)
│   │   ├── Identity/                  # usp_Identity_*
│   │   ├── Catalog/
│   │   ├── Inventory/
│   │   ├── Sales/
│   │   ├── Reporting/
│   │   └── ...                        # one folder per module
│   └── Scripts/                       # seed data, one-off scripts
│
├── web/
│   └── storefront/                    # Angular SSR. Consumes Client.Api only
│
├── apps/                              # runnable hosts (composition roots)
│   ├── Client.Api/
│   │   ├── Controllers/
│   │   │   ├── Catalog/               # ProductsController, CategoriesController
│   │   │   ├── Cart/
│   │   │   ├── Orders/
│   │   │   └── Account/
│   │   ├── Contracts/                 # JSON request/response models, per module
│   │   │   ├── Catalog/
│   │   │   │   ├── ListProducts.cs
│   │   │   │   ├── GetProduct.cs
│   │   │   │   └── Common/
│   │   │   ├── Orders/
│   │   │   └── Common/                # shared across modules within this host
│   │   ├── Mapping/                   # {Module}Mappings.cs : IRegister (Contract <-> Application model)
│   │   ├── Middleware/                # TenantResolution, ExceptionHandling
│   │   ├── Security/                  # JWT setup, policies
│   │   └── Program.cs
│   │
│   └── Admin.Mvc/
│       ├── Areas/
│       │   ├── Platform/              # platform-only screens: tenants, plans, global settings, platform users
│       │   │   ├── Controllers/
│       │   │   ├── Views/
│       │   │   └── ViewModels/
│       │   ├── Identity/              # users, roles, permissions (platform AND tenant admins)
│       │   ├── Catalog/
│       │   ├── Inventory/
│       │   ├── Warehouses/
│       │   ├── Stores/
│       │   ├── Pos/
│       │   ├── Sales/
│       │   ├── Customers/
│       │   ├── Purchasing/
│       │   ├── Reports/
│       │   └── Settings/              # email config, store config
│       │       (every area: Controllers/  Views/  ViewModels/)
│       ├── Views/Shared/              # layouts, partials, view components
│       ├── Mapping/                   # {Module}Mappings.cs : IRegister (ViewModel/FormModel <-> Application model)
│       ├── Security/                  # cookie auth, permission-based authorization
│       ├── wwwroot/
│       └── Program.cs
│
├── src/
│   ├── Domain/                        # ModularSaaS.Domain. References nothing
│   │   ├── Shared/                    # BaseEntity, AuditableEntity, IAggregateRoot, ITenantEntity,
│   │   │                              # ValueObject, Money, Address, DomainException
│   │   ├── Platform/                  # PlatformUser (not tenant-scoped)
│   │   ├── Tenancy/                   # Tenant, TenantPlan, TenantEmailSettings
│   │   │   └── Enums/
│   │   ├── Identity/                  # User, Role, Permission (tenant-scoped)
│   │   │   └── Enums/
│   │   ├── Catalog/                   # Product, Variant, Category, Brand, PriceList, Tag
│   │   │   └── Enums/
│   │   ├── Inventory/
│   │   │   ├── StockItem.cs           # aggregate root
│   │   │   ├── StockMovement.cs
│   │   │   ├── StockAdjustment.cs
│   │   │   ├── InventoryException.cs
│   │   │   ├── Enums/
│   │   │   │   ├── StockMovementType.cs
│   │   │   │   └── AdjustmentReason.cs
│   │   │   └── ValueObjects/
│   │   ├── Warehouses/                # Warehouse, Location, StockTransfer
│   │   ├── Stores/                    # Store (physical/online), Register, Shift
│   │   ├── Sales/
│   │   │   ├── Orders/                # subfolder per aggregate once a module is big
│   │   │   │   ├── Order.cs           # one Order model for online + POS + walk-in
│   │   │   │   ├── OrderLine.cs
│   │   │   │   └── Enums/
│   │   │   ├── Returns/
│   │   │   ├── Payments/
│   │   │   └── Services/              # domain services (pricing, tax calculation)
│   │   ├── Customers/
│   │   ├── Purchasing/                # Supplier, PurchaseOrder, GoodsReceipt
│   │   └── Settings/
│   │
│   ├── Application/                   # ModularSaaS.Application. References Domain
│   │   ├── Shared/
│   │   │   ├── Abstractions/          # IRepository<T>, IUnitOfWork, ITenantContext, ICurrentUser,
│   │   │   │                          # IPermissionChecker, IClock, IEmailSender, IPdfGenerator,
│   │   │   │                          # IExcelExporter, IFileStorage
│   │   │   ├── Models/                # PagedResult<T>, PageRequest, Result<T>, Error, LookupItem<TId>, AuditInfo
│   │   │   └── Exceptions/
│   │   ├── Platform/                  # tenant management, plans, platform users (platform scope only)
│   │   ├── Tenancy/
│   │   ├── Identity/
│   │   │   └── Permissions/           # permission-name constants
│   │   ├── Catalog/
│   │   ├── Inventory/
│   │   │   ├── Abstractions/          # IInventoryService, IStockRepository, IInventoryReader
│   │   │   ├── Services/              # internal sealed: StockAdjustmentService, StockTransferService
│   │   │   ├── Models/                # one file per use case
│   │   │   │   ├── AdjustStock.cs
│   │   │   │   ├── GetStockLevels.cs
│   │   │   │   └── Common/
│   │   │   ├── Validators/            # one file per use case (mirrors Models/)
│   │   │   ├── Mapping/               # InventoryMappings.cs : IRegister (Entity -> Result)
│   │   │   └── InventoryModule.cs     # AddInventoryModule(IServiceCollection)
│   │   ├── Sales/
│   │   │   ├── Abstractions/          # IOrderService, IOrderRepository, ISalesReader
│   │   │   ├── Services/
│   │   │   │   ├── OrderPlacementService.cs
│   │   │   │   ├── PosCheckoutService.cs
│   │   │   │   ├── OrderFulfillmentService.cs
│   │   │   │   └── ReturnService.cs
│   │   │   ├── Models/
│   │   │   ├── Validators/
│   │   │   ├── Mapping/
│   │   │   └── SalesModule.cs
│   │   ├── Warehouses/  Stores/  Customers/  Purchasing/
│   │   ├── Reporting/                 # report definitions, IReportReader, exporter inputs
│   │   ├── Notifications/             # email use cases + template models
│   │   └── DependencyInjection.cs     # AddApplication() calls every AddXxxModule()
│   │
│   └── Infrastructure/                # ModularSaaS.Infrastructure. References Application + Domain
│       ├── Persistence/
│       │   ├── AppDbContext.cs
│       │   ├── Configurations/        # IEntityTypeConfiguration<T>, one file per entity
│       │   │   ├── Inventory/  Sales/  Catalog/  ...
│       │   ├── Migrations/
│       │   ├── Interceptors/          # audit fields, tenant stamping
│       │   ├── Tenancy/               # TenantContext impl, global query filter setup
│       │   ├── Repositories/          # write-side, one per aggregate root
│       │   │   ├── EfRepository.cs    # generic base
│       │   │   ├── Inventory/  Sales/  ...
│       │   ├── Readers/               # read-side: IXxxReader implementations (SP via Dapper, or EF projection)
│       │   │   ├── Platform/          # the ONLY place platform SPs and IgnoreQueryFilters() are allowed
│       │   │   ├── Inventory/  Sales/  Reporting/  ...
│       │   ├── StoredProcedures/      # SP name constants, Dapper helpers, SQL script runner
│       │   │   ├── SpNames/           # InventorySps.cs, SalesSps.cs, PlatformSps.cs ...
│       │   │   ├── SqlScriptRunner.cs
│       │   │   └── DapperExecutor.cs
│       │   └── UnitOfWork.cs
│       ├── Documents/
│       │   ├── Pdf/                   # invoice, receipt, PO templates
│       │   └── Excel/
│       ├── Email/                     # TenantEmailSender, template renderer, credential encryption
│       ├── Security/                  # password hasher, token service, data protection
│       ├── Storage/                   # file/image storage
│       ├── ExternalServices/          # empty for now; one folder per provider later
│       └── DependencyInjection.cs     # AddInfrastructure(config)
│
└── tests/
    ├── Domain.UnitTests/
    ├── Application.UnitTests/
    ├── Infrastructure.IntegrationTests/   # real SQL Server (Testcontainers), SP tests
    ├── Architecture.Tests/                # layer + module + host boundary rules
    └── Client.Api.IntegrationTests/
```

---

## 4. Layers and dependency rules

### 4.1 Responsibilities

| Layer | Owns | MUST NOT |
|---|---|---|
| **Domain** | Entities, aggregates, value objects, enums, domain services, domain exceptions, invariants | reference anything; know about EF, HTTP, JSON, DTOs, tenants-as-context |
| **Application** | Use cases (services), abstractions, input/result models, validators, entity→result mapping, permission checks, transaction orchestration | reference Infrastructure or hosts; know about HTTP, HTML, or SQL |
| **Infrastructure** | EF Core, repositories, readers, SPs, email, PDF/Excel, storage, security, external providers | contain business rules; be referenced by hosts except in `Program.cs` |
| **Client.Api / Admin.Mvc** | HTTP/HTML concerns, contracts/view models, host mapping, auth setup, tenant resolution | reference each other; use `DbContext`, entities, repositories, or Infrastructure types |

### 4.2 Reference graph

```
Client.Api ─┐
            ├──> Application ──> Domain
Admin.Mvc ──┘         ▲
   │                  │
   └──> Infrastructure┘   (hosts reference Infrastructure ONLY in Program.cs to call AddInfrastructure())
        Infrastructure ──> Application ──> Domain
```

### 4.3 Does Application reference entities?

**Yes, internally.** Application loads aggregates, calls their behavior, and saves them via repositories.

**Never on its public surface.** Service interfaces, `Input`, `Result`, `ListItem`, and `Filter` types MUST NOT contain entities or value objects. Models use primitives: `decimal Amount` + `string Currency`, not `Money`.

**Enums are the one allowed crossing.** Hosts and Application models MAY use `ModularSaaS.Domain.*.Enums` types. Everything else in `ModularSaaS.Domain` is off limits to hosts. Serialize enums as strings in the API.

C# project references are transitive, so hosts can technically see Domain. That is why the rule is enforced by architecture tests (§14), not by convention alone.

### 4.4 Visibility

- Application service **implementations**, validators, and mapping registrations are `internal sealed`. Hosts get them through DI as interfaces.
- `Abstractions` and `Models` are `public`. They are the module's public surface.
- Test projects use `InternalsVisibleTo`.

### 4.5 Namespaces

**Namespace = folder path.** No exceptions, no shortcuts.

```
src/Domain/Inventory/Enums/StockMovementType.cs      → ModularSaaS.Domain.Inventory.Enums
src/Application/Sales/Models/PlaceOrder.cs           → ModularSaaS.Application.Sales.Models
apps/Client.Api/Contracts/Orders/PlaceOrder.cs       → ModularSaaS.Client.Api.Contracts.Orders
```

This makes every architecture rule a simple namespace prefix match.

---

## 5. Modules

### 5.1 What a module is

A business area that appears as a same-named folder in Domain, Application, Infrastructure (Persistence subfolders), the host areas/controllers/contracts, and as a SQL schema and SP folder. Modules: `Platform`, `Tenancy`, `Identity`, `Catalog`, `Inventory`, `Warehouses`, `Stores`, `Pos`, `Sales`, `Customers`, `Purchasing`, `Reporting`, `Notifications`, `Settings`.

> `Pos` is a host/UI area. In Domain and Application, POS logic lives in `Sales` (`PosCheckoutService`) and `Stores` (registers, shifts). POS uses the same `Order` and stock model as online sales, distinguished by `Channel` and `StoreId`.

### 5.2 Module rules

1. **Cross-module calls go through `Abstractions` only.** `Sales` may call `IInventoryService.Reserve(...)`. It MUST NOT use Inventory's repositories, entities, validators, or services directly.
2. **Cross-module entity references are by ID** (`Guid ProductId`), never navigation properties.
3. **A module MAY use another module's `Models`** (its published surface). It MUST NOT use its internals.
4. **Cross-module workflows are orchestrated explicitly** in one clearly named service inside the initiating module (e.g. `OrderPlacementService` calls Inventory and Payments inside one `IUnitOfWork` transaction). Do not hide coupling behind reflection or hooks.
5. **Each module registers itself** with `AddXxxModule(IServiceCollection)`, called from `AddApplication()`. Infrastructure registers repositories and readers explicitly per module.
6. **Each module owns a SQL schema** (`sales.Orders`, `inventory.StockItems`).
7. **Split god services.** No service with dozens of methods. Split by use case or sub-area (`OrderPlacementService`, `OrderFulfillmentService`, `OrderReturnService`).
8. **Sub-areas:** once a module folder has about 15+ files in one folder, add sub-area folders (`Sales/Orders`, `Sales/Returns`), consistently across layers.

---

## 6. Multi-tenancy and platform scope

### 6.1 Scopes

| Scope | Rule | Reachable from |
|---|---|---|
| **Tenant-scoped** (default) | `@TenantId` on every SP; `ITenantEntity` with EF global query filter | any tenant-facing service |
| **Platform-scoped** | no tenant parameter; entities are not `ITenantEntity` | only `Application/Platform` services, gated by `Platform.*` permissions |

### 6.2 `ITenantContext`

```csharp
public interface ITenantContext
{
    Guid? TenantId { get; }
    bool IsPlatformScope { get; }
    Guid RequireTenantId(); // throws if TenantId is null (fails closed)
}
```

- **Tenant-scoped code fails closed.** If `TenantId` is null and the scope is not platform, throw. Never return all rows.
- **Client.Api** resolves the tenant by domain/subdomain, `X-Tenant` header, or API key, via `TenantResolution` middleware.
- **Admin.Mvc** resolves the tenant from the user's claim. Platform admins have no tenant.

### 6.3 Platform admin acting inside a tenant

Platform admin **impersonates** a tenant explicitly: `ITenantContext.TenantId` is set for that session and the action is **audit-logged**. From then on it is ordinary tenant-scoped code. No service has a "platform mode" branch.

### 6.4 Rules

1. Every tenant-owned entity implements `ITenantEntity` and gets a global query filter on `TenantId`. `TenantId` is stamped by a `SaveChanges` interceptor.
2. `Tenant`, `TenantPlan`, and `PlatformUser` are **not** `ITenantEntity`.
3. `PlatformUser` and tenant `User` are **separate entities and tables**. Never one `User` table with nullable `TenantId`.
4. `IgnoreQueryFilters()` and platform SPs are allowed **only** in `Infrastructure/Persistence/Readers/Platform/`.
5. Global query filters do not protect SPs. Every SP outside `database/StoredProcedures/Platform/` MUST declare `@TenantId` and MUST filter by it.
6. Tenant email settings: `TenantEmailSettings` in `Domain/Tenancy`; SMTP secrets encrypted with ASP.NET Data Protection; `Infrastructure/Email/TenantEmailSender` implements `IEmailSender` and loads the current tenant's settings. Application code only calls `IEmailSender`.
7. Tenant configuration (feature flags, plan limits, store settings) lives in `Tenancy`/`Settings` and is cached per tenant.
8. Authorization is separate from scope: a platform admin and a tenant admin both use the `Identity` area; the resolved tenant and permissions differ.
9. If platform admin needs a distinct security posture later, peel `Areas/Platform` into a third host (`Platform.Mvc`). Keeping it a separate area now makes that mechanical.

---

## 7. Data access

### 7.1 Write side: aggregates and repositories

- `IRepository<T> where T : IAggregateRoot` in `Application/Shared/Abstractions`: `GetByIdAsync`, `AddAsync`, `Remove`. Implemented as `EfRepository<T>` in `Infrastructure/Persistence/Repositories/`.
- Module repositories (`IStockRepository`) extend it only when extra methods are needed. One repository per **aggregate root**.
- Repositories MUST NOT expose `IQueryable`, MUST NOT call `SaveChanges` (`IUnitOfWork` does), and MUST NOT be used for grid/list/report reads.

### 7.2 Read side: Readers

There is no "Queries" concept and no CQRS. Reads use **Readers**:

- Interface: `IXxxReader` in `Application/{Module}/Abstractions/` (e.g. `IInventoryReader`, `ISalesReader`, `IReportReader`).
- Implementation: `Infrastructure/Persistence/Readers/{Module}/XxxReader.cs`.
- A reader method returns Application `Result` / `ListItem` types directly. It MUST NOT return entities.
- A reader is implemented with either:
  - a **stored procedure via Dapper** (complex lists, joins, aggregates, reports), or
  - an **EF projection** (`Select` into a result type, `AsNoTracking`) for simple lookups.
- Platform readers live in `Readers/Platform/` and are the only place allowed to bypass tenant filters.

```csharp
internal sealed class InventoryReader(AppDbContext db, ITenantContext tenant) : IInventoryReader
{
    public async Task<PagedResult<StockLevelListItem>> GetStockLevelsAsync(
        StockLevelFilter filter, CancellationToken ct)
    {
        var conn = db.Database.GetDbConnection();
        var tx   = db.Database.CurrentTransaction?.GetDbTransaction();

        var rows = await conn.QueryAsync<StockLevelListItem>(new CommandDefinition(
            InventorySps.GetStockLevels,
            new { TenantId = tenant.RequireTenantId(), filter.WarehouseId, filter.Page, filter.PageSize },
            transaction: tx,
            commandType: CommandType.StoredProcedure,
            cancellationToken: ct));

        // ...
    }
}
```

### 7.3 Stored procedures

- **Location:** `database/StoredProcedures/{Module}/usp_{Module}_{Action}.sql` (`usp_Inventory_GetStockLevels`). Platform SPs: `database/StoredProcedures/Platform/usp_Platform_*.sql`.
- **Form:** re-runnable `CREATE OR ALTER PROCEDURE`. Never paste SP bodies into EF migration files (you lose diffs and history).
- **Deployment:** SP scripts are embedded resources applied by `SqlScriptRunner` **after** EF migrations run. (DbUp is an acceptable alternative.) Migrations and SP scripts are both applied from the same deployment step.
- **Names:** SP names live as constants in `Infrastructure/Persistence/StoredProcedures/SpNames/{Module}Sps.cs`. No string literals in readers.
- **Schema:** SP lives in the module's schema.
- **Tenant rule:** tenant-scoped SPs declare `@TenantId` as a required parameter and filter every tenant table by it.
- **Result shapes:** Dapper materializes straight into Application result types. Column names match property names.
- **Transactions:** Dapper uses the EF connection and current transaction (`db.Database.GetDbConnection()`, `CurrentTransaction`) so `IUnitOfWork` covers both.
- **Parameters:** always parameterized. No string concatenation into SQL.
- **Tests:** every SP has an integration test against real SQL Server (Testcontainers). Cross-tenant leak tests are mandatory for tenant-scoped SPs.

### 7.4 Unit of work and transactions

- `IUnitOfWork.SaveChangesAsync` commits. `IUnitOfWork.ExecuteInTransactionAsync` wraps multi-step workflows.
- One `DbContext` for all modules. Entity configurations are split per module in `Configurations/{Module}/`. Split the context only if build or model-build time hurts.
- Global query filters on `ITenantEntity`. Interceptors stamp `TenantId`, `CreatedAt/By`, `ModifiedAt/By`.

---

## 8. Types and file organization

### 8.1 Type conventions

| Where | Kind | Naming | C# type | File layout |
|---|---|---|---|---|
| Domain | Entity / aggregate | plain (`Order`, `StockItem`) | `class`, private setters, behavior methods | **One type per file**, in `Domain/{Module}[/{Aggregate}]/` |
| Domain | Value object | `Money`, `Address` | `record` or `readonly record struct` | **One type per file**, `ValueObjects/` |
| Domain | Enum | `StockMovementType` | `enum` | **One enum per file**, always in `Enums/` |
| Domain | Exception | `InventoryException` | `class` | One per module, `Domain/{Module}/` |
| Domain | Domain service | `PricingService` | `class` | One per file, `Domain/{Module}/Services/` |
| Application | Use-case input | `XxxInput` | `sealed record`, `required` + `init` | **One file per use case**: `Models/{UseCase}.cs` holds `XxxInput`, child inputs, and `XxxResult` |
| Application | Use-case result | `XxxResult` | `sealed record` | same use-case file |
| Application | List row | `XxxListItem` | `sealed record` | same use-case file as the list use case |
| Application | Filter/criteria | `XxxFilter` | `sealed record` | same use-case file. Never call it `Query` |
| Application | Service interface | `IXxxService` | `interface` | `Abstractions/`, one interface per file |
| Application | Repository interface | `IXxxRepository` | `interface` | `Abstractions/`, one per aggregate |
| Application | Reader interface | `IXxxReader` | `interface` | `Abstractions/`, one per module (or per sub-area) |
| Application | Service impl | `XxxService` | `internal sealed class` | `Services/`, **one class per file** |
| Application | Validator | `XxxInputValidator` | `internal sealed class : AbstractValidator<T>` | `Validators/{UseCase}.cs`, mirrors `Models/`, all validators for that use case in one file |
| Application | Mapping | `{Module}Mappings` | `internal sealed class : IRegister` | `Mapping/{Module}Mappings.cs`, one per module |
| Application | Module registration | `{Module}Module` | `static class` | `{Module}Module.cs` at module root |
| Application | Permission constants | `Permissions` | `static class` with nested classes | `Application/Identity/Permissions/` |
| Application (shared) | Cross-module models | `PagedResult<T>`, `LookupItem<TId>` | `sealed record` | `Shared/Models/`, one type per file (these are true primitives) |
| Client.Api | Request | `XxxRequest` | `sealed record`, `required` + `init` | **One file per endpoint/use case**: `Contracts/{Module}/{UseCase}.cs` holds `XxxRequest`, `XxxResponse`, and their child types |
| Client.Api | Response | `XxxResponse` | `sealed record` | same file |
| Client.Api | Controller | `XxxController` | `sealed class` | One per resource: `Controllers/{Module}/XxxController.cs` |
| Client.Api | Mapping | `{Module}Mappings` | `sealed class : IRegister` | `Mapping/{Module}Mappings.cs` |
| Admin.Mvc | Form model | `XxxFormModel` | `sealed class`, mutable | **One file per screen/form**: `Areas/{Area}/ViewModels/{Screen}.cs` |
| Admin.Mvc | Display view model | `XxxListViewModel`, `XxxDetailsViewModel`, `XxxRowViewModel` | `sealed record` | same screen file as the screen that renders it |
| Admin.Mvc | Controller | `XxxController` | `sealed class` | One per resource: `Areas/{Area}/Controllers/` |
| Admin.Mvc | Mapping | `{Module}Mappings` | `sealed class : IRegister` | `Mapping/{Module}Mappings.cs` |
| Infrastructure | EF configuration | `XxxConfiguration` | `internal sealed class : IEntityTypeConfiguration<T>` | **One file per entity**: `Configurations/{Module}/` |
| Infrastructure | Repository impl | `XxxRepository` | `internal sealed class` | One per aggregate: `Repositories/{Module}/` |
| Infrastructure | Reader impl | `XxxReader` | `internal sealed class` | One per reader interface: `Readers/{Module}/` |
| Infrastructure | SP name constants | `{Module}Sps` | `static class` with `const string` | One per module: `StoredProcedures/SpNames/` |

### 8.2 Use-case file example (Application)

```csharp
// src/Application/Sales/Models/PlaceOrder.cs
namespace ModularSaaS.Application.Sales.Models;

public sealed record PlaceOrderInput
{
    public required Guid StoreId { get; init; }
    public required OrderChannel Channel { get; init; }          // Domain enum: allowed
    public required IReadOnlyList<PlaceOrderLineInput> Lines { get; init; }
    public Guid? CustomerId { get; init; }
}

public sealed record PlaceOrderLineInput
{
    public required Guid VariantId { get; init; }
    public required int Quantity { get; init; }
}

public sealed record PlaceOrderResult
{
    public required Guid OrderId { get; init; }
    public required string OrderNumber { get; init; }
    public required decimal Total { get; init; }
    public required string Currency { get; init; }
}
```

```csharp
// src/Application/Sales/Validators/PlaceOrder.cs
namespace ModularSaaS.Application.Sales.Validators;

internal sealed class PlaceOrderInputValidator : AbstractValidator<PlaceOrderInput> { /* ... */ }
internal sealed class PlaceOrderLineInputValidator : AbstractValidator<PlaceOrderLineInput> { /* ... */ }
```

### 8.3 Why records, classes, or structs

- **`sealed record` for data that flows** (inputs, results, list items, filters, requests, responses, display view models): immutable, value equality, `with` support.
- **`sealed class` for MVC form models:** form posts and redisplay after validation errors round-trip more reliably through mutable classes.
- **Use `required` + `init` properties.** Avoid positional records for anything over about four properties (brittle when fields are added).
- **`class` for entities** (identity semantics, private setters); **record / `readonly record struct` for value objects**.
- **`sealed` everywhere** unless inheritance is deliberate.
- **No DTO inheritance hierarchies.** Compose instead.
- **Collections:** `IReadOnlyList<T>` for outputs and inputs; never expose mutable collections from entities (use `IReadOnlyCollection<T>` over a private list).

### 8.4 Promotion ladder for shared types

1. Used by **one use case** → same use-case file.
2. Used by **2+ use cases in one module** → `{Module}/Models/Common/` (or `Contracts/{Module}/Common/`, `ViewModels/Common/` in hosts).
3. Used by **2+ modules** → `Application/Shared/Models/` (`PagedResult<T>`, `PageRequest`, `LookupItem<TId>`, `AuditInfo`).
4. Host-only shared types → `Contracts/Common/` or `Views/Shared` + `ViewModels/Common/` in that host only.

### 8.5 Reference data (categories, tags, permissions, lookups)

- **The module that owns the concept publishes the model.** Catalog owns `CategoryItem` and `TagItem`; Identity owns `PermissionItem`; Stores owns `StoreLookup` if needed.
- Other modules MAY use it because `Models` is the public surface.
- **Dropdowns** use `LookupItem<TId>` from `Shared/Models`.
- **Do not share types because shapes look similar today.** Share only stable concepts. When in doubt, duplicate; unwinding a wrong share costs more.

### 8.6 `.editorconfig` relaxations

- Relax the "one type per file" and "file name must match type" analyzers (for example StyleCop SA1402/SA1649) for `**/Models/**`, `**/Validators/**`, `**/Contracts/**`, `**/ViewModels/**`.
- **Keep** folder-namespace matching enabled everywhere. It backs the architecture-test rules.
- Keep one-type-per-file enforced for `Domain/**`.

### 8.7 File-size guardrail

If a use-case file exceeds roughly 200 lines, or a `Models/` folder exceeds roughly 15 files, split into sub-area folders (§5.2 rule 8) or, for the file, extract the child types into `Common/`.

---

## 9. Mapping (Mapster)

### 9.1 Two-hop mapping

```
Entity ──(Application/{Module}/Mapping)──> Result ──(host Mapping)──> Contract / ViewModel
Contract / FormModel ──(host Mapping)──> Input ──> Application service ──> entity (via factory / behavior methods)
```

- **Application** maps entity → result/list item (it already knows the entities).
- **Hosts** map only between their own types and Application `Input`/`Result` types. Hosts never see entities.
- **Input → entity is NOT mapped with Mapster.** Entities are built and changed through constructors, factory methods, and behavior methods so invariants are enforced.

### 9.2 Configuration

```csharp
TypeAdapterConfig.GlobalSettings.RequireExplicitMapping = true;
TypeAdapterConfig.GlobalSettings.RequireDestinationMemberSource = true;
```

- Register mappings per module through `IRegister` classes; scan them at startup.
- **One test per assembly** calls `config.Compile()` to validate every mapping. Unmapped members MUST fail the build/test run.
- If compile-time errors are preferred later, use Mapster's source generator (`Mapster.Tool`) or Mapperly. Keep mapping code isolated so swapping is cheap.
- Do not put logic, I/O, or service calls in mappings.

---

## 10. Client.Api conventions

### 10.1 Controllers

- Thin: bind request → map to input → call Application service → map result to response → return.
- No business rules, no `DbContext`, no entities.
- Return `ActionResult<T>` with `ProblemDetails` for errors. Translate `Result<T>`/`Error` in one place (base controller or filter), not per action.

### 10.2 Versioning

- Use `Asp.Versioning` attributes (`[ApiVersion("1.0")]`). **No `V1/` folders.**
- Contracts stay flat until a breaking change forces a fork. The new version goes in a `V2/` subfolder **inside** the use-case's module folder:

```
Contracts/Orders/PlaceOrder.cs          # v1, the default
Contracts/Orders/V2/PlaceOrder.cs       # only when it diverges
```

- Versioning matters for Client.Api because mobile apps cannot be force-upgraded. `Admin.Mvc` needs no versioning.

### 10.3 JSON and contracts

- Enums serialize as **strings** (`JsonStringEnumConverter`) so the wire format is not tied to numeric values.
- API contracts belong to the API. They are shaped for storefront/mobile needs, never for admin screens.
- Do not return Application `Result` types directly from controllers; always map to contracts.

### 10.4 Security

- JWT bearer for storefront and mobile customers; API keys where needed.
- Tenant resolved by `TenantResolution` middleware before authorization.

---

## 11. Admin.Mvc conventions

### 11.1 Areas

Areas are **modules plus `Platform`**:

- `Platform` holds only platform-admin screens (tenants, plans, global settings, platform users).
- Every other area is a module (`Identity`, `Catalog`, `Inventory`, `Warehouses`, `Stores`, `Pos`, `Sales`, `Customers`, `Purchasing`, `Reports`, `Settings`).
- "Platform vs Tenant" is an **authorization and data-scope concern**, not a UI structure. A platform admin and a tenant admin both manage users through `Identity`; the resolved `ITenantContext` and permissions differ.
- Each area: `Controllers/`, `Views/`, `ViewModels/`.

### 11.2 View models and forms

- One file per screen in `ViewModels/`: `OrderList.cs` (list view model, row view model, filter form), `OrderEdit.cs` (form model, lookups).
- Form models are mutable `sealed class`. Display models are `sealed record`.
- ViewModels are UI-specific and never reused as API contracts.
- Prefer server-side validation via the Application validators; add client-side hints only as a convenience.

### 11.3 Security

- Cookie authentication.
- **Permission-based authorization** (`Inventory.AdjustStock`, `Sales.Refund`) via policies; the same permissions are re-checked inside Application services for sensitive actions so both hosts get identical protection.

---

## 12. Cross-cutting rules

### 12.1 Authorization

- Permission names are constants in `Application/Identity/Permissions/`.
- Sensitive Application services call `IPermissionChecker` themselves. Controller attributes are a convenience, not the only defence.
- Platform-only services require `Platform.*` permissions and platform scope.

### 12.2 Validation

- FluentValidation validators live in `Application/{Module}/Validators/`.
- Application services validate `Input` at the start of each use case and return a validation `Result`/`Error`. There is no mediator pipeline.
- Hosts do model binding and basic shape checks only; they never own business rules.

### 12.3 Error handling

- **Expected business failures** (validation, not found, conflict, forbidden) → `Result<T>` with `Error`. Hosts translate to `ProblemDetails` (API) or `ModelState`/flash messages (MVC).
- **Invariant violations** in Domain → `DomainException` subclasses. Middleware maps them to 4xx.
- **Unexpected errors** → global exception middleware logs and returns a generic 500 `ProblemDetails`.
- Do not use exceptions for normal control flow.

### 12.4 Email, PDF, Excel

- Application defines `IEmailSender`, `IPdfGenerator`, `IExcelExporter`. Infrastructure implements them.
- Templates and layouts live in `Infrastructure/Documents` and `Infrastructure/Email`.
- Report data comes from `IReportReader` (SPs). Exporters take Application models, never entities.
- Adding a provider (SendGrid, etc.) means a new implementation and configuration; no caller changes.

### 12.5 Third-party integrations (future)

- Define the abstraction in Application (`IPaymentProcessor`, `IShippingProvider`).
- Implement in `Infrastructure/ExternalServices/{Provider}/`.
- Keep provider DTOs inside that folder. They never leak upward.

### 12.6 Configuration and secrets

- Options classes bound via `IOptions<T>`; validated at startup.
- Tenant-specific settings come from the database (cached), not appsettings.
- Secrets encrypted with Data Protection or held in a secret store; never logged.

### 12.7 Logging and auditing

- Structured logging with `TenantId`, `UserId`, and correlation ID enriched by middleware.
- Audit trail: interceptor stamps audit fields; impersonation and sensitive actions (refunds, stock adjustments, permission changes) are explicitly audit-logged.

### 12.8 Time and IDs

- Use `IClock`; never `DateTime.Now`/`UtcNow` in Domain or Application logic.
- IDs are `Guid` (or sequential GUIDs) generated in Domain/Application, not by callers.

---

## 13. Naming conventions

| Thing | Convention | Example |
|---|---|---|
| Solution/namespaces | `ModularSaaS.{Layer}.{Module}` | `ModularSaaS.Application.Sales` |
| Application inputs/results | `XxxInput`, `XxxResult`, `XxxListItem`, `XxxFilter` | `PlaceOrderInput` |
| API contracts | `XxxRequest`, `XxxResponse` | `PlaceOrderRequest` |
| MVC models | `XxxFormModel`, `XxxListViewModel`, `XxxDetailsViewModel` | `ProductEditFormModel` |
| Service interfaces | `I{Module}Service` or `I{UseCaseGroup}Service` | `IOrderPlacementService` |
| Repositories | `I{Aggregate}Repository` | `IStockRepository` |
| Readers | `I{Module}Reader` | `IInventoryReader` |
| Validators | `{Input}Validator` | `PlaceOrderInputValidator` |
| Mapping classes | `{Module}Mappings` | `SalesMappings` |
| Module registration | `Add{Module}Module` | `AddSalesModule` |
| SPs | `usp_{Module}_{Action}`, `usp_Platform_{Action}` | `usp_Inventory_GetStockLevels` |
| SQL schemas | lower-case module | `inventory`, `sales` |
| Permissions | `{Module}.{Action}` | `Inventory.AdjustStock` |
| Files with multiple types | named after the **use case** | `PlaceOrder.cs` |
| Tests | `{Type}_{Scenario}_{Expected}` | `Adjust_NegativeStock_ReturnsError` |

Avoid: `Manager`, `Helper`, `Utils`, `Common` (as a class name), and the word `Query` for filters (we are not doing CQRS; `Filter`/`Criteria` instead).

---

## 14. Architecture tests

Project: `tests/Architecture.Tests` (NetArchTest.Rules, or ArchUnitNET where noted). These run in CI and block merges.

### 14.1 Rules

| # | Rule |
|---|---|
| A1 | Domain has no dependency on Application, Infrastructure, either host, EF Core, or Mapster |
| A2 | Application does not depend on Infrastructure, `Client.Api`, or `Admin.Mvc` |
| A3 | Infrastructure does not depend on `Client.Api` or `Admin.Mvc` |
| A4 | `Client.Api` and `Admin.Mvc` do not depend on each other |
| A5 | Hosts do not depend on Infrastructure types (allowed only `Program.cs` calling `AddInfrastructure`) |
| A6 | Hosts depend on `ModularSaaS.Domain.*` **only** through `*.Enums` namespaces |
| A7 | Hosts do not use `Microsoft.EntityFrameworkCore`, `AppDbContext`, or repositories |
| A8 | Application public types (`Abstractions`, `Models`) do not expose Domain entities or value objects in signatures |
| A9 | Module A does not depend on Module B's `Services`, `Validators`, `Mapping`, or repository types (only `Abstractions` and `Models`) |
| A10 | Domain module A does not reference Domain module B's entities (IDs only). Shared kernel `Domain.Shared` and enums excepted |
| A11 | Application service implementations, validators, and mappings are `internal` |
| A12 | Classes in `Models`, `Contracts`, `ViewModels` are `sealed` (records/classes) and not abstract |
| A13 | Every class in `Domain/**/Enums` namespace is an enum; every enum in Domain lives in an `Enums` namespace |
| A14 | Repositories are used only by Application services (not by readers, hosts) |
| A15 | Only `Infrastructure.Persistence.Readers.Platform` may call `IgnoreQueryFilters` or reference `PlatformSps` |
| A16 | Every Domain entity implementing `ITenantEntity` has a configured query filter |

### 14.2 Sample (NetArchTest)

```csharp
public class LayerTests
{
    private static readonly Assembly Domain = typeof(Domain.Shared.BaseEntity).Assembly;
    private static readonly Assembly Application = typeof(Application.DependencyInjection).Assembly;

    [Fact]
    public void Domain_ShouldNotDependOnOtherLayers()
    {
        var result = Types.InAssembly(Domain).ShouldNot()
            .HaveDependencyOnAny(
                "ModularSaaS.Application", "ModularSaaS.Infrastructure",
                "ModularSaaS.Client.Api", "ModularSaaS.Admin.Mvc",
                "Microsoft.EntityFrameworkCore")
            .GetResult();

        Assert.True(result.IsSuccessful, Format(result));
    }

    [Fact]
    public void Application_ShouldNotDependOnInfrastructureOrHosts()
    {
        var result = Types.InAssembly(Application).ShouldNot()
            .HaveDependencyOnAny(
                "ModularSaaS.Infrastructure", "ModularSaaS.Client.Api", "ModularSaaS.Admin.Mvc")
            .GetResult();

        Assert.True(result.IsSuccessful, Format(result));
    }
}
```

**Rule A6 (enum-only Domain access from hosts):** NetArchTest's namespace matching is prefix-based, so `ModularSaaS.Domain.Inventory` also matches `ModularSaaS.Domain.Inventory.Enums`. Implement A6 with ArchUnitNET, or a NetArchTest `ICustomRule` that inspects each host type's referenced types and allows only those whose namespace ends in `.Enums`. Verify the rule with a deliberately failing fixture before trusting it.

### 14.3 Non-architecture guard tests

- **Mapster:** `TypeAdapterConfig.GlobalSettings.Compile()` succeeds for every registered mapping.
- **SP tenant scan (integration):** query `sys.parameters` / `sys.sql_modules`; every SP outside the `Platform` folder declares `@TenantId`.
- **SP inventory:** every SP file in `/database/StoredProcedures` is referenced by an `SpNames` constant and vice versa.
- **Cross-tenant leak tests:** for each tenant-scoped reader and repository, data for tenant B is never visible when tenant A is resolved.

---

## 15. Testing strategy

| Project | Scope |
|---|---|
| `Domain.UnitTests` | Aggregate behavior, invariants, value objects, domain services. No mocks of infrastructure |
| `Application.UnitTests` | Services with fake repositories/readers, validators, permission checks |
| `Infrastructure.IntegrationTests` | Real SQL Server (Testcontainers): EF configs, repositories, readers, every SP, tenant isolation |
| `Architecture.Tests` | §14 |
| `Client.Api.IntegrationTests` | `WebApplicationFactory`, auth, tenant resolution, contract shape, versioning |

---

## 16. Checklist: adding a feature

1. Identify the module. If the concept crosses modules, decide the owner and expose it via `Abstractions`/`Models`.
2. **Domain:** add or change entities, value objects, enums (one type per file, enums in `Enums/`). Keep invariants inside the aggregate.
3. **Persistence:** add `IEntityTypeConfiguration` in `Configurations/{Module}/`, create the migration, put tables in the module schema, add `ITenantEntity` if tenant-scoped.
4. **SPs (if needed):** add `database/StoredProcedures/{Module}/usp_{Module}_{Action}.sql` with `@TenantId`; add the constant in `SpNames`; add an integration test including a tenant-leak test.
5. **Application:**
   - use-case file in `Models/` (`XxxInput`, `XxxResult`, ...),
   - validator file in `Validators/`,
   - service in `Services/` (`internal sealed`),
   - interface methods in `Abstractions/`,
   - entity → result mapping in `Mapping/`,
   - permission constant in `Identity/Permissions`,
   - registration in `{Module}Module`.
6. **Infrastructure:** repository (`Repositories/{Module}`) and/or reader (`Readers/{Module}`), registered in `DependencyInjection`.
7. **Host(s):** contract or view-model file per use case/screen, mapping in host `Mapping/`, controller action, authorization policy.
8. Run architecture tests and the Mapster compile test.
9. Update docs/ADR only if a rule here was bent.

---

## 17. Suggested build order

1. `Domain/Shared`, `Tenancy`, `Identity`, `Platform`
2. Persistence: `AppDbContext`, tenant filters, interceptors, SP runner, `SqlScriptRunner`, UoW
3. Architecture tests project (start empty of features, rules on from day one)
4. `Catalog`, then `Inventory` and `Warehouses`
5. `Stores`, `Sales` (online + POS), then `Purchasing`
6. `Reporting`, documents (PDF/Excel), `Notifications`/email
7. `Client.Api` + Angular storefront, alongside `Admin.Mvc` areas

---

## 18. Anti-patterns (reject in review)

- Controller or view referencing `DbContext`, an entity, or a repository.
- Domain entity returned from an Application service, or a value object inside a `Result`.
- `IQueryable` leaving a repository or reader.
- Repository used for grids/reports; `SaveChanges` called from a repository.
- Module B calling Module A's repository, or holding a navigation property to Module A's entity.
- SP without `@TenantId` outside `Platform/`; SP name as a string literal.
- Tenant-scoped code that works when `TenantId` is null instead of throwing.
- Nullable `TenantId` on shared entities to "support platform".
- Mapster mapping input → entity; mapping logic containing business rules.
- One shared DTO used by API and admin "to save time".
- DTO base-class hierarchies; positional records with many parameters.
- A god service with dozens of methods; classes named `Manager`/`Helper`.
- `DateTime.UtcNow` directly in business logic.
- A `V1/` folder for API contracts created before a `V2` exists.
- Adding a mediator, event bus, or per-module DbContext without an ADR.

---

## 19. When to revisit this architecture

Write an ADR and reconsider full modular-monolith isolation (project per module, per-module contexts, public contract assemblies), or extraction of a module, when any of these becomes true:

- multiple teams own different modules and block each other,
- a module needs independent scaling, deployment, or a different data store,
- build time or model-build time becomes a real problem,
- async/event workflows become necessary (introduce outbox + bus deliberately, not incrementally).

Until then, keep boundaries enforced and the structure boring.

---

## 20. Tooling enforcement

Rules are enforced in three places. A rule with no automated check is a convention, and conventions rot.

| Rule | Enforced by |
|---|---|
| Namespace = folder path, file-scoped namespaces | `.editorconfig` (IDE0130, IDE0161) |
| Explicit accessibility; internal types sealed | `.editorconfig` (IDE0040, CA1852) |
| Domain: one type per file; Application `Shared/Models`: one type per file | `.editorconfig` + StyleCop (SA1402, SA1649) |
| Use-case/screen files may hold many types (Models, Validators, Contracts, ViewModels) | `.editorconfig` relaxations |
| Type-name suffixes by folder (Service, Validator, Mappings, Module, Controller, Configuration, Repository, Reader, Sps, Exception, `*Model` for ViewModels) | `.editorconfig` naming rules |
| Interfaces `I*`, `_camelCase` fields, `*Async` suffix | `.editorconfig` naming rules |
| No `DateTime.Now/UtcNow` (use `IClock`) | `BannedSymbols.txt` + RS0030 |
| `IgnoreQueryFilters` only in `Persistence/Readers/Platform` | `BannedSymbols.txt` + RS0030 folder carve-out |
| Nullable, warnings as errors, SQL-injection review, security analyzers | `Directory.Build.props` + `.editorconfig` |
| Layer, module, and host boundaries; enums-only Domain access; sealed public models; no entities in public signatures; enums in `Enums/` | `tests/Architecture.Tests` (§14) |
| SP `@TenantId`, SP/constant parity, tenant-leak tests | Integration tests (§14.3) |
| Mapster mappings complete | `Compile()` test (§14.3) |
| Records vs classes, `required`/`init`, no DTO inheritance, promotion ladder, no cross-module sharing by similarity | Code review (checklist §16, anti-patterns §18) |

Files: `.editorconfig`, `stylecop.json`, `BannedSymbols.txt`, `Directory.Build.props`, `Directory.Packages.props` at the solution root. CI MUST build with `--warnaserror` (or rely on `TreatWarningsAsErrors`) and run `Architecture.Tests`.

### 20.1 Architecture test implementation

`tests/Architecture.Tests` uses a small Mono.Cecil dependency scanner (`Support/Deps.cs`) instead of NetArchTest, so every rule reports precise `Type -> Dependency` pairs, can express the enums-only and public-surface rules (A6, A8), and supports the **baseline ratchet** (`Baseline/*.txt`) used during migration. Layer rules are assembly-based; module and surface rules are namespace-based on the target namespaces. `Support/ArchConfig.cs` is the only file with solution-specific values. NetArchTest or ArchUnitNET may still be added for extra rules.

Migration mode vs strict mode: `.editorconfig.strict` is the source of truth; `tools/Set-Strictness.ps1` generates `.editorconfig` in either mode.
