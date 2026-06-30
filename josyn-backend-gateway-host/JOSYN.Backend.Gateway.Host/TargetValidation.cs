using JOSYN.Foundation.ResultPattern;
using JOSYN.Jap.Contract;
using JOSYN.Jrp.Launch;
using JOSYN.Jrp.Surface;

namespace JOSYN.Backend.Gateway.Host;

/// <summary>
/// Validates a <see cref="JrpTarget"/> against this Gateway's own identity (ADR-035 D-2).
/// Every endpoint calls <see cref="ParseAndValidate"/> or <see cref="ValidateEnvironment"/>;
/// node-specific execution verbs (<c>start-session</c>) also call <see cref="ValidateMachine"/>.
/// </summary>
internal static class TargetValidation
{
    /// <summary>
    /// Parses <paramref name="environment"/> and <paramref name="machine"/> from query strings
    /// and validates that the parsed environment matches <paramref name="thisEnv"/>.
    /// Used by GET endpoints where the target arrives as raw query parameters.
    /// </summary>
    internal static Result<JrpTarget> ParseAndValidate(
        string? environment, string? machine, RuntimeEnvironment thisEnv)
    {
        if (string.IsNullOrWhiteSpace(environment))
            return JrpError.Invalid("Query parameter 'environment' is required.");
        if (string.IsNullOrWhiteSpace(machine))
            return JrpError.Invalid("Query parameter 'machine' is required.");
        if (!Enum.TryParse<RuntimeEnvironment>(environment, ignoreCase: true, out var env))
            return JrpError.Invalid(
                $"Unknown environment '{environment}'. Valid values: {string.Join(", ", Enum.GetNames<RuntimeEnvironment>())}.");

        return ValidateEnvironment(new JrpTarget { Environment = env, Machine = machine }, thisEnv);
    }

    /// <summary>
    /// Validates that the target's environment matches this Gateway's own environment.
    /// Used by body-bound endpoints where the <see cref="JrpTarget"/> is already deserialized.
    /// </summary>
    internal static Result<JrpTarget> ValidateEnvironment(JrpTarget target, RuntimeEnvironment thisEnv)
    {
        if (target.Environment != thisEnv)
            return JrpError.Invalid(
                $"Environment mismatch: request targets '{target.Environment}' but this Gateway serves '{thisEnv}'.");
        return Result<JrpTarget>.Success(target);
    }

    /// <summary>
    /// Validates that <paramref name="machine"/> names this host — required by node-specific
    /// execution verbs such as <c>start-session</c> (ADR-035 D-1, D-2).
    /// </summary>
    internal static Result ValidateMachine(string machine)
    {
        if (!string.Equals(machine, System.Environment.MachineName, StringComparison.OrdinalIgnoreCase))
            return JrpError.Invalid(
                $"Machine mismatch: request targets '{machine}' but this Gateway is '{System.Environment.MachineName}'.");
        return Result.Success;
    }
}
