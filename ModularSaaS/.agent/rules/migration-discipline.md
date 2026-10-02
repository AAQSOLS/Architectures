---
trigger: always_on
description: Rules for restructuring the existing codebase safely
---

# Migration discipline (existing code is being moved to the target structure)

- PRESERVE BEHAVIOR. A structural step changes locations, namespaces, types, and dependencies, never logic. Bugs you notice go in `docs/migration/FOLLOWUPS.md`, not into the diff.
- ATOMIC STEPS. One module, one layer (or one host slice) per step. The solution must build and tests must pass at the end of every step. Never leave the tree red between steps.
- MOVE, DON'T REWRITE. Use `git mv` so history is kept. Change namespaces to match folders. Keep method bodies unchanged unless the target rule requires it (e.g. removing an entity from a public signature).
- ONE MODULE AT A TIME, in the order in `docs/migration/MIGRATION_PLAN.md`. Do not touch a module that is not the current task.
- NO DRIVE-BY CHANGES: no formatting sweeps, package upgrades, renames unrelated to the step.
- DATABASE: do not change schema, table names, or SP behavior during structural steps. After namespace/type moves, `dotnet ef migrations has-pending-model-changes` must report none (or only a snapshot-only change with an empty `Up`/`Down`). Schema-per-module moves and adding `@TenantId` to old SPs are separate, explicitly planned steps.
- ARCHITECTURE TESTS have a baseline allow-list (`tests/Architecture.Tests/Baseline/*.txt`). You may only REMOVE entries from it. Never add an entry to make a test pass.
- UPDATE `docs/migration/STATUS.md` at the end of each step: what moved, what remains, any decision needed.
- STOP AND ASK when: a type could belong to two modules, a dependency cycle between modules appears, a public model would need an entity, behavior would have to change, or a step needs more than about 40 files touched (propose a split instead).
- COMMIT after each green step with message `refactor(<module>): <what moved>`.
