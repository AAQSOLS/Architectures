# ModularSaaS Architecture & System Design Guide

This document provides a visual and structural reference of the ModularSaaS template architecture. It is designed to be shared with technical leads, architects, and engineering teams.

---

## 1. High-Level System Architecture

The system follows **Clean Architecture** organized with **modular vertical folders** and **isolated pluggable capability libraries**.

```mermaid
flowchart TD
    subgraph ClientZone["1. External Clients"]
        SPA["Frontend SPA (Angular / React)"]
        Mobile["Mobile Apps"]
        External["Third-Party APIs / Daemons"]
    end

    subgraph HostLayer["2. Host Layer (apps/Api)"]
        MiddlewarePipe["HTTP Middleware Pipeline<br/>- Exception Handling<br/>- OpenTelemetry Tracing<br/>- Serilog Context Enrichment<br/>- JWT Bearer Authentication<br/>- Tenant Resolution<br/>- Tenant Isolation Guard (403)<br/>- Dynamic Permission Check"]
        Controllers["Controllers<br/>(Identity, Tenancy, Platform)"]
        HostContracts["Contracts (Requests / Responses)<br/>+ Host Mapster Mappings"]
    end

    subgraph AppLayer["3. Application Layer (src/Application)"]
        Services["Application Services<br/>(UserService, TenantService, AuthService)"]
        AppModels["Models & DTOs<br/>+ FluentValidators + App Mappings"]
        AppAbstractions["Core Ports & Interfaces<br/>(IRepositories, IReaders, IClock)"]
    end

    subgraph DomainLayer["4. Domain Layer (src/Domain)"]
        Entities["Entities & Aggregates<br/>(User, Tenant, Role, RefreshToken)"]
        DomainEnums["Domain Enums<br/>(UserStatus, TenantStatus, TenantPlan)"]
        TenantMarker["ITenantEntity (Isolation Contract)"]
    end

    subgraph InfraLayer["5. Infrastructure Layer (src/Infrastructure)"]
        DbContext["AppDbContext (Internal)"]
        Interceptors["EF Core Interceptors<br/>- TenantInterceptor (Auto-stamps TenantId)<br/>- AuditInterceptor (Timestamps)"]
        DataStores["Repositories & Readers<br/>(SQL Server Database)"]
    end

    subgraph Plugins["6. Standalone Reusable Plugins (src/Plugins)"]
        SecurityPlugin["ModularSaaS.Security<br/>- Abstractions (ICurrentUser, IPasswordHasher)<br/>- Core (JsonWebTokenHandler, BCrypt)<br/>- AspNetCore (RBAC Handler, Tenant Guard)"]
        ObservabilityPlugin["ModularSaaS.Observability<br/>- OpenTelemetry Traces & Metrics<br/>- PII & Query Redaction<br/>- OTLP & Azure Monitor Exporters"]
    end

    %% Flow Connections
    SPA -->|HTTPS / REST| MiddlewarePipe
    Mobile -->|HTTPS / REST| MiddlewarePipe
    External -->|API Key / Bearer| MiddlewarePipe
    MiddlewarePipe --> Controllers
    Controllers --> HostContracts

    Controllers -->|Input DTOs| Services
    Services --> AppModels
    Services --> AppAbstractions

    Services --> Entities
    Entities --> TenantMarker
    HostContracts -.->|Enums Only| DomainEnums

    AppAbstractions -.->|Implemented by| DataStores
    DataStores --> DbContext
    DbContext --> Interceptors

    SecurityPlugin -.->|Security Primitives| AppLayer
    SecurityPlugin -.->|Guards & Policies| HostLayer
    ObservabilityPlugin -.->|Tracing & Metrics| HostLayer
```

---

## 2. Request Lifecycle & Security Barrier Sequence

This sequence illustrates how a write request traverses the pipeline, undergoes multi-tenant validation, passes business rules, and is persisted with automatic tenant isolation.

