@echo off
CHCP 1252
setlocal

set "NUGET_BASE=%USERPROFILE%\.nuget\packages"
set "PACKAGE=josyn.backend.gateway"

if exist "%NUGET_BASE%\%PACKAGE%" (
    echo Loesche: %NUGET_BASE%\%PACKAGE%
    rd /s /q "%NUGET_BASE%\%PACKAGE%"
    if errorlevel 1 ( echo   FEHLER ) else ( echo   OK )
) else (
    echo Nicht gefunden, uebersprungen: %NUGET_BASE%\%PACKAGE%
)

if /i "%~1" neq "NOPAUSE" pause
