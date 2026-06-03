@echo off
CHCP 1252
cd /d "%~dp0.."
dotnet pack --output "..\local-packages"
