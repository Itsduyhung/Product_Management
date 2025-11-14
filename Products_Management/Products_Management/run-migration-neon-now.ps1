# Script chạy migration với Neon connection string
# Script này sẽ chắc chắn dùng connection string từ appsettings.json

param(
    [string]$Environment = "Production"
)

# Fix PATH
$dotnetToolsPath = "$env:USERPROFILE\.dotnet\tools"
if ($env:PATH -notlike "*$dotnetToolsPath*") {
    $env:PATH += ";$dotnetToolsPath"
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Run Migration to Neon DB              " -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$ProjectPath = $PSScriptRoot
Set-Location $ProjectPath

# Set environment (Production sẽ đọc appsettings.json, Development sẽ đọc appsettings.Development.json)
$env:ASPNETCORE_ENVIRONMENT = $Environment

Write-Host "📍 Environment: $Environment" -ForegroundColor Yellow
Write-Host "📍 Project Path: $ProjectPath" -ForegroundColor Yellow
Write-Host ""

# Đọc và hiển thị connection string
$appsettingsPath = Join-Path $ProjectPath "appsettings.json"
if (Test-Path $appsettingsPath) {
    $appsettings = Get-Content $appsettingsPath | ConvertFrom-Json
    $connectionString = $appsettings.ConnectionStrings.DefaultConnection
    Write-Host "✅ Connection String (from appsettings.json):" -ForegroundColor Green
    Write-Host "   $($connectionString -replace 'Password=[^;]+', 'Password=***')" -ForegroundColor Gray
    Write-Host ""
} else {
    Write-Host "❌ Không tìm thấy appsettings.json!" -ForegroundColor Red
    exit 1
}

# Kiểm tra appsettings.Development.json
$devAppsettingsPath = Join-Path $ProjectPath "appsettings.Development.json"
if (Test-Path $devAppsettingsPath) {
    $devAppsettings = Get-Content $devAppsettingsPath | ConvertFrom-Json
    $devConnectionString = $devAppsettings.ConnectionStrings.DefaultConnection
    if ($devConnectionString -like "*localhost*" -or $devConnectionString -like "*127.0.0.1*") {
        Write-Host "⚠️  WARNING: appsettings.Development.json đang dùng localhost!" -ForegroundColor Yellow
        Write-Host "   Development connection: $($devConnectionString -replace 'Password=[^;]+', 'Password=***')" -ForegroundColor Gray
        Write-Host "   Script sẽ dùng Production environment để đọc appsettings.json" -ForegroundColor Yellow
        Write-Host ""
    }
}

Write-Host "🔄 Đang chạy migration..." -ForegroundColor Cyan
Write-Host ""

# Chạy migration
try {
    dotnet ef database update --project Products_Management.csproj --verbose
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "✅ Migration thành công!" -ForegroundColor Green
    } else {
        Write-Host ""
        Write-Host "❌ Migration thất bại!" -ForegroundColor Red
        exit $LASTEXITCODE
    }
} catch {
    Write-Host ""
    Write-Host "❌ Lỗi khi chạy migration: $_" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "💡 Tip: Nếu vẫn gặp lỗi 'Cart already exists', chạy:" -ForegroundColor Yellow
Write-Host "   1. Script DELETE-CART-TABLE.sql trên Neon SQL Editor" -ForegroundColor White
Write-Host "   2. Chạy lại migration" -ForegroundColor White

