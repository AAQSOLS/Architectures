# ModularSaaS: Production Blueprint & Implementation Plan

## 1. System Overview
- **Architecture:** Clean Architecture Modular Monolith (.NET 10).
- **Style:** Service-based use cases. No MediatR, no CQRS message bus, no domain events.
- **Tenancy:** Multi-tenant single-database with discriminator (`TenantId`), fail-closed security, and EF Core Global Query Filters with platform scope bypass.
- **Identity & RBAC:**
  - **Platform Super Admin:** Dedicated `identity.PlatformUsers` table (no `TenantId`), isolated login route (`/api/platform/auth/login`), and audited impersonation (`POST /api/platform/tenants/{id}/impersonate`) producing RFC 8693 `act` actor claims.
  - **Tenant Users & Normalized RBAC:** 7-table relational schema (`Users`, `Roles`, `Permissions`, `RolePermissions`, `UserRoles`, `UserPermissions`, `PasswordResetTokens`).
  - **Self-Serve Password Recovery:** Dedicated forgot password and token-validated password reset endpoints.
  - **Security & Protection:** Rate-limited authentication endpoints (`AddRateLimiter`), BCrypt hashing, rotating refresh tokens with replay detection, brute-force lockout tracking, system role immutability, and time-bound roles (`ExpiresAtUtc`).
  - **Token Optimization:** Permissions cached server-side via `HybridCache` / `IMemoryCache` (invalidated on role edits) to prevent HTTP 431 header overflow.
- **Developer Experience & Operations:** Automatic EF Core migration runner on development startup, health checks at `/health`, Serilog structured logging with tenant/user enrichment, and RFC 7807 error responses.
- **Hosts:** Single Web API host (`ModularSaaS.Api`). No MVC host.
- **Code Governance:** Custom Mono.Cecil architecture tests (`ModularSaaS.Architecture.Tests`), Banned API analyzers (`RS0030`), and `.editorconfig` (placeholder for user-supplied configuration).

---

## 2. Solution Layout

