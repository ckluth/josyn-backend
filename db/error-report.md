# JOSYN Error Report

Generated: 2026-06-11 15:44:06  |  Showing last **50** entries  |  Found: **1**

## Uebersicht

| # | OccurredAt | Causer | JobName | SessionGuid | Message |
|---|------------|--------|---------|-------------|---------|
| [1](#error-1) | 2026-06-11 15:43:28 | JOSYN.Jap.JAPServer.JAPServer.PutError | Contoso.DemoProduct.DemoJob | 92f6d3d8-91fe-4ced-8f3f-f3a3be29b195 | Konfigurationsschlüssel nicht gefunden: 'RuntimeEnvironment' |

---

## Details

### Error #1

| Feld | Wert |
| ---- | ---- |
| UID         | 49af4639-95fb-4c3c-b4ff-efa2babb89f4 |
| OccurredAt  | 2026-06-11 15:43:28 |
| Causer      | JOSYN.Jap.JAPServer.JAPServer.PutError |
| JobName     | Contoso.DemoProduct.DemoJob |
| SessionGuid | 92f6d3d8-91fe-4ced-8f3f-f3a3be29b195 |

**Message**

```
Konfigurationsschlüssel nicht gefunden: 'RuntimeEnvironment'
```

**CallStack**

```
  at JipProtocol.ToResult() in JipProtocol.cs:72
  at <JOSYN-Jap-Contract-IJosynApplicationProtocol-GetEnvironment>d__10.GetEnvironment() in JAPClient.cs:69
```

---
