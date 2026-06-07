# JOSYN.Backend.BootstrapConfig

**Bootstrap configuration contract for JOSYN backend components.**

Provides `IBootstrapConfig` — the stable interface that backend components use to obtain
connection strings and runtime paths. `FileBootstrapConfig` is the built-in implementation
that reads from `josyn.bootstrap.ini` in the backend root directory.

## Known limitation

`FileBootstrapConfig` reads from a flat INI file with no encryption. It is intentionally
a minimal bootstrap mechanism — a more sophisticated implementation (e.g. adapter-based)
can replace it via the ADR-009 adapter mechanism without changing any consumer code.

## Usage

```csharp
var result = FileBootstrapConfig.Load(Path.Combine(backendRoot, FileBootstrapConfig.FileName));
if (!result.Succeeded) { /* abort startup */ }
IBootstrapConfig config = result.Value;
var store = new SessionStore(config.SessionStoreConnectionString);
```

## Dependencies

| Package | Role |
|---------|------|
| `JOSYN.Foundation.PropertyBag` | INI deserialization |
| `JOSYN.Foundation.ResultPattern` | Error-as-value result type |

---

MIT License — Copyright © 2026 HAEVG AG
