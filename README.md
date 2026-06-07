# josyn-backend

> **Scheduler and session-orchestration layer for the JOSYN platform.**
> Full architectural context: [josyn-platform/repos/josyn-backend](../josyn-platform/repos/josyn-backend/overview.md)
> `JOSYN.Jap.JAPServer` was relocated here from `josyn-jap` per ADR-004.

---

## What this is

The scheduling and session-orchestration layer that bridges the legacy job system with the JOSYN platform.
`SessionStarter` is the **migration rendezvous**: instead of spawning the old `JobHost.exe`, it spawns
`JAPServer.exe JOSYN-IPC <guid>`, which in turn coordinates with the job executable via named-pipe IPC.

---

## Current state

All core backend packages are implemented and working:

| Package | Status |
|---------|--------|
| `JOSYN.Backend.BootstrapConfig` | ✅ `IBootstrapConfig` + `FileBootstrapConfig` (reads `josyn.bootstrap.ini`) |
| `JOSYN.Backend.SessionStore` | ✅ EF Core, SQL Server, `josyn` schema |
| `JOSYN.Backend.SessionStarter` | ✅ Allocates GUID, persists session, spawns `JAPServer.exe` |
| `JOSYN.Backend.JobRegistry` | ✅ `IJobRegistry`, `josyn.JobRegistrations` table |
| `JOSYN.Backend.ErrorHandler` | ✅ `IErrorHandler`, `SqlErrorHandler`, `josyn.ErrorStore` |
| `JOSYN.Jap.JAPServer` (EXE) | ✅ Fully wired — reads args and writes result to `SessionStore` |

For the full component description, directory structure, dependencies, and sanity notes see
[josyn-platform/repos/josyn-backend/overview.md](../josyn-platform/repos/josyn-backend/overview.md).

---

## Build

```
.local-build\build.cmd [Release|Debug]     # builds ALL solutions in the repo
.local-build\pack.cmd                      # packs all NuGet packages to ..\local-packages\
```

License: MIT | Company: HAEVG AG | Target: net10.0
