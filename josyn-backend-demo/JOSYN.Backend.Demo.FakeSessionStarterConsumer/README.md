# JOSYN.Backend.Demo.FakeSessionStarterConsumer

PoC demo console application. Demonstrates the full session round-trip:

1. Wires `HardcodedGlobalConfig`, `SessionStore`, and `SessionStarter`.
2. Calls `StartSession("DemoJob", <INI args>)` — persists the session and spawns `JAPServer.exe`.
3. Polls the `SessionStore` every 500 ms (up to 30 s) until `JAPServer` writes the result back.
4. Prints the result to the console.

## Prerequisites

- SQL Server reachable at the connection string in `HardcodedGlobalConfig`
  (`Server=localhost;Database=JobSystem;Integrated Security=true;TrustServerCertificate=true;`)
- `JOSYN.Jap.JAPServer.exe` built and present at the path in `HardcodedGlobalConfig`
  (`C:\Temp\VS.OUT\JOSYN\JOSYN.Jap.JAPServer\bin\Release\JOSYN.Jap.JAPServer.exe`)
- The `JobSessions` table created by the `SessionStore` EF Core migration

## Known PoC limitations

- `HardcodedGlobalConfig` uses compile-time developer-machine paths. Replace before any
  production or CI use.
- Poll-based result detection. A proper implementation would use a push signal (e.g., a
  notification queue or status column).
- If JAPServer crashes after the 500 ms spawn-check window, the session row remains with
  an empty `Result` and the consumer will time out.