```mermaid
sequenceDiagram
    autonumber
    actor Client as Client App
    participant Pipe as Middleware Pipeline
    participant Ctrl as UsersController
    participant Svc as UserService
    participant Repo as UserRepository
    participant Interceptor as TenantInterceptor
    participant DB as SQL Server

    Client->>Pipe: POST /api/v1/users (Bearer Token + JSON)
    Note over Pipe: 1. Trace started (OpenTelemetry W3C)<br/>2. JWT signature & expiry verified (Zero Clock Skew)<br/>3. Tenant resolved from claim / header<br/>4. Tenant Guard: Token tid == Header tid?<br/>5. HasPermission("Users.Write")
    Pipe->>Ctrl: Invoke CreateUser(CreateUserRequest)
    Note over Ctrl: Mapster (Host): CreateUserRequest -> CreateUserInput
    Ctrl->>Svc: RegisterUserAsync(CreateUserInput)
    Note over Svc: 1. FluentValidation check<br/>2. Business invariant verification<br/>3. Instantiate User aggregate
    Svc->>Repo: AddAsync(user)
    Repo->>Interceptor: SaveChangesAsync()
    Note over Interceptor: Automatically stamps TenantId on all ITenantEntity records
    Interceptor->>DB: INSERT INTO Users (Id, TenantId, Email, ...)
    DB-->>Interceptor: Rows Affected: 1
    Interceptor-->>Repo: Saved
    Svc->>Svc: Mapster (App): User -> UserResult
    Svc-->>Ctrl: UserResult
    Note over Ctrl: Mapster (Host): UserResult -> UserResponse
    Ctrl-->>Pipe: 201 Created (UserResponse)
    Note over Pipe: Record saas_requests_total metric
    Pipe-->>Client: 201 Created JSON
```

---

## 3. Multi-Tenant Data Isolation Architecture

Data isolation is enforced at the database and query pipeline level to prevent cross-tenant data leaks.

```mermaid
flowchart LR
    subgraph IncomingRequest["Incoming Request"]
        Token["JWT Token (tid: Tenant-1)"]
        Header["Header: X-Tenant-Id: Tenant-1"]
    end

    subgraph DefenseGuards["Defense in Depth"]
        Guard["TenantIsolationGuardMiddleware<br/>Validates: Token.tid == Header.tid<br/>(Fails closed with 403 Forbidden)"]
        Context["ITenantContext<br/>Scoped Ambient Tenant State"]
    end

    subgraph DataPipeline["Persistence Boundary"]
        QueryFilter["EF Core Global Query Filter<br/>WHERE e.TenantId == CurrentTenantId"]
        Interceptor["TenantInterceptor<br/>e.TenantId = CurrentTenantId on INSERT"]
        DB[(Shared Multi-Tenant Database)]
    end

    IncomingRequest --> Guard
    Guard --> Context
    Context --> QueryFilter
    Context --> Interceptor
    QueryFilter --> DB
    Interceptor --> DB
```

---

## 4. Standalone Security Plugin Suite (`src/Plugins/Security`)

The security subsystem is decoupled from domain aggregates into three composable libraries packageable as NuGet packages.

```mermaid
flowchart TD
    subgraph Abstractions["ModularSaaS.Security.Abstractions (Zero Dependencies)"]
        ICurrentUser["ICurrentUser<br/>- UserId<br/>- TenantId<br/>- Principal"]
        CurrentUserExt["CurrentUserExtensions<br/>- GetEmail()<br/>- HasRole()<br/>- IsPlatformAdmin()<br/>- GetPermissions()"]
        Contracts["Contracts<br/>- IPasswordHasher<br/>- ITokenService<br/>- ISecureRandomGenerator<br/>- ITokenRevocationRegistry<br/>- ISecurityEventSink"]
    end

    subgraph Core["ModularSaaS.Security.Core (Crypto Engine)"]
        JwtService["JsonWebTokenService<br/>- Modern JsonWebTokenHandler<br/>- Zero clock-skew default<br/>- Symmetric & Asymmetric rollover"]
        PasswordHasher["BCryptPasswordHasher<br/>- Work factor inspection<br/>- Auto-rehash detection"]
        RandomGen["SecureRandomGenerator<br/>- Constant-time equality (FixedTimeEquals)"]
        Revocation["InMemoryTokenRevocationRegistry<br/>- Thread-safe token invalidation"]
    end

    subgraph AspNetCore["ModularSaaS.Security.AspNetCore (HTTP & Policies)"]
        PolicyProvider["PermissionPolicyProvider<br/>- Dynamic policy generation"]
        AuthHandler["PermissionAuthorizationHandler<br/>- Claims + IPermissionEvaluator"]
        TenantGuard["TenantIsolationGuardMiddleware<br/>- Cross-tenant replay protection"]
        Builder["SecurityPluginBuilder<br/>- services.AddSecurityPlugin()"]
    end

    Abstractions --> Core
    Core --> AspNetCore
```

---

## 5. OpenTelemetry Observability Pipeline (`src/Plugins/Observability`)

Observability is vendor-agnostic and built on open standards, supporting Azure Monitor, Datadog, Grafana/Prometheus, or Jaeger via OTLP.

