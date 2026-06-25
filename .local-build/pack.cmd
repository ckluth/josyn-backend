@echo off
CHCP 1252
setlocal

set "ROOT=%~dp0.."

call :run_pack "josyn-backend-contracts"
if %ERRORLEVEL% neq 0 exit /b %ERRORLEVEL%

call :run_pack "josyn-backend-session-store"
if %ERRORLEVEL% neq 0 exit /b %ERRORLEVEL%

call :run_pack "josyn-backend-config-store"
if %ERRORLEVEL% neq 0 exit /b %ERRORLEVEL%

call :run_pack "josyn-backend-bootstrap-config"
if %ERRORLEVEL% neq 0 exit /b %ERRORLEVEL%

call :run_pack "josyn-backend-job-registry"
if %ERRORLEVEL% neq 0 exit /b %ERRORLEVEL%

call :run_pack "josyn-backend-gateway"
if %ERRORLEVEL% neq 0 exit /b %ERRORLEVEL%

call :run_pack "josyn-backend-job-schedule-store"
if %ERRORLEVEL% neq 0 exit /b %ERRORLEVEL%

call :run_pack "josyn-backend-session-launcher"
if %ERRORLEVEL% neq 0 exit /b %ERRORLEVEL%

call :run_pack "josyn-backend-error-handler"
if %ERRORLEVEL% neq 0 exit /b %ERRORLEVEL%

echo.
echo [OK] Pack abgeschlossen.
exit /b 0

:run_pack
echo.
echo ======================================================
echo  %~1
echo ======================================================
call "%ROOT%\%~1\.local-build\pack.cmd"
exit /b %ERRORLEVEL%
