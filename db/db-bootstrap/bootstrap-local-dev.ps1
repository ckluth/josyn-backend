# bootstrap-local-dev.ps1
# Drops and recreates the josyn-db-local database from scratch.
# Runs via bootstrap-local-dev.cmd.
#
# WARNING: ALL EXISTING DATA WILL BE LOST.
# Uses Windows authentication -- requires sysadmin rights on the target server.

. "$PSScriptRoot\..\db-config.ps1"

Write-Host ""
Write-Host "=========================================================" -ForegroundColor Yellow
Write-Host "  JOSYN -- Bootstrap Local Development Database" -ForegroundColor Yellow
Write-Host "=========================================================" -ForegroundColor Yellow
Write-Host ""
Write-Host "  Server   : $DbServer" -ForegroundColor Cyan
Write-Host "  Database : $DbDatabase" -ForegroundColor Cyan
Write-Host ""
Write-Host "  WARNING: This will DROP and RECREATE the database." -ForegroundColor Red
Write-Host "           All existing data will be permanently lost." -ForegroundColor Red
Write-Host ""

$confirmation = Read-Host "  Type YES to continue"

if ($confirmation -ne "YES") {
    Write-Host ""
    Write-Host "  Aborted." -ForegroundColor Gray
    Write-Host ""
    exit 0
}

Write-Host ""
Write-Host "  Running bootstrap script..." -ForegroundColor Cyan

sqlcmd -S $DbServer -E -i "$PSScriptRoot\bootstrap-local-dev.sql"

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "  Done. Database is ready." -ForegroundColor Green
} else {
    Write-Host ""
    Write-Host "  Bootstrap failed. Exit code: $LASTEXITCODE" -ForegroundColor Red
}

Write-Host ""