```text
ModularSaaS/
├── ModularSaaS.sln (or ModularSaaS.slnx)
├── Directory.Build.props              # Analyzers, warnings, net10.0 target
├── Directory.Packages.props           # Central Package Management (CPM)
├── .editorconfig                      # Placeholder for user-supplied configuration
├── BannedSymbols.txt                  # Bans DateTime.Now/UtcNow, restricts IgnoreQueryFilters
├── stylecop.json                      # StyleCop metadata configuration
├── PLAN.md                            # Blueprint and execution steps
├── README.md                          # Quickstart, setup, and verification guide
│
├── src/
│   ├── Domain/                        # ModularSaaS.Domain (References NOTHING)
│   │   ├── Shared/
│   │   │   ├── BaseEntity.cs          # Guid Id base
│   │   │   ├── AuditableEntity.cs     # Timestamps and audit user IDs
│   │   │   ├── ITenantEntity.cs       # Marker interface: Guid TenantId { get; set; }
│   │   │   ├── IAggregateRoot.cs      # Repository boundary marker
│   │   │   └── DomainException.cs     # Invariant violation exception
│   │   ├── Tenancy/
│   │   │   ├── Tenant.cs              # Platform aggregate (Name, Slug, Status, Plan)
│   │   │   └── Enums/
│   │   │       ├── TenantStatus.cs    # Active, Suspended, PendingVerification
│   │   │       └── TenantPlan.cs      # Free, Starter, Professional, Enterprise
│   │   └── Identity/
│   │       ├── PlatformUser.cs        # Global Super Admin (No TenantId)
│   │       ├── User.cs                # Tenant User (TenantId, Email, Security Fields)
│   │       ├── Role.cs                # Tenant Role (TenantId, Name, IsSystem, IsDefault)
│   │       ├── Permission.cs          # Global Permission Catalog (Code, Name, Module)
│   │       ├── RolePermission.cs      # Join: RoleId + PermissionId
│   │       ├── UserRole.cs            # Join: UserId + RoleId + Audit/Expiration
│   │       ├── UserPermission.cs      # Direct User Override: UserId + PermissionId + IsGranted
│   │       ├── RefreshToken.cs        # Tenant Entity (Token, UserId, Expiration, Replay)
│   │       ├── PasswordResetToken.cs  # Tenant Entity (TokenHash, UserId, Expiry, UsedAtUtc)
│   │       └── Enums/
│   │           └── UserStatus.cs      # Active, Inactive, Locked
│   │
│   ├── Application/                   # ModularSaaS.Application (References Domain only)
│   │   ├── Shared/
│   │   │   ├── Abstractions/
│   │   │   │   ├── IRepository.cs     # Generic base repository
│   │   │   │   ├── IUnitOfWork.cs     # Transaction commit contract
│   │   │   │   ├── ITenantContext.cs  # TenantId, IsPlatformScope, Impersonation state
│   │   │   │   ├── ICurrentUser.cs    # UserId, TenantId, IsPlatformAdmin, ActorId
│   │   │   │   ├── IClock.cs          # UtcNow provider replacing DateTime.Now
│   │   │   │   ├── IEmailSender.cs    # Outbound email interface
│   │   │   │   └── IPermissionCache.cs# Permission caching and invalidation abstraction
│   │   │   ├── Models/
│   │   │   │   ├── Result.cs          # Generic and non-generic Result pattern
│   │   │   │   ├── Error.cs           # Error record with code, description, ErrorType
│   │   │   │   ├── ErrorType.cs       # Failure, Validation, NotFound, Conflict, Unauthorized
│   │   │   │   ├── ValidationError.cs # Property-level validation error details
│   │   │   │   ├── PagedResult.cs     # Standard paged response wrapper
│   │   │   │   └── PageRequest.cs     # Page and PageSize request parameters
│   │   │   └── Exceptions/
│   │   │       ├── TenantRequiredException.cs
│   │   │       ├── ValidationException.cs
│   │   │       ├── NotFoundException.cs
│   │   │       └── UnauthorizedException.cs
│   │   ├── Tenancy/
│   │   │   ├── Abstractions/
│   │   │   │   ├── ITenantService.cs  # Provisioning and lifecycle
│   │   │   │   └── ITenantRepository.cs
│   │   │   ├── Models/
│   │   │   │   ├── CreateTenant.cs    # CreateTenantInput, TenantResult
│   │   │   │   └── GetTenants.cs      # TenantListItem, TenantFilter
│   │   │   ├── Services/
│   │   │   │   └── TenantService.cs   # internal sealed implementation
│   │   │   ├── Validators/
│   │   │   │   └── CreateTenantValidator.cs
│   │   │   └── TenancyModule.cs       # Module DI registration
│   │   ├── Identity/
│   │   │   ├── Abstractions/
│   │   │   │   ├── IAuthService.cs    # Login, Refresh, Revoke, ForgotPassword, ResetPassword
│   │   │   │   ├── IPlatformAuthService.cs # Super admin login and tenant impersonation
│   │   │   │   ├── IUserService.cs    # Profile, Register, ChangePassword, ListUsers
│   │   │   │   ├── IRoleService.cs    # Role CRUD and permission assignments
│   │   │   │   ├── IPermissionService.cs # Matrix queries and metadata
│   │   │   │   ├── ITokenService.cs   # JWT generation using primitives and claims
│   │   │   │   ├── IPasswordHasher.cs # BCrypt hashing and verification
│   │   │   │   ├── IUserRepository.cs
│   │   │   │   ├── IPlatformUserRepository.cs
│   │   │   │   ├── IRoleRepository.cs
│   │   │   │   ├── IPermissionRepository.cs
│   │   │   │   ├── IPasswordResetTokenRepository.cs
│   │   │   │   └── IRefreshTokenRepository.cs
│   │   │   ├── Models/
│   │   │   │   ├── Login.cs           # LoginInput, AuthTokensResult
│   │   │   │   ├── PlatformLogin.cs   # PlatformLoginInput, PlatformAuthResult
│   │   │   │   ├── Impersonate.cs     # ImpersonateInput, ImpersonationResult
│   │   │   │   ├── Register.cs        # RegisterUserInput, UserResult
│   │   │   │   ├── RefreshToken.cs    # RefreshTokenInput
│   │   │   │   ├── ChangePassword.cs  # ChangePasswordInput
│   │   │   │   ├── ForgotPassword.cs  # ForgotPasswordInput
│   │   │   │   ├── ResetPassword.cs   # ResetPasswordInput
│   │   │   │   ├── RoleModels.cs      # CreateRoleInput, UpdateRoleInput, RoleResult
│   │   │   │   ├── PermissionModels.cs# PermissionMatrixItem, PermissionGroupResult
│   │   │   │   └── UserResult.cs      # UserDetailsResult, UserListItem
│   │   │   ├── Services/
│   │   │   │   ├── AuthService.cs     # internal sealed
│   │   │   │   ├── PlatformAuthService.cs # internal sealed
│   │   │   │   ├── UserService.cs     # internal sealed
│   │   │   │   └── RoleService.cs     # internal sealed
│   │   │   ├── Validators/
│   │   │   │   ├── LoginValidator.cs, RegisterValidator.cs, CreateRoleValidator.cs
│   │   │   │   ├── ForgotPasswordValidator.cs, ResetPasswordValidator.cs
│   │   │   ├── Permissions/
│   │   │   │   └── AppPermissions.cs  # Module.Resource.Action constants
│   │   │   └── IdentityModule.cs      # Module DI registration
│   │   └── DependencyInjection.cs     # AddApplication() wiring modules
│   │
│   └── Infrastructure/                # ModularSaaS.Infrastructure (References Application + Domain)
│       ├── Persistence/
│       │   ├── AppDbContext.cs        # Global query filters + platform bypass
│       │   ├── Configurations/
│       │   │   ├── Tenancy/
│       │   │   │   └── TenantConfiguration.cs
│       │   │   └── Identity/
│       │   │       ├── PlatformUserConfiguration.cs
│       │   │       ├── UserConfiguration.cs
│       │   │       ├── RoleConfiguration.cs
│       │   │       ├── PermissionConfiguration.cs
│       │   │       ├── RolePermissionConfiguration.cs
│       │   │       ├── UserRoleConfiguration.cs
│       │   │       ├── UserPermissionConfiguration.cs
│       │   │       ├── RefreshTokenConfiguration.cs
│       │   │       └── PasswordResetTokenConfiguration.cs
│       │   ├── Repositories/
│       │   │   ├── EfRepository.cs
│       │   │   ├── TenantRepository.cs
│       │   │   ├── PlatformUserRepository.cs
│       │   │   ├── UserRepository.cs
│       │   │   ├── RoleRepository.cs
│       │   │   ├── PermissionRepository.cs
│       │   │   ├── PasswordResetTokenRepository.cs
│       │   │   └── RefreshTokenRepository.cs
│       │   ├── Interceptors/
│       │   │   ├── TenantInterceptor.cs # Auto-stamps TenantId on added ITenantEntity
│       │   │   └── AuditInterceptor.cs  # Auto-stamps audit timestamps and actor IDs
│       │   ├── Caching/
│       │   │   └── PermissionCache.cs # In-memory / HybridCache provider for resolved permissions
│       │   ├── Seed/
│       │   │   └── DbInitializer.cs   # Seeds system catalog, default roles, root platform admin
│       │   └── UnitOfWork.cs          # IUnitOfWork backed by AppDbContext
│       ├── Security/
│       │   ├── PasswordHasher.cs      # BCrypt implementation
│       │   ├── JwtTokenService.cs     # JWT generator (supports standard and RFC 8693 act claims)
│       │   └── CurrentUser.cs         # ICurrentUser backed by HttpContext ClaimsPrincipal
│       ├── Tenancy/
│       │   └── TenantContext.cs       # Scoped state holding resolved tenant and impersonation flags
│       ├── Time/
│       │   └── SystemClock.cs         # IClock backed by DateTimeOffset.UtcNow (RS0030 carve-out)
│       └── DependencyInjection.cs     # AddInfrastructure() wiring persistence and auth
│
├── apps/
│   └── Api/                           # ModularSaaS.Api (Composition Root)
│       ├── Controllers/
│       │   ├── BaseApiController.cs   # Result<T> mapping to ProblemDetails
│       │   ├── AuthController.cs      # Tenant auth: login, refresh, revoke, forgot/reset password
│       │   ├── UsersController.cs     # Tenant users: me, change-password, list
│       │   ├── RolesController.cs     # Tenant roles: CRUD + assign permissions
│       │   ├── PermissionsController.cs# Matrix grouped by module
│       │   └── Platform/
│       │       ├── PlatformAuthController.cs # Super admin login: /api/platform/auth/login
│       │       └── PlatformTenantsController.cs # /api/platform/tenants, /impersonate
│       ├── Contracts/
│       │   ├── Auth/
│       │   ├── Platform/
│       │   ├── Users/
│       │   ├── Roles/
│       │   └── Tenants/
│       ├── Middleware/
│       │   ├── TenantResolutionMiddleware.cs # Detects platform routes vs tenant routes, checks X-Tenant-Id
│       │   └── ExceptionHandlingMiddleware.cs # Maps exceptions to RFC 7807 ProblemDetails
│       ├── Security/
│       │   ├── HasPermissionAttribute.cs
│       │   └── PermissionAuthorizationHandler.cs # Checks cache/claims for required permission
│       ├── Extensions/
│       │   ├── MigrationExtensions.cs # app.ApplyMigrationsAsync() runner on startup
│       │   ├── RateLimitingExtensions.cs # IP & endpoint rate limiting policies
│       │   ├── SerilogExtensions.cs   # Enriches logs with TenantId, UserId, ImpersonatedBy
│       │   └── SwaggerExtensions.cs   # Swagger UI with Bearer auth and X-Tenant-Id header
│       ├── appsettings.json
│       ├── appsettings.Development.json
│       └── Program.cs
│
└── tests/
    └── Architecture.Tests/            # Mono.Cecil static inspection (Single-host adapted)
        ├── Support/
        │   ├── ArchConfig.cs
        │   ├── AssemblyInfo.cs        # Disables xUnit parallel execution for Mono.Cecil
        │   ├── Baseline.cs
        │   ├── Deps.cs
        │   ├── Ns.cs
        │   └── RepoPaths.cs
        ├── Rules/
        │   ├── LayerRules.cs          # A1, A2, A3, A5, A7
        │   ├── HostRules.cs           # A5b, A6
        │   ├── SurfaceRules.cs        # A8, A11, A12, A13
        │   └── ModuleRules.cs         # A9a, A9b, A10
        └── Baseline/                  # Ratchet tracking files (*.txt)
```

