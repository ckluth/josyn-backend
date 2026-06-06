# get-error-report.ps1
# Reads the last N entries from josyn.ErrorStore and writes error-report.md.
# Run via get-error-report.cmd [N]   (default: 50)

param(
    [int] $Top = 50
)

. "$PSScriptRoot\db-config.ps1"

$output  = "$PSScriptRoot\error-report.md"

# ---------------------------------------------------------------------------
# Query
# ---------------------------------------------------------------------------
$connStr = "Server=$DbServer;Database=$DbDatabase;User Id=$DbUser;Password=$DbPassword;TrustServerCertificate=True"
$query   = @"
SELECT TOP ($Top)
    Id, UID, OccurredAt, Causer, Message,
    CallStack, ExceptionDetails, JobName, SessionGuid
FROM josyn.ErrorStore
ORDER BY OccurredAt DESC
"@

$conn = New-Object System.Data.SqlClient.SqlConnection $connStr
$conn.Open()
$cmd        = $conn.CreateCommand()
$cmd.CommandText = $query
$adapter    = New-Object System.Data.SqlClient.SqlDataAdapter $cmd
$table      = New-Object System.Data.DataTable
$adapter.Fill($table) | Out-Null
$conn.Close()

# ---------------------------------------------------------------------------
# Render markdown
# ---------------------------------------------------------------------------
$now   = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
$count = $table.Rows.Count
$lines = [System.Collections.Generic.List[string]]::new()

$lines.Add("# JOSYN Error Report")
$lines.Add("")
$lines.Add("Generated: $now  |  Showing last **$Top** entries  |  Found: **$count**")
$lines.Add("")

if ($count -eq 0) {
    $lines.Add("_Keine Eintraege._")
}
else {
    # --- overview table ---
    $lines.Add("## Uebersicht")
    $lines.Add("")
    $lines.Add("| # | OccurredAt | Causer | JobName | SessionGuid | Message |")
    $lines.Add("|---|------------|--------|---------|-------------|---------|")

    $i = 1
    foreach ($row in $table.Rows) {
        $occurredAt  = [DateTimeOffset]$row["OccurredAt"]
        $ts          = $occurredAt.ToString("yyyy-MM-dd HH:mm:ss")
        $causer      = $row["Causer"]
        $jobName     = if ($row.IsNull("JobName"))     { "-" } else { $row["JobName"] }
        $sessionGuid = if ($row.IsNull("SessionGuid")) { "-" } else { $row["SessionGuid"] }
        $message     = ($row["Message"] -replace "\|", "\|" -replace "\r?\n", " ").Substring(0, [Math]::Min(120, $row["Message"].Length))
        if ($row["Message"].Length -gt 120) { $message += " ..." }

        $lines.Add("| [$i](#error-$i) | $ts | $causer | $jobName | $sessionGuid | $message |")
        $i++
    }

    # --- detail sections ---
    $lines.Add("")
    $lines.Add("---")
    $lines.Add("")
    $lines.Add("## Details")

    $i = 1
    foreach ($row in $table.Rows) {
        $occurredAt      = [DateTimeOffset]$row["OccurredAt"]
        $ts              = $occurredAt.ToString("yyyy-MM-dd HH:mm:ss")
        $causer          = $row["Causer"]
        $uid             = $row["UID"]
        $jobName         = if ($row.IsNull("JobName"))          { "-" } else { $row["JobName"] }
        $sessionGuid     = if ($row.IsNull("SessionGuid"))      { "-" } else { $row["SessionGuid"] }
        $message         = $row["Message"]
        $callStack       = if ($row.IsNull("CallStack"))        { "-" } else { $row["CallStack"] }
        $exDetails       = if ($row.IsNull("ExceptionDetails")) { "-" } else { $row["ExceptionDetails"] }

        $lines.Add("")
        $lines.Add("### Error #$i")
        $lines.Add("")
        $lines.Add("| Feld | Wert |")
        $lines.Add("| ---- | ---- |")
        $lines.Add("| UID         | $uid |")
        $lines.Add("| OccurredAt  | $ts |")
        $lines.Add("| Causer      | $causer |")
        $lines.Add("| JobName     | $jobName |")
        $lines.Add("| SessionGuid | $sessionGuid |")
        $lines.Add("")
        $lines.Add("**Message**")
        $lines.Add("")
        $lines.Add("``````")
        $lines.Add($message)
        $lines.Add("``````")

        if ($callStack -ne "-") {
            $lines.Add("")
            $lines.Add("**CallStack**")
            $lines.Add("")
            $lines.Add("``````")
            $lines.Add($callStack)
            $lines.Add("``````")
        }

        if ($exDetails -ne "-") {
            $lines.Add("")
            $lines.Add("**ExceptionDetails**")
            $lines.Add("")
            $lines.Add("``````")
            $lines.Add($exDetails)
            $lines.Add("``````")
        }

        $lines.Add("")
        $lines.Add("---")
        $i++
    }
}

$lines | Set-Content -Path $output -Encoding UTF8
Write-Host "[OK] Report geschrieben: $output ($count Eintraege)"
