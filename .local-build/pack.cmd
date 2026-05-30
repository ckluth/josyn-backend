@echo off
CHCP 1252
cd /d "%~dp0.."

dotnet pack JOSYN.Backend.SessionStarter --output "..\local-packages"
if %ERRORLEVEL% neq 0 (
    echo [FEHLER] Pack JOSYN.Backend.SessionStarter fehlgeschlagen.
    exit /b %ERRORLEVEL%
)

echo.
echo [OK] Paket erfolgreich gepackt.
REM pause
