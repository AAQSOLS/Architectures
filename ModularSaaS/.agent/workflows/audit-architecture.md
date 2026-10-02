---
description: Read-only audit of the current codebase against docs/ARCHITECTURE.md (Phase 1). Produces docs/migration/AUDIT.md
---

DO NOT modify any source file in this workflow. Output only `docs/migration/AUDIT.md`.

1. Read `docs/ARCHITECTURE.md` fully.
2. Inventory: list every project (csproj), its references, target framework, and folder layout.
3. Module map: identify the business modules present (Catalog, Inventory, Sales, ...). For each, list where its entities, DTOs/models, services, repositories, controllers, views, SPs, and EF configurations currently live (paths).
4. Violations: for each rule in ARCHITECTURE.md §0, §4, §5, §6, §7, §8 list concrete violations with file paths and counts. Include at least:
   - controllers/views using DbContext, entities, repositories, or Infrastructure types
   - entities or value objects exposed by service interfaces or DTOs
   - cross-module dependencies (repository/entity/navigation-property use)
   - DTOs shared between Client.Api and Admin.Mvc
   - SPs missing `@TenantId`, tenant entities missing `ITenantEntity`, nullable-tenant shortcuts for platform data
   - generic repositories exposing IQueryable, repositories used for reads
   - mediator/event/bus usage
   - god services (more than ~15 public methods)
   - `DateTime.Now/UtcNow` usage counts
5. Mapping table: current path -> target path for every module (grouped, not per file).
6. Dependency graph between modules (who calls whom) and any cycles.
7. Recommended migration order (leaf modules first) with risk and size (files/LOC) per module.
8. Open questions: everything ambiguous that needs a human decision.

Finish by summarizing the top 10 risks. Stop and wait for human review.
