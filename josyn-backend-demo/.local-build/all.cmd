@echo off
CHCP 1252
setlocal

echo [INFO] Running clean...
call "%~dp0clean.cmd"
if %ERRORLEVEL% neq 0 exit /b %ERRORLEVEL%

echo.
echo [INFO] Running build...
call "%~dp0build.cmd"
if %ERRORLEVEL% neq 0 exit /b %ERRORLEVEL%

echo.
echo [OK] All steps completed.
exit /b 0
