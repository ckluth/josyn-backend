@echo off
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0get-session-report.ps1" %1
pause
