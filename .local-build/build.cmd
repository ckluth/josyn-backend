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

echo === JOSYN.Backend.Contracts ===
dotnet build "%ROOT%\josyn-backend-contracts\JOSYN.Backend.Contracts.slnx" --configuration %CONFIGURATION%
if %ERRORLEVEL% neq 0 (
    echo.
    echo [FEHLER] Build fehlgeschlagen: JOSYN.Backend.Contracts. Exit-Code: %ERRORLEVEL%
    exit /b %ERRORLEVEL%
)

echo === JOSYN.Backend.SessionStore ===
dotnet build "%ROOT%\josyn-backend-session-store\JOSYN.Backend.SessionStore.slnx" --configuration %CONFIGURATION%
if %ERRORLEVEL% neq 0 (
    echo.
    echo [FEHLER] Build fehlgeschlagen: JOSYN.Backend.SessionStore. Exit-Code: %ERRORLEVEL%
    exit /b %ERRORLEVEL%
)

echo === JOSYN.Backend.ConfigStore ===
dotnet build "%ROOT%\josyn-backend-config-store\JOSYN.Backend.ConfigStore.slnx" --configuration %CONFIGURATION%
if %ERRORLEVEL% neq 0 (
    echo.
    echo [FEHLER] Build fehlgeschlagen: JOSYN.Backend.ConfigStore. Exit-Code: %ERRORLEVEL%
    exit /b %ERRORLEVEL%
)

echo === JOSYN.Backend.BootstrapConfig ===
dotnet build "%ROOT%\josyn-backend-bootstrap-config\JOSYN.Backend.BootstrapConfig.slnx" --configuration %CONFIGURATION%
if %ERRORLEVEL% neq 0 (
    echo.
    echo [FEHLER] Build fehlgeschlagen: JOSYN.Backend.BootstrapConfig. Exit-Code: %ERRORLEVEL%
    exit /b %ERRORLEVEL%
)

echo === JOSYN.Backend.ErrorHandler ===
dotnet build "%ROOT%\josyn-backend-error-handler\JOSYN.Backend.ErrorHandler.slnx" --configuration %CONFIGURATION%
if %ERRORLEVEL% neq 0 (
    echo.
    echo [FEHLER] Build fehlgeschlagen: JOSYN.Backend.ErrorHandler. Exit-Code: %ERRORLEVEL%
    exit /b %ERRORLEVEL%
)

echo === JOSYN.Backend.JobRegistry ===
dotnet build "%ROOT%\josyn-backend-job-registry\JOSYN.Backend.JobRegistry.slnx" --configuration %CONFIGURATION%
if %ERRORLEVEL% neq 0 (
    echo.
    echo [FEHLER] Build fehlgeschlagen: JOSYN.Backend.JobRegistry. Exit-Code: %ERRORLEVEL%
    exit /b %ERRORLEVEL%
)

echo === JOSYN.Backend.Listener ===
dotnet build "%ROOT%\josyn-backend-listener\JOSYN.Backend.Listener.slnx" --configuration %CONFIGURATION%
if %ERRORLEVEL% neq 0 (
    echo.
    echo [FEHLER] Build fehlgeschlagen: JOSYN.Backend.Listener. Exit-Code: %ERRORLEVEL%
    exit /b %ERRORLEVEL%
)

echo === JOSYN.Backend.Ticker ===
dotnet build "%ROOT%\josyn-backend-ticker\JOSYN.Backend.Ticker.slnx" --configuration %CONFIGURATION%
if %ERRORLEVEL% neq 0 (
    echo.
    echo [FEHLER] Build fehlgeschlagen: JOSYN.Backend.Ticker. Exit-Code: %ERRORLEVEL%
    exit /b %ERRORLEVEL%
)

echo === JOSYN.Backend.CLI ===
dotnet build "%ROOT%\josyn-backend-cli\JOSYN.Backend.CLI.slnx" --configuration %CONFIGURATION%
if %ERRORLEVEL% neq 0 (
    echo.
    echo [FEHLER] Build fehlgeschlagen: JOSYN.Backend.CLI. Exit-Code: %ERRORLEVEL%
    exit /b %ERRORLEVEL%
)

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

