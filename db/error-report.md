# JOSYN Error Report

Generated: 2026-06-12 18:53:40  |  Showing last **50** entries  |  Found: **2**

## Uebersicht

| # | OccurredAt | Causer | JobName | SessionGuid | Message |
|---|------------|--------|---------|-------------|---------|
| [1](#error-1) | 2026-06-12 18:12:52 | JOSYN.Jap.JAPServer.JAPServer.PutError | Contoso.DemoProduct.DemoJob | 25bb538c-b6d1-4e5d-998e-cae740ff50f8 | No sections found. |
| [2](#error-2) | 2026-06-12 18:09:40 | JOSYN.Jap.JAPServer.JAPServer.PutError | Contoso.DemoProduct.DemoJob | 910f0464-748b-469a-9ab1-cf5c1f732415 | No sections found. |

---

## Details

### Error #1

| Feld | Wert |
| ---- | ---- |
| UID         | 94dd3cd8-3b9b-4152-9b00-c31d5636b6b1 |
| OccurredAt  | 2026-06-12 18:12:52 |
| Causer      | JOSYN.Jap.JAPServer.JAPServer.PutError |
| JobName     | Contoso.DemoProduct.DemoJob |
| SessionGuid | 25bb538c-b6d1-4e5d-998e-cae740ff50f8 |

**Message**

```
No sections found.
```

**CallStack**

```
  at IniDictionarySerializer.DeserializeSingleSection()
  at PropertyBag.Deserialize() in PropertyBag.cs:273
  at JobInvoker.RetrieveInvocationArguments() in JobInvoker.cs:187
  at <CreateInvocationArguments>d__5.CreateInvocationArguments() in JobInvoker.cs:151
  at <InvokeJob>d__1.InvokeJob() in JobInvoker.cs:46
```

---

### Error #2

| Feld | Wert |
| ---- | ---- |
| UID         | 53174be0-a8fe-47b7-add7-f742dfeae0f1 |
| OccurredAt  | 2026-06-12 18:09:40 |
| Causer      | JOSYN.Jap.JAPServer.JAPServer.PutError |
| JobName     | Contoso.DemoProduct.DemoJob |
| SessionGuid | 910f0464-748b-469a-9ab1-cf5c1f732415 |

**Message**

```
No sections found.
```

**CallStack**

```
  at IniDictionarySerializer.DeserializeSingleSection()
  at PropertyBag.Deserialize() in PropertyBag.cs:273
  at JobInvoker.RetrieveInvocationArguments() in JobInvoker.cs:187
  at <CreateInvocationArguments>d__5.CreateInvocationArguments() in JobInvoker.cs:151
  at <InvokeJob>d__1.InvokeJob() in JobInvoker.cs:46
```

---
