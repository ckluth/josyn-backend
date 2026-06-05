namespace JOSYN.Backend.ErrorHandler;

internal sealed class ErrorStoreEntity
{
    public int    Id               { get; set; }
    public Guid   UID              { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
    public string Causer           { get; set; } = string.Empty;

    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public string Message          { get; set; } = string.Empty;

    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public string? CallStack       { get; set; }

    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    public string? ExceptionDetails { get; set; }

    public string? JobName         { get; set; }
    public Guid?   SessionGuid     { get; set; }
}
