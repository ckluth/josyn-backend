namespace JOSYN.Backend.SessionStore;

public interface IJobSessionRecord
{
    Guid UID { get; init; }
    string JobTypeName { get; init; }
    string Arguments { get; init; }
    string Result { get; init; }
}
