# JOSYN Database Schema Reference

**Database:** `josyn-db-local` (dev) / `josyn` schema
**Engine:** SQL Server
**Authoritative source:** `bootstrap-local-dev.sql` (PoC phase — drop-and-recreate)

This document is the human-readable mirror of the bootstrap script.
Update both together whenever the schema changes.

---

## Tables

### `josyn.SessionStore`

Stores one row per executed job session.

| Column | Type | Nullable | Notes |
|--------|------|----------|-------|
| `Id` | `INT IDENTITY` | NO | PK (clustered) |
| `UID` | `UNIQUEIDENTIFIER` | NO | Public session identifier |
| `JobTypeName` | `NVARCHAR(256)` | NO | FK → `josyn.JobRegistry.Name` |
| `Arguments` | `NVARCHAR(MAX)` | NO | INI-serialized argument payload (base64) |
| `Result` | `NVARCHAR(MAX)` | NO | Job result payload |
| `JobVersion` | `VARCHAR(24)` | NO | |
| `UserName` | `VARCHAR(64)` | NO | Caller OS user |
| `UserDomain` | `VARCHAR(32)` | NO | Caller OS domain |
| `ClientApplication` | `VARCHAR(128)` | NO | Orchestrator name |
| `ClientMachine` | `VARCHAR(64)` | NO | |
| `TecUser` | `VARCHAR(64)` | YES | Technical user used for impersonation |
| `Started` | `DATETIME2` | NO | |
| `ExecutionStatus` | `VARCHAR(32)` | NO | See ADR-016 for valid values |
| `Progress` | `VARCHAR(512)` | YES | |
| `Finished` | `DATETIME2` | YES | NULL while session is active |
| `JapServerProcessId` | `INT` | NO | Default 0 |
| `JobHostProcessId` | `INT` | NO | Default 0 |
| `LastWriteTime` | `DATETIME2` | YES | |
| `Host` | `VARCHAR(64)` | YES | Machine that ran the session |

---

### `josyn.JobRegistry`

One row per registered job. The platform-wide master list of known jobs.

| Column | Type | Nullable | Notes |
|--------|------|----------|-------|
| `Id` | `INT IDENTITY` | NO | PK (clustered) |
| `Name` | `NVARCHAR(256)` | NO | Unique; natural key referenced by all other tables |
| `TechnicalUserName` | `NVARCHAR(256)` | NO | OS identity for process impersonation |

**ADR:** ADR-007B-02

---

### `josyn.ArgumentRecords`

Named INI argument payloads per job. 0-to-n records per job.
`"default"` is the conventional name for the single-payload case.

| Column | Type | Nullable | Notes |
|--------|------|----------|-------|
| `JobName` | `NVARCHAR(200)` | NO | PK (part 1); FK → `josyn.JobRegistry.Name` |
| `Name` | `NVARCHAR(200)` | NO | PK (part 2); operator-chosen, e.g. `"default"` |
| `Content` | `NVARCHAR(MAX)` | NO | INI-serialized argument payload, stored verbatim |

**ADR:** ADR-028

---

### `josyn.ErrorStore`

Archival error records. No FK to other tables — error records must outlive
the sessions and jobs they reference.

| Column | Type | Nullable | Notes |
|--------|------|----------|-------|
| `Id` | `INT IDENTITY` | NO | PK (clustered) |
| `UID` | `UNIQUEIDENTIFIER` | NO | Unique; default `NEWSEQUENTIALID()` |
| `OccurredAt` | `DATETIMEOFFSET` | NO | Indexed (DESC) |
| `Causer` | `NVARCHAR(256)` | NO | Component that reported the error |
| `Message` | `NVARCHAR(MAX)` | NO | |
| `CallStack` | `NVARCHAR(MAX)` | YES | |
| `ExceptionDetails` | `NVARCHAR(MAX)` | YES | |
| `JobName` | `NVARCHAR(256)` | YES | Indexed (sparse) |
| `SessionGuid` | `UNIQUEIDENTIFIER` | YES | Indexed (sparse) |

**ADR:** ADR-011B-01

---

### `josyn.ConfigStore`

Runtime configuration key-value pairs. Built-in implementation of `IConfigSource`.
Can be replaced by a company-specific adapter (ADR-009).

