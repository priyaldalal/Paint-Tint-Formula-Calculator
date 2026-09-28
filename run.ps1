# Launch both API and WPF Application
Write-Host "========================================================" -ForegroundColor Cyan
Write-Host "  Launching Paint Tint API & WPF Workstation" -ForegroundColor Cyan
Write-Host "========================================================" -ForegroundColor Cyan

# 1. Start Web API
Write-Host "[1/2] Starting ASP.NET Core Web API on http://localhost:5000..." -ForegroundColor Yellow
Start-Process powershell -ArgumentList "-NoExit", "-Command", "dotnet run --project src/PaintTint.Api/PaintTint.Api.csproj --launch-profile http"

# Wait for API to boot and seed database
Write-Host "Waiting 3 seconds for API initialization..." -ForegroundColor Gray
Start-Sleep -Seconds 3

# 2. Start WPF Application
Write-Host "[2/2] Starting WPF Desktop Workstation..." -ForegroundColor Green
Start-Process powershell -ArgumentList "-Command", "dotnet run --project src/PaintTint.Wpf/PaintTint.Wpf.csproj"

Write-Host "Both applications launched successfully!" -ForegroundColor Green
