using JOSYN.Foundation.ResultPattern;
using JOSYN.Jrp.Surface;

namespace JOSYN.Backend.Gateway;

/// <summary>
/// Abstraction over <see cref="GatewayCommandHandler"/> so surface-side code and tests can depend
/// on the interface rather than the concrete backend class.
/// </summary>
public interface IGatewayCommandHandler
{
    /// <inheritdoc cref="GatewayCommandHandler.HandleChangeJobArgument"/>
    Result<ArgumentChangeOutcome> HandleChangeJobArgument(ChangeJobArgument command);
}
