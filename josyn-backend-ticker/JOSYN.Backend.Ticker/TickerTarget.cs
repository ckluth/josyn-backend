namespace JOSYN.Backend.Ticker;

/// <summary>
/// A single target entry from the <c>[Ticker-Targets]</c> config section.
/// </summary>
/// <param name="Name">Logical name — also the sub-folder under BackendRoot where the EXE lives.</param>
/// <param name="ExeName">Executable filename, without path.</param>
/// <param name="Offset">Second within a minute at which the first fire occurs (0–59).</param>
/// <param name="Period">How often to fire, in seconds (1–60).</param>
internal sealed record TickerTarget(string Name, string ExeName, int Offset, int Period);
