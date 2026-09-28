@echo off
title Paint Tint Launcher
echo ========================================================
echo   Launching Paint Tint API & WPF Workstation
echo ========================================================

echo [1/2] Starting ASP.NET Core Web API on http://localhost:5000...
start "PaintTint API Server" cmd /k "dotnet run --project src/PaintTint.Api/PaintTint.Api.csproj --launch-profile http"

echo Waiting for API server to initialize...
timeout /t 3 /nobreak >nul

echo [2/2] Starting WPF Desktop Workstation...
start "PaintTint WPF Workstation" cmd /c "dotnet run --project src/PaintTint.Wpf/PaintTint.Wpf.csproj"

echo ========================================================
echo   Both applications are running!
echo ========================================================
