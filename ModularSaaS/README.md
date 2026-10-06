# ModularSaaS - Production-Grade .NET 10 Multi-Tenant Modular Monolith

A production-grade, Clean Architecture modular monolith backend template targeting .NET 10, Entity Framework Core 10, PostgreSQL, and ASP.NET Core Web API.

---

## 1. Solution Architecture

The solution implements Clean Architecture inward dependency boundaries with modular domain organization:

```
apps/
  Api/                           # Single Web API host (Controllers, Middleware, Swagger, Auth)
src/
  Domain/                        # Enterprise entities, value objects, domain invariants, enums
  Application/                   # Use cases, interfaces, validators, DTO models (Identity & Tenancy)
  Infrastructure/                # EF Core 10 persistence, PostgreSQL, JWT, BCrypt, clock, seeders
tests/
  Architecture.Tests/            # Automated Mono.Cecil architecture rule suite (A1-A13)
```

### Dependency Rules Enforced

- **Domain** depends on nothing outward (zero framework dependencies, no EF Core, no host packages).
- **Application** depends strictly on Domain. Zero references to Infrastructure or Host.
- **Infrastructure** implements Application abstractions and depends on Application and Domain.
- **Api Host** references Application abstractions and models. References Infrastructure exclusively in `Program.cs` for DI registration.
- **Module Boundaries**: Features are partitioned into folder-based modules (`Identity`, `Tenancy`). Modules interact strictly through Application interfaces and DTO models, never through foreign Domain entities.

---

## 2. Core Capabilities

### Multi-Tenancy
- **Strategy**: Multi-tenant single database with discriminator column (`TenantId`) on all tenant-owned entities (`ITenantEntity`).
- **Isolation**: EF Core Global Query Filters dynamically isolate data at the DbContext level using the scoped `ITenantContext`.
- **Platform Scope Bypass**: Global query filters automatically deactivate when `ITenantContext.IsPlatformScope` is enabled, permitting super-admin platform management without raw SQL leaks.
- **Tenant Resolution**: Handled automatically in `TenantResolutionMiddleware` via `X-Tenant-Id` header or authenticated JWT claims.

### Security & Identity
- **Normalized 7-Table RBAC**: Granular relational permission system with `Users`, `Roles`, `Permissions`, `UserRoles`, `RolePermissions`, `UserPermissions` (explicit user-level grants/revocations), and `PlatformUsers`.
- **Platform Super-Admin & Impersonation**: Dedicated platform administrators (`PlatformUsers`) with audit-tracked tenant impersonation adhering to **RFC 8693** (`act` actor claim).
- **Token Security**: HMAC-SHA256 JWT access tokens paired with high-entropy cryptographic refresh tokens stored in PostgreSQL with rotation tracking.
- **Password Safety**: BCrypt work-factor hashing with cryptographically secure, single-use password recovery tokens.
- **Permission Policy Provider**: Custom `IPolicyProvider` dynamically transforms `[HasPermission("...")]` controller attributes into ASP.NET Core authorization policies evaluated against cached user permission sets (`IPermissionCache`).
- **Defensive Rate Limiting**: Built-in ASP.NET Core RateLimiter configured with partition policies: strict limits on auth/login endpoints (5 req/min per IP) and standard limits on authenticated API calls (100 req/min).

---

## 3. Getting Started

### Prerequisites
- .NET 10 SDK (v10.0.100 or later)
- PostgreSQL 15+ (local instance or Docker container)

### Configuration

Connection strings and JWT settings are configured in [`apps/Api/appsettings.json`](file:///D:/Repos/Personal/SOL/Repos/ModularSaaS/apps/Api/appsettings.json):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=ModularSaaS;Username=postgres;Password=postgres"
  },
  "Jwt": {
    "Issuer": "ModularSaaS",
    "Audience": "ModularSaaS.Client",
    "Secret": "ReplaceWithASecretKeyAtLeast32BytesLongForSecurityInProduction123!",
    "ExpiryMinutes": 60,
    "RefreshTokenDays": 7
  }
}
```

### Running the Application

Execute the API host from the command line:

```bash
dotnet run --project apps/Api/ModularSaaS.Api.csproj
```

In `Development` mode, the application will automatically:
1. Apply pending EF Core database migrations.
2. Seed the system tenant (`00000000-0000-0000-0000-000000000001`).
3. Seed system permissions catalog (Users, Roles, Tenants, Permissions).
4. Seed default tenant roles (`Admin`, `Member`).
5. Seed initial platform super-admin (configured via `Seed:PlatformAdminEmail` and `Seed:PlatformAdminPassword`).

Swagger UI will be available at:
`https://localhost:5001/swagger` (or corresponding port assigned by launch settings).

---

## 4. Seed Credentials Configuration (Development)

| User Type | Default Email | Password | Scope |
|---|---|---|---|
| Platform Super-Admin | `superadmin@modularsaas.local` | Configured via `user-secrets` or env (`Seed:PlatformAdminPassword`) | Global Platform (`/api/platform/*`) |

---

## 5. Automated Verification Gates

### Compiling All Projects
Verify clean compilation with zero warnings and zero errors across the entire solution:

```bash
dotnet build ModularSaaS.slnx
```

### Running Architecture Tests
Validate that all layer boundaries, internal visibility constraints, sealed models, and module isolation rules are respected:

```bash
dotnet test tests/Architecture.Tests/ModularSaaS.Architecture.Tests.csproj
```

Suite covers:
- `A1`: Domain depends on nothing outward.
- `A2`: Application does not depend on Infrastructure or Hosts.
- `A3`: Infrastructure does not depend on Hosts.
- `A5`: Hosts use Infrastructure only from `Program.cs`.
- `A5b`: Hosts use only Application Abstractions, Models, Permissions, and Exceptions.
- `A6`: Hosts access Domain exclusively through Enums.
- `A7`: Hosts do not reference Entity Framework Core directly.
- `A8`: Application public surface exposes no Domain types except Enums.
- `A9a`: Application modules talk to other modules only via Abstractions and Models.
- `A9b`: Application modules never reference foreign Domain types.
- `A10`: Domain modules reference other modules by ID only.
- `A11`: Services, Validators, and Mappings are strictly internal.
- `A12`: All DTO Models and API Contracts are sealed.
- `A13`: Domain enums reside exclusively in `.Enums` namespaces.
