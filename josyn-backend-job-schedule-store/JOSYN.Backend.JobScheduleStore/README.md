# JOSYN.Backend.JobScheduleStore

Time-based scheduling configuration per registered job (ADR-027).

Provides `IJobScheduleStore` — consumed by `TimeScheduler.exe` to retrieve all active
job schedules and their inline `ScheduleDefinition` payloads (ADR-026 JSONC) at each
evaluation tick.
