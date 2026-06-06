# JOSYN Error Report

Generated: 2026-06-06 12:40:19  |  Showing last **50** entries  |  Found: **14**

## Uebersicht

| # | OccurredAt | Causer | JobName | SessionGuid | Message |
|---|------------|--------|---------|-------------|---------|
| [1](#error-1) | 2026-06-06 11:56:04 | JOSYN.Jap.JAPServer.Host.RunServer | Contoso.DemoProduct.DemoJob | feb09a53-50e5-4151-b5a8-c4e41d9a2b7b | Client-Exe not found: C:\ProgramData\JOSYN\JobRepository\Contoso.DemoProduct.DemoJob.exe |
| [2](#error-2) | 2026-06-06 11:13:43 | RunServer | Contoso.DemoProduct.DemoJob | 59ff1f8b-7965-438b-bc64-c7d3c1bb6a45 | Client-Exe not found: C:\ProgramData\JOSYN\JobRepository\Contoso.DemoProduct.DemoJob.exe |
| [3](#error-3) | 2026-06-06 11:13:03 | RunServer | Contoso.DemoProduct.DemoJob | d8d4148c-a17f-4e5b-b43a-e9150487b4aa | Client-Exe not found: C:\ProgramData\JOSYN\JobRepository\Contoso.DemoProduct.DemoJob.exe |
| [4](#error-4) | 2026-06-06 00:57:28 | <Main>$ | - | - | Job nicht registriert: 'Contoso.DemoProduct.DemoJob'. Bitte zuerst in josyn.JobRegistry eintragen. |
| [5](#error-5) | 2026-06-06 00:57:11 | <Main>$ | - | - | Job nicht registriert: 'Contoso.DemoProduct.DemoJob'. Bitte zuerst in josyn.JobRegistry eintragen. |
| [6](#error-6) | 2026-06-05 23:42:31 | HandleHandlerError | MyDemoCompany.MyDemoProduct.MyDemoJob | 264569b6-05b3-4372-844f-50b6ffcaf61f | Fehler beim Verarbeiten der Anfrage: {"What":"GetEnvironment","Data":null} |
| [7](#error-7) | 2026-06-05 23:38:32 | HandleHandlerError | MyDemoCompany.MyDemoProduct.MyDemoJob | 74ac1f2c-272e-42f9-8354-19b1bb63d7cf | Fehler beim Verarbeiten der Anfrage: {"What":"GetEnvironment","Data":null} |
| [8](#error-8) | 2026-06-05 16:26:13 | RunServer | MyDemoCompany.MyDemoProduct.MyDemoJob | bef44539-bda6-4ee7-b8e2-2703c2f5fdd7 | Methode 'IJosynApplicationProtocol.GetEnvironment' hat eine nicht unterstützte Signatur für RegisterAll. Unterstützt: Ta ... |
| [9](#error-9) | 2026-06-05 16:21:23 | RunServer | MyDemoCompany.MyDemoProduct.MyDemoJob | 2dd50f74-912e-4d31-8e64-b23d098e47d3 | Methode 'IJosynApplicationProtocol.GetEnvironment' hat eine nicht unterstützte Signatur für RegisterAll. Unterstützt: Ta ... |
| [10](#error-10) | 2026-06-05 16:17:06 | RunServer | MyDemoCompany.MyDemoProduct.MyDemoJob | 07ac267c-0df9-4238-9757-b021630f82aa | Methode 'IJosynApplicationProtocol.GetEnvironment' hat eine nicht unterstützte Signatur für RegisterAll. Unterstützt: Ta ... |
| [11](#error-11) | 2026-06-05 16:03:54 | RunServer | MyDemoCompany.MyDemoProduct.MyDemoJob | d59ac543-5a86-442c-bf20-face9eed08fa | Methode 'IJosynApplicationProtocol.GetEnvironment' hat eine nicht unterstützte Signatur für RegisterAll. Unterstützt: Ta ... |
| [12](#error-12) | 2026-06-05 16:02:06 | RunServer | MyDemoCompany.MyDemoProduct.MyDemoJob | 6fbdc7ce-6e51-4f07-9fff-49f1a6fab95b | Methode 'IJosynApplicationProtocol.GetEnvironment' hat eine nicht unterstützte Signatur für RegisterAll. Unterstützt: Ta ... |
| [13](#error-13) | 2026-06-05 16:01:39 | <Main>$ | - | - | JAPServer-Executable nicht gefunden: 'C:\Temp\VS.OUT\JOSYN\JOSYN.Jap.JAPServer\bin\Release\JOSYN.Jap.JAPServer.exe' |
| [14](#error-14) | 2026-06-05 16:00:23 | <Main>$ | - | - | JAPServer-Executable nicht gefunden: 'C:\Temp\VS.OUT\JOSYN\JOSYN.Jap.JAPServer\bin\Release\JOSYN.Jap.JAPServer.exe' |

---

## Details

### Error #1

| Feld | Wert |
| ---- | ---- |
| UID         | 702208f6-60ae-4b48-879d-2beff6ff0e2c |
| OccurredAt  | 2026-06-06 11:56:04 |
| Causer      | JOSYN.Jap.JAPServer.Host.RunServer |
| JobName     | Contoso.DemoProduct.DemoJob |
| SessionGuid | feb09a53-50e5-4151-b5a8-c4e41d9a2b7b |

**Message**

```
Client-Exe not found: C:\ProgramData\JOSYN\JobRepository\Contoso.DemoProduct.DemoJob.exe
```

**CallStack**

```
  at PipesServer.StartClientExe()
  at <RunAsyncInternal>d__1.RunAsyncInternal() in PipesServer.cs:53
  at <RunAsync>d__0.RunAsync() in PipesServer.cs:39
```

---

### Error #2

| Feld | Wert |
| ---- | ---- |
| UID         | 569cc0e8-ec86-478c-bef7-9279f9ff2fab |
| OccurredAt  | 2026-06-06 11:13:43 |
| Causer      | RunServer |
| JobName     | Contoso.DemoProduct.DemoJob |
| SessionGuid | 59ff1f8b-7965-438b-bc64-c7d3c1bb6a45 |

**Message**

```
Client-Exe not found: C:\ProgramData\JOSYN\JobRepository\Contoso.DemoProduct.DemoJob.exe
```

**CallStack**

```
  at PipesServer.StartClientExe()
  at <RunAsyncInternal>d__1.RunAsyncInternal() in PipesServer.cs:53
  at <RunAsync>d__0.RunAsync() in PipesServer.cs:39
```

---

### Error #3

| Feld | Wert |
| ---- | ---- |
| UID         | 18cefcb6-99fb-4ffb-b477-0b51b6809f66 |
| OccurredAt  | 2026-06-06 11:13:03 |
| Causer      | RunServer |
| JobName     | Contoso.DemoProduct.DemoJob |
| SessionGuid | d8d4148c-a17f-4e5b-b43a-e9150487b4aa |

**Message**

```
Client-Exe not found: C:\ProgramData\JOSYN\JobRepository\Contoso.DemoProduct.DemoJob.exe
```

**CallStack**

```
  at PipesServer.StartClientExe()
  at <RunAsyncInternal>d__1.RunAsyncInternal() in PipesServer.cs:53
  at <RunAsync>d__0.RunAsync() in PipesServer.cs:39
```

---

### Error #4

| Feld | Wert |
| ---- | ---- |
| UID         | b18eb81d-ff66-46ef-b8d7-7725c6c01551 |
| OccurredAt  | 2026-06-06 00:57:28 |
| Causer      | <Main>$ |
| JobName     | - |
| SessionGuid | - |

**Message**

```
Job nicht registriert: 'Contoso.DemoProduct.DemoJob'. Bitte zuerst in josyn.JobRegistry eintragen.
```

**CallStack**

```
  at SessionStarter.StartSession()
```

---

### Error #5

| Feld | Wert |
| ---- | ---- |
| UID         | 64849289-244a-47ec-8f47-2be0eda39235 |
| OccurredAt  | 2026-06-06 00:57:11 |
| Causer      | <Main>$ |
| JobName     | - |
| SessionGuid | - |

**Message**

```
Job nicht registriert: 'Contoso.DemoProduct.DemoJob'. Bitte zuerst in josyn.JobRegistry eintragen.
```

**CallStack**

```
  at SessionStarter.StartSession()
```

---

### Error #6

| Feld | Wert |
| ---- | ---- |
| UID         | 0daf4352-8ea3-46a0-bacb-cafede233b10 |
| OccurredAt  | 2026-06-05 23:42:31 |
| Causer      | HandleHandlerError |
| JobName     | MyDemoCompany.MyDemoProduct.MyDemoJob |
| SessionGuid | 264569b6-05b3-4372-844f-50b6ffcaf61f |

**Message**

```
Fehler beim Verarbeiten der Anfrage: {"What":"GetEnvironment","Data":null}
```

**ExceptionDetails**

```
System.Reflection.TargetInvocationException: Exception has been thrown by the target of an invocation.
 ---> System.MissingMethodException: Method not found: 'JOSYN.Foundation.ResultPattern.Error JOSYN.Foundation.ResultPattern.Error.op_Implicit(System.String)'.
   at Contoso.Josyn.Adapter.ContosoConfigSource.GetValue(String key)
   at Contoso.Josyn.Adapter.ContosoConfigSource.GetValue(String key)
   at JOSYN.Backend.ConfigStore.ConfigStore.GetValue(String key)
   at JOSYN.Jap.JAPServer.JAPServer.JOSYN.Jap.Shared.Contract.IJosynApplicationProtocol.GetEnvironment() in C:\Users\chris\OneDrive\DevGit\josyn-backend\josyn-backend-jap-server\JOSYN.Jap.JAPServer\JAPServer.cs:line 75
   at System.Reflection.MethodBaseInvoker.InterpretedInvoke_Method(Object obj, IntPtr* args)
   at System.Reflection.MethodBaseInvoker.InvokeWithNoArgs(Object obj, BindingFlags invokeAttr)
   --- End of inner exception stack trace ---
   at System.Reflection.MethodBaseInvoker.InvokeWithNoArgs(Object obj, BindingFlags invokeAttr)
   at System.Reflection.RuntimeMethodInfo.Invoke(Object obj, BindingFlags invokeAttr, Binder binder, Object[] parameters, CultureInfo culture)
   at System.Reflection.MethodBase.Invoke(Object obj, Object[] parameters)
   at JOSYN.Foundation.JIP.JipDispatcher.<>c__DisplayClass12_0`1.<<WrapEnumHandler>b__0>d.MoveNext()
--- End of stack trace from previous location ---
   at JOSYN.Foundation.JIP.JipDispatcher.<.ctor>b__4_0(Request req)
   at JOSYN.Foundation.JIP.JipServer.<>c__DisplayClass1_0.<<WrapHandler>b__0>d.MoveNext()
--- End of stack trace from previous location ---
   at JOSYN.Jap.JAPServer.Host.HandleRequest(IJipDispatcher dispatcher, String requestStr) in C:\Users\chris\OneDrive\DevGit\josyn-backend\josyn-backend-jap-server\JOSYN.Jap.JAPServer\Host.cs:line 193
   at JOSYN.Foundation.JIP.PipesServer.RawRequestHandler.ProcessRawRequest(Byte[] requestBytes)
   at JOSYN.Foundation.JIP.PipesServer.RequestLoopAsync(NamedPipeServerStream reqPipe, NamedPipeServerStream resPipe, Func`2 processRequest, Func`3 onError, CancellationToken cancellationToken)
```

---

### Error #7

| Feld | Wert |
| ---- | ---- |
| UID         | c36334e7-ed4f-4a85-b809-47fbb1522bc4 |
| OccurredAt  | 2026-06-05 23:38:32 |
| Causer      | HandleHandlerError |
| JobName     | MyDemoCompany.MyDemoProduct.MyDemoJob |
| SessionGuid | 74ac1f2c-272e-42f9-8354-19b1bb63d7cf |

**Message**

```
Fehler beim Verarbeiten der Anfrage: {"What":"GetEnvironment","Data":null}
```

**ExceptionDetails**

```
System.Reflection.TargetInvocationException: Exception has been thrown by the target of an invocation.
 ---> System.MissingMethodException: Method not found: 'JOSYN.Foundation.ResultPattern.Error JOSYN.Foundation.ResultPattern.Error.op_Implicit(System.String)'.
   at Contoso.Josyn.Adapter.ContosoConfigSource.GetValue(String key)
   at Contoso.Josyn.Adapter.ContosoConfigSource.GetValue(String key)
   at JOSYN.Backend.ConfigStore.ConfigStore.GetValue(String key)
   at JOSYN.Jap.JAPServer.JAPServer.JOSYN.Jap.Shared.Contract.IJosynApplicationProtocol.GetEnvironment() in C:\Users\chris\OneDrive\DevGit\josyn-backend\josyn-backend-jap-server\JOSYN.Jap.JAPServer\JAPServer.cs:line 75
   at System.Reflection.MethodBaseInvoker.InterpretedInvoke_Method(Object obj, IntPtr* args)
   at System.Reflection.MethodBaseInvoker.InvokeWithNoArgs(Object obj, BindingFlags invokeAttr)
   --- End of inner exception stack trace ---
   at System.Reflection.MethodBaseInvoker.InvokeWithNoArgs(Object obj, BindingFlags invokeAttr)
   at System.Reflection.RuntimeMethodInfo.Invoke(Object obj, BindingFlags invokeAttr, Binder binder, Object[] parameters, CultureInfo culture)
   at System.Reflection.MethodBase.Invoke(Object obj, Object[] parameters)
   at JOSYN.Foundation.JIP.JipDispatcher.<>c__DisplayClass12_0`1.<<WrapEnumHandler>b__0>d.MoveNext()
--- End of stack trace from previous location ---
   at JOSYN.Foundation.JIP.JipDispatcher.<.ctor>b__4_0(Request req)
   at JOSYN.Foundation.JIP.JipServer.<>c__DisplayClass1_0.<<WrapHandler>b__0>d.MoveNext()
--- End of stack trace from previous location ---
   at JOSYN.Jap.JAPServer.Host.HandleRequest(IJipDispatcher dispatcher, String requestStr) in C:\Users\chris\OneDrive\DevGit\josyn-backend\josyn-backend-jap-server\JOSYN.Jap.JAPServer\Host.cs:line 193
   at JOSYN.Foundation.JIP.PipesServer.RawRequestHandler.ProcessRawRequest(Byte[] requestBytes)
   at JOSYN.Foundation.JIP.PipesServer.RequestLoopAsync(NamedPipeServerStream reqPipe, NamedPipeServerStream resPipe, Func`2 processRequest, Func`3 onError, CancellationToken cancellationToken)
```

---

### Error #8

| Feld | Wert |
| ---- | ---- |
| UID         | 96d82564-eb8c-4a7a-9b1a-2f524887b19e |
| OccurredAt  | 2026-06-05 16:26:13 |
| Causer      | RunServer |
| JobName     | MyDemoCompany.MyDemoProduct.MyDemoJob |
| SessionGuid | bef44539-bda6-4ee7-b8e2-2703c2f5fdd7 |

**Message**

```
Methode 'IJosynApplicationProtocol.GetEnvironment' hat eine nicht unterstützte Signatur für RegisterAll. Unterstützt: Task<Result<string>> Method() oder Task<Result> Method(string).
```

**CallStack**

```
  at JipDispatcher.RegisterAll() in JipDispatcher.cs:89
```

---

### Error #9

| Feld | Wert |
| ---- | ---- |
| UID         | f8086b33-05ed-4c61-873b-079a9a06f5fc |
| OccurredAt  | 2026-06-05 16:21:23 |
| Causer      | RunServer |
| JobName     | MyDemoCompany.MyDemoProduct.MyDemoJob |
| SessionGuid | 2dd50f74-912e-4d31-8e64-b23d098e47d3 |

**Message**

```
Methode 'IJosynApplicationProtocol.GetEnvironment' hat eine nicht unterstützte Signatur für RegisterAll. Unterstützt: Task<Result<string>> Method() oder Task<Result> Method(string).
```

**CallStack**

```
  at JipDispatcher.RegisterAll() in JipDispatcher.cs:89
```

---

### Error #10

| Feld | Wert |
| ---- | ---- |
| UID         | 61d73052-af02-4103-ab4e-824de6e20aa5 |
| OccurredAt  | 2026-06-05 16:17:06 |
| Causer      | RunServer |
| JobName     | MyDemoCompany.MyDemoProduct.MyDemoJob |
| SessionGuid | 07ac267c-0df9-4238-9757-b021630f82aa |

**Message**

```
Methode 'IJosynApplicationProtocol.GetEnvironment' hat eine nicht unterstützte Signatur für RegisterAll. Unterstützt: Task<Result<string>> Method() oder Task<Result> Method(string).
```

**CallStack**

```
  at JipDispatcher.RegisterAll() in JipDispatcher.cs:89
```

---

### Error #11

| Feld | Wert |
| ---- | ---- |
| UID         | 9bcf7e74-9766-45d1-be98-ddd003e8fa06 |
| OccurredAt  | 2026-06-05 16:03:54 |
| Causer      | RunServer |
| JobName     | MyDemoCompany.MyDemoProduct.MyDemoJob |
| SessionGuid | d59ac543-5a86-442c-bf20-face9eed08fa |

**Message**

```
Methode 'IJosynApplicationProtocol.GetEnvironment' hat eine nicht unterstützte Signatur für RegisterAll. Unterstützt: Task<Result<string>> Method() oder Task<Result> Method(string).
```

**CallStack**

```
  at JipDispatcher.RegisterAll() in JipDispatcher.cs:89
```

---

### Error #12

| Feld | Wert |
| ---- | ---- |
| UID         | 8132d2b5-f53d-4d60-bbc1-020ddda009b5 |
| OccurredAt  | 2026-06-05 16:02:06 |
| Causer      | RunServer |
| JobName     | MyDemoCompany.MyDemoProduct.MyDemoJob |
| SessionGuid | 6fbdc7ce-6e51-4f07-9fff-49f1a6fab95b |

**Message**

```
Methode 'IJosynApplicationProtocol.GetEnvironment' hat eine nicht unterstützte Signatur für RegisterAll. Unterstützt: Task<Result<string>> Method() oder Task<Result> Method(string).
```

**CallStack**

```
  at JipDispatcher.RegisterAll() in JipDispatcher.cs:89
```

---

### Error #13

| Feld | Wert |
| ---- | ---- |
| UID         | 7487a4bf-51f2-4a68-bfba-b909709a059c |
| OccurredAt  | 2026-06-05 16:01:39 |
| Causer      | <Main>$ |
| JobName     | - |
| SessionGuid | - |

**Message**

```
JAPServer-Executable nicht gefunden: 'C:\Temp\VS.OUT\JOSYN\JOSYN.Jap.JAPServer\bin\Release\JOSYN.Jap.JAPServer.exe'
```

**CallStack**

```
  at SessionStarter.StartSession()
```

---

### Error #14

| Feld | Wert |
| ---- | ---- |
| UID         | 031232c1-d495-4b87-b25a-b2e215545535 |
| OccurredAt  | 2026-06-05 16:00:23 |
| Causer      | <Main>$ |
| JobName     | - |
| SessionGuid | - |

**Message**

```
JAPServer-Executable nicht gefunden: 'C:\Temp\VS.OUT\JOSYN\JOSYN.Jap.JAPServer\bin\Release\JOSYN.Jap.JAPServer.exe'
```

**CallStack**

```
  at SessionStarter.StartSession()
```

---
