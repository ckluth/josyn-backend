# josyn-backend

> **Stub — placeholder for the JOSYN backend scheduler integration layer.**
> Full architectural context: [josyn-platform](../josyn-platform/README.md)
> `JOSYN.Jap.JAPServer` was relocated here from `josyn-jap` per ADR-004.

---

## What this will be

The scheduling and session-orchestration layer that bridges the legacy job system with the JOSYN platform.

Its `SessionStarter` component is the **migration rendezvous**: instead of spawning the old `JobHost.exe`, it will spawn `JAPServer.exe JOSYN-IPC <guid>`, which in turn coordinates with the job executable via named-pipe IPC.

See [josyn-platform/repos/josyn-backend.md](../josyn-platform/repos/josyn-backend.md) for the full architectural description, migration narrative, and planned component list.

---

## Current state

`JOSYN.Backend.SessionStarter` is a compilable stub; all methods return `Result.Error("Noch nicht implementiert.")`.
`JOSYN.Jap.JAPServer` is a working PoC EXE — relocated from `josyn-jap` per ADR-004.

```
josyn-backend/
├── JOSYN.Backend.SessionStarter.slnx      ← library solution
├── JOSYN.Backend.SessionStarter/
│   ├── Contracts/
│   │   └── ISessionStarter.cs             ← static abstract contract
│   ├── SessionStarter.cs                  ← stub implementation
│   └── StartSessionRequest.cs
│
├── JOSYN.Jap.JAPServer.slnx               ← EXE solution (relocated from josyn-jap)
├── JOSYN.Jap.JAPServer/
│   ├── Host.cs
│   ├── JAPServer.cs
│   ├── Program.cs
│   ├── icon.ico
│   ├── Properties/launchSettings.json
│   └── .local-build/                      ← project-local build + launch scripts
│
└── .local-build/
    ├── build.cmd                           ← builds ALL solutions
    └── pack.cmd                            ← packs SessionStarter only
```

---

## Build

```
.local-build\build.cmd [Release|Debug]     # builds SessionStarter + JAPServer
.local-build\pack.cmd                      # packs SessionStarter to ..\local-packages\
```

JAPServer only:

```
JOSYN.Jap.JAPServer\.local-build\build.cmd [Release|Debug]
```

License: MIT | Company: HAEVG AG | Target: net10.0
