namespace JOSYN.Backend.SessionStore;

internal sealed class SessionStoreEntity
{
    public int      Id                { get; set; }
    public Guid     UID               { get; set; }
    public string   JobTypeName       { get; set; } = string.Empty;

    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    // Arguments can be large, so we won't set a max length.
    public string   Arguments         { get; set; } = string.Empty;

    // ReSharper disable once EntityFramework.ModelValidation.UnlimitedStringLength
    // Result can also be large, so we won't set a max length.
    public string   Result            { get; set; } = string.Empty;

    public string   JobVersion        { get; set; } = string.Empty;
    public string   UserName          { get; set; } = string.Empty;
    public string   UserDomain        { get; set; } = string.Empty;
    public string   ClientApplication { get; set; } = string.Empty;
    public string   ClientMachine     { get; set; } = string.Empty;
    public string?  TecUser           { get; set; }
    public DateTime Started           { get; set; }
    public string   ExecutionStatus   { get; set; } = string.Empty;
    public string?  Progress          { get; set; }
    public DateTime? Finished         { get; set; }
    public int      JapServerProcessId { get; set; }
    public int      JobHostProcessId  { get; set; }
    public DateTime? LastWriteTime    { get; set; }
    public string?  Host              { get; set; }
}