---

## 3. Database Schema Specifications

### 3.1 Tenancy Schema (`tenancy`)

#### Table: `tenancy.Tenants`
| Column | Type | Constraints / Purpose |
|---|---|---|
| `Id` | `uniqueidentifier` | Primary Key |
| `Name` | `nvarchar(150)` | Organization / Company Name |
| `Identifier` | `nvarchar(100)` | Unique URL-safe slug (e.g., `acme-corp`) |
| `Status` | `int` | `Active = 1`, `Suspended = 2`, `PendingVerification = 3` |
| `Plan` | `int` | `Free = 1`, `Starter = 2`, `Professional = 3`, `Enterprise = 4` |
| `CreatedAtUtc` | `datetimeoffset` | Audit creation timestamp |
| `CreatedBy` | `uniqueidentifier?` | Creator identifier |
| `ModifiedAtUtc` | `datetimeoffset?` | Audit update timestamp |
| `ModifiedBy` | `uniqueidentifier?` | Updater identifier |

---

### 3.2 Identity Schema (`identity`)

#### Table: `identity.PlatformUsers` (Super Admin - Global Scope)
| Column | Type | Constraints / Purpose |
|---|---|---|
| `Id` | `uniqueidentifier` | Primary Key |
| `Email` | `nvarchar(256)` | Unique global login, lowercased |
| `PasswordHash` | `nvarchar(max)` | BCrypt enhanced hash |
| `FirstName` | `nvarchar(100)` | Administrator first name |
| `LastName` | `nvarchar(100)` | Administrator last name |
| `Status` | `int` | `Active = 1`, `Inactive = 2`, `Locked = 3` |
| `CreatedAtUtc` | `datetimeoffset` | Audit timestamp |

