# register-demo-job.ps1
# Temporary demo script — registers MyDemoCompany.MyDemoProduct.MyDemoJob in josyn.JobRegistry.
# Run via register-demo-job.cmd.

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

sqlcmd -S "localhost\SQLEXPRESS01" -d "josyn-db-local" -U "tu.josyn" -P "josyn" -i $tempSql

Remove-Item $tempSql -ErrorAction SilentlyContinue
