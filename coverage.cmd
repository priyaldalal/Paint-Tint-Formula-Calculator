@echo off
title Paint Tint Code Coverage Generator
echo ========================================================
echo   Running Unit Tests & Generating Code Coverage Report
echo ========================================================

echo [1/3] Building solution and test project...
dotnet build tests/PaintTint.Tests/PaintTint.Tests.csproj -c Debug

echo [2/3] Executing 31 unit tests with XPlat Cobertura collection...
dotnet test tests/PaintTint.Tests/PaintTint.Tests.csproj --no-build --collect:"XPlat Code Coverage"

echo [3/3] Generating HTML, Markdown, and Text Coverage Reports...
reportgenerator -reports:"tests/PaintTint.Tests/TestResults/**/coverage.cobertura.xml" -targetdir:"coverage-report" -reporttypes:"Html;MarkdownSummary;TextSummary" "-classfilters:-*Migration*;-*ModelSnapshot*;-*Program*"

echo ========================================================
echo   Coverage Report generated in 'coverage-report\index.html'!
echo ========================================================
start "" "%~dp0coverage-report\index.html"
