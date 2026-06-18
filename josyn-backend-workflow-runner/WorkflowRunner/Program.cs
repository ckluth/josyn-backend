// WorkflowRunner stub — Ticker target (ADR-024).
// Logs that it was invoked, then exits cleanly.
// This is a placeholder; replace with real workflow-condition session scheduling logic.

var logDir  = Path.Combine(AppContext.BaseDirectory, "logs");
var logFile = Path.Combine(logDir, $"{DateTime.Now:yyyy-MM-dd}.log");
var entry   = $"[{DateTime.Now:HH:mm:ss}] WorkflowRunner invoked by Ticker.";

Directory.CreateDirectory(logDir);
File.AppendAllText(logFile, entry + Environment.NewLine);

Console.WriteLine(entry);
