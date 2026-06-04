# JOSYN.Backend.ErrorHandler

Platform-wide error reporting endpoint for the JOSYN backend.

## Contract

`IErrorHandler` — receives error reports from any backend component:

```csharp
void Handle(string message);
void Handle(string message, Exception exception);
```

## First version

`FileSystemErrorHandler` writes a timestamped entry to `Console.Error` and appends it to
`%TEMP%\josyn-error.log`. Notification and durable storage are deferred.

## Known limitations

- Log path is hard-coded (`Path.GetTempPath()`). Will be moved to `IGlobalConfig` when a
  log-path property is added.
- No notification mechanism yet. Operator must monitor the log file manually.

## Dependencies

- `JOSYN.Backend.GlobalConfig` — accepted in constructor; reserved for future log-path use
- `JOSYN.Foundation.ResultPattern` — platform infrastructure baseline