```mermaid
flowchart LR
    subgraph Sources["Instrumentation Sources"]
        Http["Incoming HTTP Requests"]
        Client["Outbound HttpClient"]
        Runtime["Runtime Metrics (GC/CPU)"]
        Custom["SaaSActivities & SaaSMetrics"]
    end

    subgraph Processing["Processing & Sanitization"]
        Filter["DefaultTelemetryFilter<br/>Drops: /health, /openapi, /scalar"]
        Sanitizer["SensitiveDataSanitizer<br/>Redacts: Query strings, Authorization, Cookies"]
        Enricher["TenantTelemetryEnricher<br/>Injects: tenant.id, user.id, correlation.id"]
        Sampler["ParentBasedAdaptiveSampler<br/>100% Errors / 5% Normal"]
    end

    subgraph Exporters["Configurable Exporters"]
        Otlp["OTLP Collector<br/>(Datadog, Grafana, Jaeger)"]
        Azure["Azure Monitor<br/>(Application Insights)"]
        Console["Console / Local Dev"]
    end

    Sources --> Filter
    Filter --> Sanitizer
    Sanitizer --> Enricher
    Enricher --> Sampler
    Sampler --> Otlp
    Sampler --> Azure
    Sampler --> Console
```

---

## 6. Two-Hop Mapping & CQRS Separation

To prevent entity leakages and keep query paths fast, the system uses two separate mappings and segregates read paths from write repositories.

```mermaid
flowchart TD
    subgraph WritePath["Write / Command Path"]
        WriteReq["CreateUserRequest (Host Contract)"]
        WriteMap1["Host Mapster"]
        WriteInput["CreateUserInput (App Model)"]
        WriteSvc["UserService (App Service)"]
        WriteRepo["UserRepository (Write Repository)"]
        WriteEntity["User Entity (Domain)"]

        WriteReq --> WriteMap1 --> WriteInput --> WriteSvc --> WriteRepo --> WriteEntity
    end

    subgraph ReadPath["Read / Query Path (CQRS Reader)"]
        ReadReq["GetUsersRequest (Host Contract)"]
        ReadMap1["Host Mapster"]
        ReadSvc["UserQueryService"]
        Reader["IIdentityReader (Direct Projection)"]
        ReadResult["UserListItem (Read Model)"]
        ReadMap2["Host Mapster"]
        ReadResp["UserListItemResponse (Host Contract)"]

        ReadReq --> ReadMap1 --> ReadSvc --> Reader --> ReadResult --> ReadMap2 --> ReadResp
    end
```

---

## 7. What Goes Where Reference Guide

| Layer | Path | What Goes Here | What NEVER Goes Here |
|---|---|---|---|
| **Domain** | `src/Domain/` | Entities, Domain Events, Domain Enums, `ITenantEntity`. | No EF Core, no Mapster, no ASP.NET, no repositories. |
| **Application** | `src/Application/` | Services, Use Cases, Ports/Interfaces, DTOs, FluentValidators, Application Mappings. | No SQL queries, no `AppDbContext`, no HTTP Controllers. |
| **Infrastructure** | `src/Infrastructure/` | Internal `AppDbContext`, Repositories, Readers, Interceptors, Migrations. | No business validation rules, no HTTP controllers. |
| **Host (API)** | `apps/Api/` | Controllers, Host Contracts, Middleware, OpenAPI, Scalar UI, Host Mappings, `Program.cs`. | No `AppDbContext`, no SQL, no direct Domain entity exposure. |
| **Security Plugin** | `src/Plugins/Security/` | Token engine, BCrypt hashing, `ICurrentUser`, dynamic RBAC provider, tenant guard. | No hardcoded business tables or tenant domain entities. |
| **Observability Plugin** | `src/Plugins/Observability/` | ActivitySource, Metrics Meters, PII sanitizers, `/health` filters, OTLP exporters. | No hardcoded vendor-specific tracing SDKs in business code. |

---

## 8. Automated Architecture Enforcement Matrix

The architecture is continuously protected by **35 automated tests**:

1. **Layer Boundary Enforcement**:
   - `A1`: Domain depends on nothing outward.
   - `A2`: Application does not reference Infrastructure or Hosts.
   - `A3`: Infrastructure does not reference Hosts.
   - `A5`: Hosts interact with Infrastructure exclusively via `Program.cs`.
   - `A7`: Hosts do not reference Entity Framework.
2. **Encapsulation & Type Safety**:
   - `A5b`: Hosts use only Application Abstractions and Models.
   - `A6`: Hosts interact with Domain exclusively through Enums.
   - `A8`: Application public surface exposes zero Domain types except Enums.
   - `A11`: Services, Validators, and Mappings are strictly internal.
   - `A12`: All Models, Contracts, and DTOs are sealed records.
3. **CQRS & Query Separation**:
   - `A14`: Query Readers do not depend on write repositories.
4. **Security & Observability Verifications**:
   - Signature tampering rejection, work-factor re-hash detection, zero clock-skew expiration, PII header sanitization, and health check filtering.
