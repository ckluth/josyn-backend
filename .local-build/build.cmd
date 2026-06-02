@echo off
CHCP 1252
setlocal

:: -------------------------------------------------------
:: Aufruf:  build.cmd [Release|Debug]
:: Default: Release
:: -------------------------------------------------------
set "CONFIGURATION=%~1"
if not defined CONFIGURATION set "CONFIGURATION=Release"

:: Nur bekannte Werte erlauben
if /i "%CONFIGURATION%" neq "Release" if /i "%CONFIGURATION%" neq "Debug" (
    echo [FEHLER] Unbekannte Konfiguration: "%CONFIGURATION%"
    echo          Erlaubt: Release, Debug
    exit /b 1
)

set "ROOT=%~dp0.."

echo [INFO] Starte dotnet build --configuration %CONFIGURATION% ...
echo.

echo === JOSYN.Jap.JAPServer ===
dotnet build "%ROOT%\josyn-backend-jap-server\JOSYN.Jap.JAPServer.slnx" --configuration %CONFIGURATION%
if %ERRORLEVEL% neq 0 (
    echo.
    echo [FEHLER] Build fehlgeschlagen: JOSYN.Jap.JAPServer. Exit-Code: %ERRORLEVEL%
    exit /b %ERRORLEVEL%
)

echo.
echo [OK] Build erfolgreich abgeschlossen ^(%CONFIGURATION%^).
exit /b 0

