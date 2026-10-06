# Clean Code Rules for C# and .NET

### 1. Async Signatures
- Never write `async void` (except UI event handlers). It cannot be awaited, and unhandled exceptions crash the process.
- Suffix async methods with `Async`.
- Accept a `CancellationToken` in async methods and pass it down.

```csharp
// Before
public async void ProcessOrder(Guid orderId) { }

// After
public async Task ProcessOrderAsync(Guid orderId, CancellationToken cancellationToken = default) { }
```

### 2. Return Most Specific Useful Type
- `IEnumerable<T>` on in-memory results invites multiple enumeration and hides whether the source was materialized.
- Return `IReadOnlyList<T>` (or `IReadOnlyCollection<T>`) when data is materialized in memory.
- Keep `IEnumerable<T>` or `IAsyncEnumerable<T>` only for deferred execution streaming.

```csharp
// Before
public IEnumerable<Tenant> GetActiveTenants() => _tenants.Where(t => t.IsActive);

// After
public IReadOnlyList<Tenant> GetActiveTenants() => _tenants.Where(t => t.IsActive).ToList();
```

### 3. Never Pass Boolean Flags as Parameters
- Boolean parameters make call sites ambiguous (e.g., `SendAsync(order, true)`).
- A boolean flag usually means the method is doing two different jobs. Split into two methods or use a descriptive enum.

```csharp
// Before
await SendAsync(order, isDraft: true);

// After
await SendDraftAsync(order);
// or
await SendAsync(order, DispatchMode.Draft);
```

### 4. Avoid Manager, Helper, and Utils Class Names
- Classes named `Manager`, `Helper`, or `Utils` convey no concrete responsibility and become dumping grounds.
- Name the class after its specific domain action or responsibility, or use focused extension methods.

```csharp
// Before
public static class UserHelper { ... }

// After
public sealed class PasswordPolicyEvaluator { ... }
```

### 5. Delete #region Blocks
- `#region` hides code smells and excessive class complexity rather than fixing them.
- If a class requires regions to be readable, extract cohesive responsibilities into separate classes.

### 6. Do Not Extract Single-Implementation Interfaces Solely for DI
- .NET dependency injection resolves and injects concrete classes natively.
- A 1-to-1 interface adds indirection without benefit until multiple implementations exist, a test stub is required, or a cross-module boundary is crossed.

```csharp
// Before
builder.Services.AddScoped<ITokenGenerator, TokenGenerator>();

// After (when only one implementation exists and no external contract is needed)
builder.Services.AddScoped<TokenGenerator>();
```

### 7. Prefer Composition Over Inheritance
- Inheritance couples classes tightly to base class hierarchies and constructor chaining.
- Prefer composition and the Decorator pattern to add behavior dynamically without rigid inheritance chains.

### 8. Readability and Control Flow
- Use guard clauses and return early to avoid deep nesting (`if (...) return;`).
- Replace magic numbers and string literals with named constants or domain objects.