#### Table: `identity.Users` (Customer Account - Tenant Scoped)
| Column | Type | Constraints / Purpose |
|---|---|---|
| `Id` | `uniqueidentifier` | Primary Key |
| `TenantId` | `uniqueidentifier` | FK -> `tenancy.Tenants.Id` (Indexed) |
| `Email` | `nvarchar(256)` | Composite Unique Index: `(TenantId, Email)` |
| `PasswordHash` | `nvarchar(max)` | BCrypt enhanced hash |
| `FirstName` | `nvarchar(100)` | User given name |
| `LastName` | `nvarchar(100)` | User family name |
| `Status` | `int` | `Active = 1`, `Inactive = 2`, `Locked = 3` |
| `EmailConfirmed`| `bit` | Email verification flag |
| `AccessFailedCount`| `int` | Consecutive failed logins for lockout defense |
| `LockoutEndUtc` | `datetimeoffset?` | Lockout expiration timestamp |
| `LastLoginAtUtc` | `datetimeoffset?` | Dormant account auditing |
| `CreatedAtUtc` | `datetimeoffset` | Audit timestamp |
| `CreatedBy` | `uniqueidentifier?` | Assigning admin |
| `ModifiedAtUtc` | `datetimeoffset?` | Audit timestamp |
| `ModifiedBy` | `uniqueidentifier?` | Last editor |

