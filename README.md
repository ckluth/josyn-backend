# josyn-backend

> **Stub — placeholder for the JOSYN backend scheduler integration layer.**
> Full architectural context: [josyn-platform](../josyn-platform/README.md)

---

## What this will be

The scheduling and session-orchestration layer that bridges the legacy job system with the JOSYN platform.

Its `SessionStarter` component is the **migration rendezvous**: instead of spawning the old `JobHost.exe`, it will spawn `JAPServer.exe JOSYN-IPC <guid>`, which in turn coordinates with the job executable via named-pipe IPC.

See [josyn-platform/repos/josyn-backend.md](../josyn-platform/repos/josyn-backend.md) for the full architectural description, migration narrative, and planned component list.

---

## Current state — stub

Only `JOSYN.Backend.SessionStarter` exists as a compilable stub.
All methods return `Result.Error("Noch nicht implementiert.")`.

```
josyn-backend/
└── JOSYN.Backend.SessionStarter/
    ├── Contracts/
    │   └── ISessionStarter.cs     ← static abstract contract
    ├── SessionStarter.cs          ← stub implementation
    └── StartSessionRequest.cs     ← session start parameters
```

---

## Build

```
.local-build\build.cmd [Release|Debug]
.local-build\pack.cmd
```

License: MIT | Company: HAEVG AG | Target: net10.0
