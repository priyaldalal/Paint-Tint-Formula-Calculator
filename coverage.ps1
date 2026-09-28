# Run Unit Tests and Generate Interactive Code Coverage Report
Write-Host "========================================================" -ForegroundColor Cyan
Write-Host "  Paint Tint Code Coverage Generator" -ForegroundColor Cyan
Write-Host "========================================================" -ForegroundColor Cyan

# 1. Build test project
Write-Host "[1/3] Building test project..." -ForegroundColor Yellow
dotnet build tests/PaintTint.Tests/PaintTint.Tests.csproj -c Debug

# 2. Run tests with coverage collection
Write-Host "[2/3] Running tests with code coverage collection..." -ForegroundColor Yellow
dotnet test tests/PaintTint.Tests/PaintTint.Tests.csproj --no-build --collect:"XPlat Code Coverage"

# 3. Generate HTML report
Write-Host "[3/3] Generating HTML report via ReportGenerator..." -ForegroundColor Yellow
reportgenerator -reports:"tests/PaintTint.Tests/TestResults/**/coverage.cobertura.xml" `
                -targetdir:"coverage-report" `
                -reporttypes:"Html;MarkdownSummary;TextSummary" `
                "-classfilters:-*Migration*;-*ModelSnapshot*;-*Program*"

Write-Host "Code coverage report generated successfully!" -ForegroundColor Green
Write-Host "Opening coverage-report\index.html in default browser..." -ForegroundColor Green
Start-Process "coverage-report\index.html"
