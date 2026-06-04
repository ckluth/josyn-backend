@echo off
CHCP 1252
setlocal

set "ROOT=%~dp0.."

call :run_clean "josyn-backend-session-store"
if %ERRORLEVEL% neq 0 exit /b %ERRORLEVEL%

call :run_clean "josyn-backend-global-config"
if %ERRORLEVEL% neq 0 exit /b %ERRORLEVEL%

call :run_clean "josyn-backend-session-starter"
if %ERRORLEVEL% neq 0 exit /b %ERRORLEVEL%

call :run_clean "josyn-backend-error-handler"
if %ERRORLEVEL% neq 0 exit /b %ERRORLEVEL%

call :run_clean "josyn-backend-jap-server"
if %ERRORLEVEL% neq 0 exit /b %ERRORLEVEL%

call :run_clean "josyn-backend-listener"
if %ERRORLEVEL% neq 0 exit /b %ERRORLEVEL%

call :run_clean "josyn-backend-ticker"
if %ERRORLEVEL% neq 0 exit /b %ERRORLEVEL%

call :run_clean "josyn-backend-cli"
if %ERRORLEVEL% neq 0 exit /b %ERRORLEVEL%

call :run_clean "josyn-backend-demo"
if %ERRORLEVEL% neq 0 exit /b %ERRORLEVEL%

echo.
echo [OK] NuGet-Cache bereinigt.
exit /b 0

:run_clean
echo.
echo ======================================================
echo  %~1
echo ======================================================
call "%ROOT%\%~1\.local-build\clean.cmd" NOPAUSE
exit /b %ERRORLEVEL%
