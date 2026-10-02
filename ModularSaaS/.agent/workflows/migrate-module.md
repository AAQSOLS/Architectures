---
description: Migrate ONE module to the target structure. Usage: /migrate-module <ModuleName>
---

Precondition: `docs/migration/AUDIT.md` is approved and `docs/migration/STATUS.md` lists <ModuleName> as next. Read ARCHITECTURE.md §4-§9 and the AUDIT entry for this module. Write a short plan (files to move, types to split, decisions needed) and STOP for approval before step 1.

Run each step, then build + tests, then commit. Stop on any failure.

1. DOMAIN: move entities, value objects, enums, domain exceptions/services to `src/Domain/<Module>/` (one type per file; enums one per file in `Enums/`; namespaces = folders). No behavior change. Make setters private only if callers are not broken; otherwise record a follow-up.
2. PERSISTENCE MAPPING: move EF configurations to `Persistence/Configurations/<Module>/` (one per entity). Run `has-pending-model-changes`.
3. APPLICATION ABSTRACTIONS: create `Abstractions/` (service, repository, reader interfaces). Rename any "Queries" abstractions to `IXxxReader`. Repositories only per aggregate root.
4. APPLICATION MODELS: create use-case files in `Models/` (`XxxInput`, `XxxResult`, `XxxListItem`, `XxxFilter` as `sealed record`, `required` + `init`). Remove entities/value objects from public signatures. Old shared DTOs are split into these; delete them once unreferenced.
5. APPLICATION SERVICES: move services to `Services/` as `internal sealed`; split god services by use case; validators to `Validators/` (one file per use case); entity -> result mappings to `Mapping/<Module>Mappings.cs`; add `<Module>Module.cs` and register it in `AddApplication()`.
6. INFRASTRUCTURE: repositories to `Repositories/<Module>/`; read-side implementations to `Readers/<Module>/` (SP via Dapper or EF projection, using SP-name constants from `SpNames/<Module>Sps.cs`); SP scripts to `database/StoredProcedures/<Module>/`. Do not edit SP logic in this step; list SPs missing `@TenantId` in FOLLOWUPS.md.
7. HOSTS: for each of Client.Api and Admin.Mvc: create contracts / view models per use case or screen, host `Mapping/<Module>Mappings.cs`, update controllers to use only Application abstractions and models. Remove references to entities, repositories, DbContext.
8. TESTS: ensure architecture tests pass for this module and remove this module's entries from the baseline allow-list. Add/adjust tests for moved code.
9. STATUS: update `docs/migration/STATUS.md`; list follow-ups. Stop and report.