| Column | Type | Nullable | Notes |
|--------|------|----------|-------|
| `Id` | `INT IDENTITY` | NO | PK (clustered) |
| `Key` | `NVARCHAR(256)` | NO | Unique |
| `Value` | `NVARCHAR(MAX)` | NO | Arbitrary string — connection strings, paths, flags |

**ADR:** ADR-006B-02

---

### `josyn.JobSchedules`

Per-job scheduling header. One row per job that has any time-based schedule.
Jobs with no row here are never launched by `TimeScheduler`.

| Column | Type | Nullable | Notes |
|--------|------|----------|-------|
| `JobName` | `NVARCHAR(256)` | NO | PK; FK → `josyn.JobRegistry.Name` |
| `Suspended` | `BIT` | NO | Default `0`; suppresses all entries when `1` |
| `SuspendedUntil` | `DATE` | YES | Auto-lift date; only meaningful when `Suspended = 1` |

**ADR:** ADR-027

---

### `josyn.JobScheduleEntries`

One row per independently scheduled invocation of a job.
Each entry pairs a `ScheduleDefinition` (ADR-026 JSONC) with a named argument record.

| Column | Type | Nullable | Notes |
|--------|------|----------|-------|
| `JobName` | `NVARCHAR(256)` | NO | PK (part 1); FK → `josyn.JobSchedules.JobName` |
| `ArgumentRecordName` | `NVARCHAR(128)` | NO | PK (part 2); FK → `josyn.ArgumentRecords.Name` (same `JobName`) |
| `ScheduleDefinition` | `NVARCHAR(MAX)` | NO | ADR-026 JSONC text, stored verbatim |
| `ToleranceMinutes` | `INT` | YES | Override T for this entry. `NULL` = platform default (1 min). See ADR-029 §1. |

**ADR:** ADR-027, ADR-029

---

### `josyn.FiredSlots`

Deduplication log for the `TimeScheduler` tolerance-window algorithm.
One row per handled scheduled slot. Written *before* each session launch (at-most-once semantics).
Pruned on every `TimeScheduler` invocation; rows older than 1 day + 10 minutes are removed.

| Column | Type | Nullable | Notes |
|--------|------|----------|-------|
| `JobName` | `NVARCHAR(256)` | NO | PK (part 1) |
| `ArgumentRecordName` | `NVARCHAR(128)` | NO | PK (part 2) |
| `SlotTime` | `DATETIME2` | NO | PK (part 3); canonical scheduled fire time S |
| `FiredAt` | `DATETIME2` | NO | Actual `now` of the tick that handled this slot |

No FK constraints — this is a rolling operational log; rows are pruned, not cascaded.

**ADR:** ADR-029

---

## Foreign key map

```
josyn.JobRegistry.Name
    ← josyn.SessionStore.JobTypeName
    ← josyn.ArgumentRecords.JobName
    ← josyn.JobSchedules.JobName

josyn.JobSchedules.JobName
    ← josyn.JobScheduleEntries.JobName

josyn.ArgumentRecords.(JobName, Name)
    ← josyn.JobScheduleEntries.(JobName, ArgumentRecordName)  [logical — not enforced as composite FK]

josyn.FiredSlots
    — no FK constraints; rolling deduplication log, pruned on each TimeScheduler invocation
```

> The FK from `JobScheduleEntries` to `ArgumentRecords` is logical only. SQL Server does not
> support composite FKs across tables where the referenced columns span multiple rows.
> Referential integrity between `JobScheduleEntries.ArgumentRecordName` and
> `josyn.ArgumentRecords.Name` is enforced at the application layer (`IJobRegistry.GetArgument`
> returns `Result.Error` if the record does not exist).

---

## Dev seed data

Inserted by `bootstrap-local-dev.sql` for immediate local usability:

| Table | Row |
|-------|-----|
| `josyn.JobRegistry` | `Contoso.DemoProduct.DemoJob` |
| `josyn.ArgumentRecords` | `(Contoso.DemoProduct.DemoJob, "default")` — minimal placeholder INI |
| `josyn.ConfigStore` | `RuntimeEnvironment = DEV` |

---

## Future

Schema versioning and migration strategy (Flyway, DbUp, or equivalent) to be defined
when the first shared persistent environment is established. See ROADMAP.md.
