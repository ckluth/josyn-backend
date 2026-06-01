@echo off
CHCP 1252
call "%~dp0build.cmd" Debug
rem pause
exit /b %ERRORLEVEL%
