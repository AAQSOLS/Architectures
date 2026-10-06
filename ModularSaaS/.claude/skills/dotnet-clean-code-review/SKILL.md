---
name: dotnet-clean-code-review
description: Review C# and .NET code for clean code problems - Manager/Helper/Utils class names, #region blocks, boolean flag parameters, weak return types, async naming and CancellationToken, single-implementation interfaces, and deep inheritance. Use when asked to "review my changes", "do a clean code review", "check this class", "is this code clean", or wants naming and readability feedback on C# code. Do NOT use for correctness bugs or security issues. Do NOT use when writing new code.
---

# .NET Clean Code Review Skill

Reviews C# and .NET code changes against clean code and design standards. Run this review in an independent session or subagent so the review is completely unburdened by earlier implementation context.

## Workflow

### Step 1. Decide the Scope
Determine which files were changed:
- Staged/unstaged changes: `git status --short` and `git diff HEAD`
- PR/branch review: `git diff main...HEAD`
- Specific file/class review: inspect requested file directly

### Step 2. Run Mechanical Detection
Execute the mechanical regex search patterns in `references/detection-commands.md`:
```bash
rg -n --glob "*.cs" "\basync\s+void\b"
rg -n --glob "*.cs" "^\s*#region"
rg -n --glob "*.cs" -e "\b(class|record|struct|interface)\s+[A-Za-z0-9_]*(Manager|Helper|Helpers|Utils|Utility|Utilities)\b"
rg -n --glob "*.cs" -e "\b(public|internal)\s+[A-Za-z0-9_<>, ]+\s+[A-Za-z0-9_]+\s*\([^)]*\bbool\s+[a-zA-Z0-9_]+"
```

### Step 3. Semantic Judgement Pass
Evaluate each rule from `references/clean-code-rules.md`:
1. **Async signatures**: No `async void`, suffix with `Async`, accept `CancellationToken`.
2. **Specific return types**: Return `IReadOnlyList<T>` instead of `IEnumerable<T>` for materialized collections.
3. **Boolean flag parameters**: Disallow flag parameters in method signatures.
4. **Manager / Helper / Utils names**: Flag ambiguous type naming.
5. **#region blocks**: Flag `#region` tags hiding bloat.
6. **Single-implementation interfaces**: Flag 1-to-1 interfaces created purely for DI without abstraction need.
7. **Composition over inheritance**: Flag deep inheritance where decorators or composition are cleaner.
8. **Readability**: Check for deep nesting, missing guard clauses, and magic literals.

### Step 4. Write the Structured Report
Output a checklist table where every rule is listed with either `PASS` or specific findings:

| Rule | Result | Details |
| --- | --- | --- |
| 1. async void / Async suffix / CancellationToken | PASS / Finding | ... |
| 2. Most specific return type | PASS / Finding | ... |
| 3. Boolean flag parameters | PASS / Finding | ... |
| 4. Manager / Helper / Utils names | PASS / Finding | ... |
| 5. #region blocks | PASS / Finding | ... |
| 6. Single-implementation interfaces | PASS / Finding | ... |
| 7. Composition over inheritance | PASS / Finding | ... |
| 8. Readability (nesting, early returns, magic values) | PASS / Finding | ... |