#### Table: `identity.Permissions` (Global System Catalog)
| Column | Type | Constraints / Purpose |
|---|---|---|
| `Id` | `uniqueidentifier` | Primary Key |
| `Code` | `nvarchar(100)` | Unique code in format `{Module}.{Resource}.{Action}` |
| `Name` | `nvarchar(100)` | Display label for UI |
| `Module` | `nvarchar(50)` | Grouping category (`Identity`, `Tenancy`, `Billing`) |
| `Description` | `nvarchar(250)` | Explanation text |

#### Table: `identity.Roles` (Tenant Scoped)
| Column | Type | Constraints / Purpose |
|---|---|---|
| `Id` | `uniqueidentifier` | Primary Key |
| `TenantId` | `uniqueidentifier` | FK -> `tenancy.Tenants.Id` |
| `Name` | `nvarchar(100)` | Role title (`Admin`, `Member`, `Custom Role`) |
| `Description` | `nvarchar(250)` | Role description |
| `IsSystem` | `bit` | Immutable flag: system roles (`Admin`) cannot be deleted |
| `IsDefault` | `bit` | Automatically assigned to newly registered tenant users |
| `CreatedAtUtc` | `datetimeoffset` | Audit timestamp |
| `CreatedBy` | `uniqueidentifier?` | Creator |
| `ModifiedAtUtc` | `datetimeoffset?` | Audit timestamp |
| `ModifiedBy` | `uniqueidentifier?` | Editor |

#### Table: `identity.RolePermissions` (Join Table)
| Column | Type | Constraints / Purpose |
|---|---|---|
| `RoleId` | `uniqueidentifier` | Composite PK 1, FK -> `identity.Roles.Id` (Cascade delete) |
| `PermissionId` | `uniqueidentifier` | Composite PK 2, FK -> `identity.Permissions.Id` (Restrict delete) |

#### Table: `identity.UserRoles` (Join Table with SOC2 Audit Tracking)
| Column | Type | Constraints / Purpose |
|---|---|---|
| `UserId` | `uniqueidentifier` | Composite PK 1, FK -> `identity.Users.Id` (Cascade delete) |
| `RoleId` | `uniqueidentifier` | Composite PK 2, FK -> `identity.Roles.Id` (Cascade delete) |
| `AssignedAtUtc` | `datetimeoffset` | Exact timestamp role was granted |
| `AssignedBy` | `uniqueidentifier?` | Admin who granted the role |
| `ExpiresAtUtc` | `datetimeoffset?` | Optional expiry timestamp for time-bound/contractor access |

#### Table: `identity.UserPermissions` (Direct User Overrides)
| Column | Type | Constraints / Purpose |
|---|---|---|
| `UserId` | `uniqueidentifier` | Composite PK 1, FK -> `identity.Users.Id` (Cascade delete) |
| `PermissionId` | `uniqueidentifier` | Composite PK 2, FK -> `identity.Permissions.Id` (Restrict delete) |
| `IsGranted` | `bit` | `true` = Explicit Grant, `false` = Explicit Deny override |

#### Table: `identity.RefreshTokens` (Tenant Scoped Token Store)
| Column | Type | Constraints / Purpose |
|---|---|---|
| `Id` | `uniqueidentifier` | Primary Key |
| `TenantId` | `uniqueidentifier` | FK -> `tenancy.Tenants.Id` |
| `UserId` | `uniqueidentifier` | FK -> `identity.Users.Id` |
| `Token` | `nvarchar(200)` | Cryptographic string (Unique index) |
| `ExpiresAtUtc` | `datetimeoffset` | Absolute expiry date |
| `RevokedAtUtc` | `datetimeoffset?` | Revocation timestamp |
| `ReplacedByToken`| `nvarchar(200)?` | Detection of token reuse/replay attacks |
| `CreatedByIp` | `nvarchar(50)?` | Auditing client IP of origin |
| `CreatedAtUtc` | `datetimeoffset` | Issuance timestamp |

