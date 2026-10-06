---
description: Review C# and .NET code changes against clean code rules and design standards. Produces a structured clean code evaluation report.
---

1. Determine the scope of changed files:
   - Staged or unstaged working tree: `git status --short` and `git diff HEAD`
   - Branch or PR review: `git diff main...HEAD`
   - Explicit target: inspect specified files directly

2. Execute mechanical detection scans:
   - `rg -n --glob "*.cs" "\basync\s+void\b"`
   - `rg -n --glob "*.cs" "^\s*#region"`
   - `rg -n --glob "*.cs" -e "\b(class|record|struct|interface)\s+[A-Za-z0-9_]*(Manager|Helper|Helpers|Utils|Utility|Utilities)\b"`
   - `rg -n --glob "*.cs" -e "\b(public|internal)\s+[A-Za-z0-9_<>, ]+\s+[A-Za-z0-9_]+\s*\([^)]*\bbool\s+[a-zA-Z0-9_]+"`

3. Evaluate the 8 clean code rules against the scope:
   - Rule 1: Async signatures (no `async void`, method names end with `Async`, accept `CancellationToken`).
   - Rule 2: Most specific return type (`IReadOnlyList<T>` instead of `IEnumerable<T>` for materialized collections).
   - Rule 3: Boolean flag parameters (split into explicit methods or option objects).
   - Rule 4: Manager/Helper/Utils names (rename to domain-specific responsibilities).
   - Rule 5: `#region` blocks (remove regions and refactor bloated classes).
   - Rule 6: Single-implementation interfaces (avoid 1-to-1 interfaces without polymorphic or testing purpose).
   - Rule 7: Composition over inheritance (favor composition/decorators over deep inheritance hierarchies).
   - Rule 8: Readability (guard clauses, early returns, no magic literals or deep nesting).

4. Output the structured review table:
   | Rule | Result | Details |
   | --- | --- | --- |
   | 1. async void / Async suffix / CancellationToken | PASS / Finding | ... |
   | 2. Most specific return type | PASS / Finding | ... |
   | 3. Boolean flag parameters | PASS / Finding | ... |
   | 4. Manager / Helper / Utils names | PASS / Finding | ... |
   | 5. #region blocks | PASS / Finding | ... |
   | 6. Single-implementation interfaces | PASS / Finding | ... |
   | 7. Composition over inheritance | PASS / Finding | ... |
   | 8. Readability | PASS / Finding | ... |
