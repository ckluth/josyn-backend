#pragma warning disable IDE0130
namespace JOSYN.Backend.JobRegistry;
#pragma warning restore IDE0130

/// <summary>
/// The outcome of a <see cref="IJobArgumentWriter.ChangeArgument"/> operation.
/// The handler reads both <see cref="Before"/> and writes <see cref="After"/> in one atomic
/// operation so callers do not need a racy read-before-write to show a meaningful diff.
/// </summary>
public sealed record ArgumentChangeOutcome
{
    /// <summary>Registry name of the job.</summary>
    public required string JobName { get; init; }

    /// <summary>Name of the argument record that was changed.</summary>
    public required string ArgumentName { get; init; }

    /// <summary>Content of the argument record before this change.</summary>
    public required string Before { get; init; }

    /// <summary>Content of the argument record after this change.</summary>
    public required string After { get; init; }
}
