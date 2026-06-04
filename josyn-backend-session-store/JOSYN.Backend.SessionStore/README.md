# JOSYN.Backend.SessionStore

Persistenz-Schicht für JOSYN Job-Sessions. Teil des JOSYN Storage Realm (ADR-007).

Stellt `ISessionStore` und `IJobSessionRecord` als stabile Kontrakte bereit.
Die Produktionsimplementierung (`SessionStore`) verwendet EF Core gegen SQL Server
(Schema `josyn`, Tabelle `SessionStore`).

## Verwendung

```csharp
ISessionStore store = new SessionStore(connectionString);

// Session anlegen
var session = new JobSessionRecord
{
    UID         = Guid.NewGuid(),
    JobTypeName = "MyJob",
    Arguments   = "<ini>",
    Result      = string.Empty
};
store.SaveNewSession(session);

// Session lesen
var get = store.GetSession(session.UID);

// Session aktualisieren
store.UpdateSession(session with { Result = "<result>" });
```

## Datenbank

Tabelle: `josyn.SessionStore`
DDL: `josyn-backend/db/migrations/V001__session_store.sql`
Dev-Bootstrap: `josyn-backend/db/bootstrap-local-dev.sql`

## Abhängigkeiten

- `JOSYN.Foundation.ResultPattern`
- `Microsoft.EntityFrameworkCore.SqlServer`
