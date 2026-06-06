# insert-config-value.ps1
# Inserts or updates a key/value pair in josyn.ConfigStore.
# Run via insert-config-value.cmd.

. "$PSScriptRoot\db-config.ps1"

$key   = "RuntimeEnvironment"
$value = "DEV"

$sql = @"
IF NOT EXISTS (SELECT 1 FROM josyn.ConfigStore WHERE [Key] = '$key')
BEGIN
    INSERT INTO josyn.ConfigStore ([Key], [Value]) VALUES ('$key', '$value')
    PRINT 'Inserted: $key = $value'
END
ELSE
BEGIN
    UPDATE josyn.ConfigStore SET [Value] = '$value' WHERE [Key] = '$key'
    PRINT 'Updated: $key = $value'
END
"@

$tempSql = [System.IO.Path]::GetTempFileName() + ".sql"
$sql | Set-Content -Encoding UTF8 $tempSql

sqlcmd -S $DbServer -d $DbDatabase -U $DbUser -P $DbPassword -i $tempSql

Remove-Item $tempSql -ErrorAction SilentlyContinue
