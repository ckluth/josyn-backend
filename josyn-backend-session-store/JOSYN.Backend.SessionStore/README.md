# JOSYN.Backend.SessionStore

Persistenz-Schicht für JOSYN Job-Sessions.

Stellt `ISessionStore` und `IJobSession` als stabile Kontrakte bereit.
Die Produktionsimplementierung (`SessionStore`) verwendet EF Core gegen SQL Server
(Schema `josyn`, Tabelle `SessionStore`).

## Verwendung

```csharp
ISessionStore store = new SessionStore(connectionString);

// Session anlegen
var session = new JobSession
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

## Abhängigkeiten

- `JOSYN.Foundation.ResultPattern`
- `Microsoft.EntityFrameworkCore.SqlServer`
