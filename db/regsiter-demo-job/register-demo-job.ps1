# register-demo-job.ps1
# Temporary demo script — registers Contoso.DemoProduct.DemoJob in josyn.JobRegistry.
# Run via register-demo-job.cmd.

. "$PSScriptRoot\..\db-config.ps1"

$jobTypeName  = "Contoso.DemoProduct.DemoJob"
$techUser     = "tu.josyn"

$sql = @"
IF NOT EXISTS (SELECT 1 FROM josyn.JobRegistry WHERE Name = '$jobTypeName')
BEGIN
    INSERT INTO josyn.JobRegistry (Name, TechnicalUserName) VALUES ('$jobTypeName', '$techUser')
    PRINT 'Job registered: $jobTypeName'
END
ELSE
    PRINT 'Already registered: $jobTypeName'
"@

$tempSql = [System.IO.Path]::GetTempFileName() + ".sql"
$sql | Set-Content -Encoding UTF8 $tempSql

sqlcmd -S $DbServer -d $DbDatabase -U $DbUser -P $DbPassword -i $tempSql

Remove-Item $tempSql -ErrorAction SilentlyContinue