#### Table: `identity.PasswordResetTokens` (Self-Serve Recovery Store)
| Column | Type | Constraints / Purpose |
|---|---|---|
| `Id` | `uniqueidentifier` | Primary Key |
| `TenantId` | `uniqueidentifier` | FK -> `tenancy.Tenants.Id` |
| `UserId` | `uniqueidentifier` | FK -> `identity.Users.Id` |
| `TokenHash` | `nvarchar(256)` | Cryptographic hash of random token string (Indexed) |
| `ExpiresAtUtc` | `datetimeoffset` | Short expiration (15 to 30 minutes) |
| `UsedAtUtc` | `datetimeoffset?` | Timestamp once redeemed (prevents reuse) |
| `CreatedAtUtc` | `datetimeoffset` | Issuance timestamp |

---

## 4. Key Mechanisms and Security Workflows

### 4.1 Password Reset Workflow
1. User requests password reset via `POST /api/auth/forgot-password` with `Email`.
2. Backend looks up user in current tenant. Even if not found, it returns `200 OK` (generic success response to prevent account enumeration).
3. If found, generates a 32-byte cryptographic random token, stores `TokenHash = SHA256(token)` in `identity.PasswordResetTokens` expiring in 30 minutes, and calls `IEmailSender.SendAsync()` with reset link.
4. User submits new password with raw token via `POST /api/auth/reset-password`.
5. Backend verifies token hash, verifies `ExpiresAtUtc > UtcNow` and `UsedAtUtc == null`, sets new password hash on `User`, marks token `UsedAtUtc = UtcNow`, and revokes active refresh tokens.

### 4.2 Rate Limiting Policies
Configured in `RateLimitingExtensions.cs` using ASP.NET Core `Microsoft.AspNetCore.RateLimiting`:
- **Auth Strict Policy:** Applied to `/api/auth/login`, `/api/auth/forgot-password`, `/api/platform/auth/login`.
  - Window: 1 minute.
  - Limit: 5 requests per IP address.
  - Rejection: Returns `429 Too Many Requests` with `Retry-After` header.
- **General Policy:** Applied to authenticated API endpoints.
  - Window: 1 minute.
  - Limit: 100 requests per authenticated `UserId` (or per IP if anonymous).

### 4.3 Automatic Database Migration Runner
In `MigrationExtensions.cs` called from `Program.cs`:
```csharp
public static async Task ApplyMigrationsAsync(this WebApplication app)
{
    if (app.Environment.IsDevelopment())
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
        
        var seeder = scope.ServiceProvider.GetRequiredService<DbInitializer>();
        await seeder.InitializeAsync();
    }
}
```
Ensures that any developer cloning the project runs `dotnet run` and immediately has a migrated, seeded, ready-to-use database.

### 4.4 Permission Evaluation & Cache Optimization
1. **JWT Contents:** Access token contains only `sub` (`UserId`), `tenant_id` (`TenantId`), and `role` claims to avoid HTTP 431 header size issues.
2. **Caching:** `IPermissionCache` caches effective permission string hashsets per user using key `tenant:{tenantId}:user:{userId}:perms` with a sliding expiration of 15 minutes.
3. **Invalidation:** Modifying a role, assigning a role, or modifying direct user permissions invokes `IPermissionCache.InvalidateAsync(tenantId, userId)`.
4. **Runtime Enforcement:** `PermissionAuthorizationHandler` queries `IPermissionCache` to evaluate `[HasPermission("Module.Resource.Action")]`.

### 4.5 Super Admin Impersonation Workflow
1. Super Admin authenticates at `POST /api/platform/auth/login` and receives a platform token with claim `scope: "platform"`.
2. Super Admin calls `POST /api/platform/tenants/{tenantId}/impersonate`.
3. The API generates a 15-minute impersonation token with RFC 8693 actor claims:
   ```json
   {
     "sub": "target-tenant-user-guid",
     "tenant_id": "target-tenant-guid",
     "is_impersonated": true,
     "act": {
       "sub": "platform-admin-guid",
       "email": "superadmin@domain.com"
     }
   }
   ```
4. **Audit Interceptor:** EF Core `AuditInterceptor` inspects `ICurrentUser`. If `IsImpersonated == true`, it logs structured audit telemetry with both the target user and the super admin actor ID.

---

## 5. Execution Phases

