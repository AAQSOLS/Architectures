# Detection Commands for .NET Clean Code Review

Run these commands using `rg` across the changed files or git diff:

```bash
# 1. Detect async void methods (anti-pattern: untracked background execution, process crash on error)
rg -n --glob "*.cs" "\basync\s+void\b"

# 2. Detect #region blocks (anti-pattern: hides bloated classes instead of refactoring)
rg -n --glob "*.cs" "^\s*#region"

# 3. Detect Manager, Helper, Utils naming (anti-pattern: ambiguous responsibility dumping ground)
rg -n --glob "*.cs" -e "\b(class|record|struct|interface)\s+[A-Za-z0-9_]*(Manager|Helper|Helpers|Utils|Utility|Utilities)\b"

# 4. Detect boolean flag parameters in method signatures (anti-pattern: flags doing two different jobs)
rg -n --glob "*.cs" -e "\b(public|internal)\s+[A-Za-z0-9_<>, ]+\s+[A-Za-z0-9_]+\s*\([^)]*\bbool\s+[a-zA-Z0-9_]+"
```
