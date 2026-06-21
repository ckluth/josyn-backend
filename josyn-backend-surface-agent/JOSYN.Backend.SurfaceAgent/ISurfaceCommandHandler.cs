using JOSYN.Backend.JobRegistry;
using JOSYN.Foundation.ResultPattern;

namespace JOSYN.Backend.SurfaceAgent;

/// <summary>
/// Abstraction over <see cref="SurfaceCommandHandler"/> so surface-side code and tests can depend
/// on the interface rather than the concrete backend class.
/// </summary>
public interface ISurfaceCommandHandler
{
    /// <inheritdoc cref="SurfaceCommandHandler.HandleChangeJobArgument"/>
    Result<ArgumentChangeOutcome> HandleChangeJobArgument(ChangeJobArgumentCommand command);
}
