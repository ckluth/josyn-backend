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
    Id, UID, JobTypeName, Arguments, Result,
    JobVersion, UserName, UserDomain, ClientApplication, ClientMachine,
    TecUser, Started, ExecutionStatus, Progress, Finished,
    JapServerProcessId, JobHostProcessId,
    LastWriteTime, Host
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
# Helpers
# ---------------------------------------------------------------------------
function NullOr($row, $col) {
    if ($row.IsNull($col)) { return "-" }
    return $row[$col]
}

function Ts($row, $col) {
    if ($row.IsNull($col)) { return "-" }
    return ([DateTime]$row[$col]).ToString("yyyy-MM-dd HH:mm:ss")
}

function Preview($text, $max = 80) {
    $s = ($text -replace "\|", "\|" -replace "\r?\n", " ")
    if ($s.Length -gt $max) { return $s.Substring(0, $max) + " ..." }
    return $s
}

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
    $lines.Add("| # | Id | JobTypeName | Version | User | Started | Status | Result (preview) |")
    $lines.Add("|---|----|-------------|---------|------|---------|--------|-----------------|")

    $i = 1
    foreach ($row in $table.Rows) {
        $id      = $row["Id"]
        $job     = $row["JobTypeName"]
        $ver     = $row["JobVersion"]
        $user    = "$($row["UserName"])@$($row["UserDomain"])"
        $started = Ts $row "Started"
        $status  = $row["ExecutionStatus"]
        $resPrev = Preview $row["Result"]

        $lines.Add("| [$i](#session-$i) | $id | $job | $ver | $user | $started | $status | $resPrev |")
        $i++
    }

    # --- detail sections ---
    $lines.Add("")
    $lines.Add("---")
    $lines.Add("")
    $lines.Add("## Details")

    $i = 1
    foreach ($row in $table.Rows) {
        $lines.Add("")
        $lines.Add("### Session #$i  <a name='session-$i'></a>")
        $lines.Add("")
        $lines.Add("| Field             | Value |")
        $lines.Add("| ----------------- | ----- |")
        $lines.Add("| Id                | $($row["Id"]) |")
        $lines.Add("| UID               | $($row["UID"]) |")
        $lines.Add("| JobTypeName       | $($row["JobTypeName"]) |")
        $lines.Add("| JobVersion        | $($row["JobVersion"]) |")
        $lines.Add("| UserName          | $($row["UserName"]) |")
        $lines.Add("| UserDomain        | $($row["UserDomain"]) |")
        $lines.Add("| ClientApplication | $($row["ClientApplication"]) |")
        $lines.Add("| ClientMachine     | $($row["ClientMachine"]) |")
        $lines.Add("| TecUser           | $(NullOr $row "TecUser") |")
        $lines.Add("| Started           | $(Ts $row "Started") |")
        $lines.Add("| ExecutionStatus   | $($row["ExecutionStatus"]) |")
        $lines.Add("| Progress          | $(NullOr $row "Progress") |")
        $lines.Add("| Finished          | $(Ts $row "Finished") |")
        $lines.Add("| JapServerProcessId | $($row["JapServerProcessId"]) |")
        $lines.Add("| JobHostProcessId   | $($row["JobHostProcessId"]) |")
        $lines.Add("| LastWriteTime      | $(Ts $row "LastWriteTime") |")
        $lines.Add("| Host              | $(NullOr $row "Host") |")
        $lines.Add("")
        $lines.Add("**Arguments**")
        $lines.Add("")
        $lines.Add("``````")
        $lines.Add($row["Arguments"])
        $lines.Add("``````")
        $lines.Add("")
        $lines.Add("**Result**")
        $lines.Add("")
        $lines.Add("``````")
        $lines.Add($row["Result"])
        $lines.Add("``````")
        $lines.Add("")
        $lines.Add("---")
        $i++
    }
}

$lines | Set-Content -Path $output -Encoding UTF8
Write-Host "[OK] Report written: $output ($count entries)"