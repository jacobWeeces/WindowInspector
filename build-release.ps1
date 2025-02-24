# Build and package release script for Window Inspector
$ErrorActionPreference = "Stop"

Write-Host "🚀 Building Window Inspector Release..." -ForegroundColor Cyan

# Ensure we're in the right directory
$scriptPath = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $scriptPath

# Clean previous builds
Write-Host "🧹 Cleaning previous builds..." -ForegroundColor Yellow
Remove-Item -Path ".\WindowInspector.App\bin\Release" -Recurse -ErrorAction SilentlyContinue
Remove-Item -Path ".\release" -Recurse -ErrorAction SilentlyContinue

# Restore dependencies
Write-Host "📦 Restoring dependencies..." -ForegroundColor Yellow
dotnet restore

# Build release
Write-Host "🔨 Building release..." -ForegroundColor Yellow
dotnet publish WindowInspector.App `
    --configuration Release `
    --runtime win-x64 `
    --self-contained true `
    --output .\release\WindowInspector `
    /p:PublishSingleFile=true `
    /p:PublishTrimmed=true `
    /p:DebugType=None

# Create ZIP file
Write-Host "📎 Creating ZIP archive..." -ForegroundColor Yellow
$version = "1.0.0" # Update this for each release
$zipFile = ".\release\WindowInspector-v$version.zip"

Compress-Archive `
    -Path ".\release\WindowInspector\*" `
    -DestinationPath $zipFile `
    -Force

Write-Host "✨ Release build complete!" -ForegroundColor Green
Write-Host "Release files are in: $((Resolve-Path ".\release").Path)" -ForegroundColor Cyan
Write-Host "ZIP file created: $zipFile" -ForegroundColor Cyan 