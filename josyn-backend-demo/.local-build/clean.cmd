@echo off
CHCP 1252
setlocal

set "ROOT=%~dp0.."

echo [INFO] Bereinige Build-Ausgaben...

dotnet clean "%ROOT%\JOSYN.Backend.Demo.FakeSessionStarterConsumer.slnx" --configuration Release
dotnet clean "%ROOT%\JOSYN.Backend.Demo.FakeSessionStarterConsumer.slnx" --configuration Debug

echo.
echo [OK] Clean abgeschlossen.
exit /b 0
