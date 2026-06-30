using JOSYN.Jrp.Launch;
using JOSYN.Jrp.Surface;

namespace JOSYN.Backend.Gateway.Host;

/// <summary>
/// Maps <see cref="JOSYN.Foundation.ResultPattern.Result{T}"/> values to Minimal API
/// <see cref="IResult"/> HTTP responses, translating <see cref="JrpErrorCategory"/> tokens
/// to standard status codes at the host edge (ADR-034 D-7).
/// </summary>
internal static class JrpHttpResult
{
    /// <summary>200 OK with the result value, or a problem response on failure.</summary>
    internal static IResult ToHttpResult<T>(this Foundation.ResultPattern.Result<T> result) =>
        result.Succeeded ? Results.Ok(result.Value) : ToErrorResult(result.ErrorMessage);

    /// <summary>Maps a non-generic failed <see cref="Foundation.ResultPattern.Result"/> to a problem response.</summary>
    internal static IResult ToHttpError(this Foundation.ResultPattern.Result result) =>
        ToErrorResult(result.ErrorMessage);

    /// <summary>202 Accepted with the result value, or a problem response on failure.</summary>
    internal static IResult ToHttpAccepted<T>(this Foundation.ResultPattern.Result<T> result) =>
        result.Succeeded ? Results.Accepted(value: result.Value) : ToErrorResult(result.ErrorMessage);

    // ── helpers ──────────────────────────────────────────────────────────────
    private static IResult ToErrorResult(string? message) =>
        JrpError.CategoryOf(message) switch
        {
            JrpErrorCategory.NotFound     => Results.Problem(detail: message, statusCode: 404),
            JrpErrorCategory.Invalid      => Results.Problem(detail: message, statusCode: 400),
            JrpErrorCategory.Unauthorized => Results.Problem(detail: "Unauthorized.", statusCode: 401),
            _                             => Results.Problem(detail: message, statusCode: 500)
        };
}