### Phase 1: Tooling & Governance Scaffold
1. Solution and project files initialization:
   - `src/Domain/ModularSaaS.Domain.csproj`
   - `src/Application/ModularSaaS.Application.csproj`
   - `src/Infrastructure/ModularSaaS.Infrastructure.csproj`
   - `apps/Api/ModularSaaS.Api.csproj`
   - `tests/Architecture.Tests/ModularSaaS.Architecture.Tests.csproj`
2. Wire project references.
3. Configure `Directory.Build.props` (net10.0, nullable enabled, TreatWarningsAsErrors=true, analyzers).
4. Configure `Directory.Packages.props` with Central Package Management:
   - `Microsoft.EntityFrameworkCore.SqlServer`
   - `Microsoft.EntityFrameworkCore.Design`
   - `Microsoft.AspNetCore.Authentication.JwtBearer`
   - `FluentValidation.DependencyInjectionExtensions`
   - `BCrypt.Net-Next`
   - `Serilog.AspNetCore`
   - `Mono.Cecil`
   - `xunit`, `xunit.runner.visualstudio`, `Microsoft.NET.Test.Sdk`
   - `StyleCop.Analyzers`, `Microsoft.CodeAnalysis.BannedApiAnalyzers`
5. Place `BannedSymbols.txt` and `stylecop.json`.
6. Ensure placeholder `.editorconfig` is preserved for user rules.

### Phase 2: Domain Layer Implementation
1. `Domain/Shared/`: `BaseEntity`, `AuditableEntity`, `ITenantEntity`, `IAggregateRoot`, `DomainException`.
2. `Domain/Tenancy/`: `Tenant`, `Enums/TenantStatus.cs`, `Enums/TenantPlan.cs`.
3. `Domain/Identity/`:
   - `PlatformUser.cs`
   - `User.cs`
   - `Role.cs`
   - `Permission.cs`
   - `RolePermission.cs`
   - `UserRole.cs`
   - `UserPermission.cs`
   - `RefreshToken.cs`
   - `PasswordResetToken.cs`
   - `Enums/UserStatus.cs`
4. Compile `ModularSaaS.Domain` and verify 0 warnings/errors.

### Phase 3: Application Shared Kernel & Abstractions
1. `Application/Shared/Models/`: `Result`, `Result<T>`, `Error`, `ErrorType`, `ValidationError`, `PagedResult<T>`, `PageRequest`.
2. `Application/Shared/Exceptions/`: `TenantRequiredException`, `ValidationException`, `NotFoundException`, `UnauthorizedException`.
3. `Application/Shared/Abstractions/`: `IRepository<T>`, `IUnitOfWork`, `ITenantContext`, `ICurrentUser`, `IClock`, `IEmailSender`, `IPermissionCache`.

### Phase 4: Application Modules Implementation
1. **Tenancy Module:**
   - Abstractions: `ITenantService`, `ITenantRepository`.
   - Models: `CreateTenant.cs`, `GetTenants.cs`.
   - Validators: `CreateTenantValidator.cs`.
   - Implementation: `TenantService.cs` (`internal sealed`).
   - Registration: `TenancyModule.cs`.
2. **Identity Module:**
   - Abstractions: `IAuthService`, `IPlatformAuthService`, `IUserService`, `IRoleService`, `IPermissionService`, `ITokenService`, `IPasswordHasher`, `IUserRepository`, `IPlatformUserRepository`, `IRoleRepository`, `IPermissionRepository`, `IRefreshTokenRepository`, `IPasswordResetTokenRepository`.
   - Models: `Login.cs`, `PlatformLogin.cs`, `Impersonate.cs`, `Register.cs`, `RefreshToken.cs`, `ChangePassword.cs`, `ForgotPassword.cs`, `ResetPassword.cs`, `RoleModels.cs`, `PermissionModels.cs`, `UserResult.cs`.
   - Validators: `LoginValidator.cs`, `RegisterValidator.cs`, `CreateRoleValidator.cs`, `ChangePasswordValidator.cs`, `ForgotPasswordValidator.cs`, `ResetPasswordValidator.cs`.
   - Permissions Catalog: `AppPermissions.cs` with `{Module}.{Resource}.{Action}` naming.
   - Implementation: `AuthService.cs`, `PlatformAuthService.cs`, `UserService.cs`, `RoleService.cs` (`internal sealed`).
   - Registration: `IdentityModule.cs`.
3. Wire `Application/DependencyInjection.cs`.
4. Compile `ModularSaaS.Application` and verify 0 warnings/errors.

