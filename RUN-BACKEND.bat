@echo off
cd /d "%~dp0backend\RealTimeRailwayTrafficMonitoring.Api"
dotnet restore
dotnet run
pause
