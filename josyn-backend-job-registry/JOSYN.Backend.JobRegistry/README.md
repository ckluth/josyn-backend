# JOSYN.Backend.JobRegistry

Job-Registrierung im JOSYN Storage Realm. Teil des JOSYN Backend.

Stellt `IJobRegistry` und `IJobRegistrationRecord` als stabile Kontrakte bereit.
Die Produktionsimplementierung (`SqlJobRegistry`) verwendet EF Core gegen SQL Server
(Schema `josyn`, Tabelle `JobRegistrations`).

## Verwendung

```csharp
IJobRegistry registry = new SqlJobRegistry(connectionString);

// Job per Name nachschlagen
var get = registry.GetByName("demojob");

// Alle registrierten Jobs abrufen
var all = registry.GetAll();
```

## Datenbank

Tabelle: `josyn.JobRegistry`
DDL: `josyn-backend/db/migrations/V002__job_registry.sql`
Dev-Bootstrap: `josyn-backend/db/bootstrap-local-dev.sql`

## Abhängigkeiten

- `JOSYN.Foundation.ResultPattern`
- `Microsoft.EntityFrameworkCore.SqlServer`
