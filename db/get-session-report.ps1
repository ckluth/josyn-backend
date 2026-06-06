# get-session-report.ps1
# Reads the last N entries from josyn.SessionStore and writes session-report.md.
# Run via get-session-report.cmd [N]   (default: 50)

param(
    [int] $Top = 50
)

. "$PSScriptRoot\db-config.ps1"

$output = "$PSScriptRoot\session-report.md"

# ---------------------------------------------------------------------------
# Query
# ---------------------------------------------------------------------------
$connStr = "Server=$DbServer;Database=$DbDatabase;User Id=$DbUser;Password=$DbPassword;TrustServerCertificate=True"
$query   = @"
SELECT TOP ($Top)
    Id, UID, JobTypeName, Arguments, Result
FROM josyn.SessionStore
ORDER BY Id DESC
"@

$conn = New-Object System.Data.SqlClient.SqlConnection $connStr
$conn.Open()
$cmd             = $conn.CreateCommand()
$cmd.CommandText = $query
$adapter         = New-Object System.Data.SqlClient.SqlDataAdapter $cmd
$table           = New-Object System.Data.DataTable
$adapter.Fill($table) | Out-Null
$conn.Close()

# ---------------------------------------------------------------------------
# Render markdown
# ---------------------------------------------------------------------------
$now   = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
$count = $table.Rows.Count
$lines = [System.Collections.Generic.List[string]]::new()

$lines.Add("# JOSYN Session Report")
$lines.Add("")
$lines.Add("Generated: $now  |  Showing last **$Top** entries  |  Found: **$count**")
$lines.Add("")

if ($count -eq 0) {
    $lines.Add("_No entries._")
}
else {
    # --- overview table ---
    $lines.Add("## Overview")
    $lines.Add("")
    $lines.Add("| # | Id | UID | JobTypeName | Arguments (preview) | Result (preview) |")
    $lines.Add("|---|----|-----|-------------|---------------------|-----------------|")

    $i = 1
    foreach ($row in $table.Rows) {
        $id          = $row["Id"]
        $uid         = $row["UID"]
        $jobTypeName = $row["JobTypeName"]

        $argStr  = ($row["Arguments"] -replace "\|", "\|" -replace "\r?\n", " ")
        $argPrev = $argStr.Substring(0, [Math]::Min(80, $argStr.Length))
        if ($argStr.Length -gt 80) { $argPrev += " ..." }

        $resStr  = ($row["Result"] -replace "\|", "\|" -replace "\r?\n", " ")
        $resPrev = $resStr.Substring(0, [Math]::Min(80, $resStr.Length))
        if ($resStr.Length -gt 80) { $resPrev += " ..." }

        $lines.Add("| [$i](#session-$i) | $id | $uid | $jobTypeName | $argPrev | $resPrev |")
        $i++
    }

    # --- detail sections ---
    $lines.Add("")
    $lines.Add("---")
    $lines.Add("")
    $lines.Add("## Details")

    $i = 1
    foreach ($row in $table.Rows) {
        $id          = $row["Id"]
        $uid         = $row["UID"]
        $jobTypeName = $row["JobTypeName"]
        $arguments   = $row["Arguments"]
        $result      = $row["Result"]

        $lines.Add("")
        $lines.Add("### Session #$i")
        $lines.Add("")
        $lines.Add("| Field       | Value |")
        $lines.Add("| ----------- | ----- |")
        $lines.Add("| Id          | $id |")
        $lines.Add("| UID         | $uid |")
        $lines.Add("| JobTypeName | $jobTypeName |")
        $lines.Add("")
        $lines.Add("**Arguments**")
        $lines.Add("")
        $lines.Add("``````")
        $lines.Add($arguments)
        $lines.Add("``````")
        $lines.Add("")
        $lines.Add("**Result**")
        $lines.Add("")
        $lines.Add("``````")
        $lines.Add($result)
        $lines.Add("``````")
        $lines.Add("")
        $lines.Add("---")
        $i++
    }
}

$lines | Set-Content -Path $output -Encoding UTF8
Write-Host "[OK] Report written: $output ($count entries)"