### Phase 5: Infrastructure Implementation
1. `Time/SystemClock.cs` implementing `IClock`.
2. `Security/PasswordHasher.cs` implementing `IPasswordHasher`.
3. `Security/JwtTokenService.cs` implementing `ITokenService` (with impersonation actor claim generation).
4. `Security/CurrentUser.cs` implementing `ICurrentUser`.
5. `Tenancy/TenantContext.cs` implementing `ITenantContext`.
6. `Caching/PermissionCache.cs` implementing `IPermissionCache`.
7. `Persistence/Interceptors/`: `TenantInterceptor.cs`, `AuditInterceptor.cs`.
8. `Persistence/Configurations/`: Entity type configurations for all 9 entities.
9. `Persistence/AppDbContext.cs` with `ITenantEntity` dynamic query filters.
10. `Persistence/Repositories/`: All specialized repository implementations.
11. `Persistence/UnitOfWork.cs`.
12. `Persistence/Seed/DbInitializer.cs`: Seeds standard permissions catalog, default roles (`Owner`, `Admin`, `Member`), and root platform super-admin account.
13. `Infrastructure/DependencyInjection.cs`.
14. Compile `ModularSaaS.Infrastructure` and verify 0 warnings/errors.

### Phase 6: Web API Host Implementation
1. `Security/HasPermissionAttribute.cs` & `PermissionAuthorizationHandler.cs`.
2. `Middleware/ExceptionHandlingMiddleware.cs`.
3. `Middleware/TenantResolutionMiddleware.cs`.
4. `Contracts/`: Request and Response records per endpoint.
5. `Controllers/`:
   - `BaseApiController.cs`
   - `AuthController.cs` (login, refresh, revoke, forgot-password, reset-password)
   - `UsersController.cs`
   - `RolesController.cs`
   - `PermissionsController.cs`
   - `Platform/PlatformAuthController.cs`
   - `Platform/PlatformTenantsController.cs`
6. `Extensions/`:
   - `MigrationExtensions.cs` (automatic migration apply in dev)
   - `RateLimitingExtensions.cs` (strict auth endpoint limiters)
   - `SerilogExtensions.cs`
   - `SwaggerExtensions.cs`
7. `appsettings.json` and `appsettings.Development.json`.
8. `Program.cs` wiring pipeline and executing `app.ApplyMigrationsAsync()`.
9. Compile `ModularSaaS.Api` and verify 0 warnings/errors.

### Phase 7: Architecture Tests & Verification Gate
1. Port Mono.Cecil scanner into `ModularSaaS.Architecture.Tests`:
   - `Support/Deps.cs`, `Support/Ns.cs`, `Support/ArchConfig.cs`, `Support/Baseline.cs`, `Support/RepoPaths.cs`, `Support/AssemblyInfo.cs`.
2. Configure `ArchConfig.cs` for `ModularSaaS` root namespace and single `ModularSaaS.Api` host.
3. Implement `Rules/`: `LayerRules.cs`, `HostRules.cs`, `SurfaceRules.cs`, `ModuleRules.cs`.
4. Run `dotnet test tests/Architecture.Tests`. All tests must pass with 0 baseline bypasses.
5. Create `README.md` at solution root documenting setup, seeding, and architecture.

---

## 6. Verification Gate (Checklist to Complete Phase)
- [ ] `dotnet build ModularSaaS.sln` completes with 0 errors and 0 warnings.
- [ ] `dotnet test tests/Architecture.Tests` runs and passes 100% of rules.
- [ ] `dotnet run --project apps/Api/ModularSaaS.Api.csproj` automatically applies migrations and seeds the database.
- [ ] API launches with Swagger at `/swagger`.
- [ ] Health check endpoint responds with 200 OK at `/health`.
- [ ] Rate limiting rejects more than 5 requests per minute to `/api/auth/login` with 429 Too Many Requests.
- [ ] Password reset flow: `forgot-password` generates token, `reset-password` updates password and revokes previous refresh tokens.
- [ ] Platform Super Admin can log in at `/api/platform/auth/login` and provision tenants.
- [ ] Platform Super Admin can impersonate a tenant user at `/api/platform/tenants/{id}/impersonate` with actor claims.
- [ ] Tenant Admin can log in, create roles, assign permissions, and register users.
- [ ] Direct user permissions (grants and denies) correctly override role permissions.
- [ ] Tenant resolution operates fail-closed: accessing tenant-scoped endpoints without resolved tenant rejects with 400 Bad Request.
