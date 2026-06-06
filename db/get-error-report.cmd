@echo off
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0get-error-report.ps1" %1
pause
