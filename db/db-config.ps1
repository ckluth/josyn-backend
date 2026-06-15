# db-config.ps1
# Single source of truth for local-dev database connection parameters.
# Dot-source this file in all db scripts: . "$PSScriptRoot\db-config.ps1"

if ($env:COMPUTERNAME -eq "RZ-KNB784") {
    $DbServer = "(localdb)\MSSQLLocalDB"
} else {
    $DbServer = "localhost\SQLEXPRESS01"
}

$DbDatabase = "josyn-db-local"
$DbUser     = "tu.josyn"
$DbPassword = "josyn"
