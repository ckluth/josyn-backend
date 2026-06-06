# db-config.ps1
# Single source of truth for local-dev database connection parameters.
# Dot-source this file in all db scripts: . "$PSScriptRoot\db-config.ps1"

$DbServer   = "localhost\SQLEXPRESS01"
$DbDatabase = "josyn-db-local"
$DbUser     = "tu.josyn"
$DbPassword = "josyn"
