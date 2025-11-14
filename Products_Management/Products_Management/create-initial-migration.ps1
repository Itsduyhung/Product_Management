# Script tạo Initial Migration cho tất cả các tables
# Script này sẽ tạo migration mới cho toàn bộ schema

param(
    [string]$MigrationName = "InitialCreate",
    [string]$ConnectionString = ""
)

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Tạo Initial Migration                 " -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$ProjectPath = $PSScriptRoot
$ProjectFile = Join-Path $ProjectPath "Products_Management.csproj"

if (-not (Test-Path $ProjectFile)) {
    Write-Host "❌ Không tìm thấy file project!" -ForegroundColor Red
    exit 1
}

# Kiểm tra dotnet ef tool
Write-Host "🔍 Kiểm tra dotnet ef tool..." -ForegroundColor Yellow
$efCheck = dotnet ef --version 2>&1
$useLocal = $false

if ($LASTEXITCODE -ne 0) {
    Write-Host "📦 Đang cài đặt dotnet ef tool..." -ForegroundColor Yellow
    
    # Thử cài đặt global với version cụ thể
    $result = dotnet tool install --global dotnet-ef --version 8.0.0 2>&1
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "   Thử cài đặt local tool..." -ForegroundColor Yellow
        
        # Tạo tool manifest nếu chưa có
        if (-not (Test-Path (Join-Path $ProjectPath ".config\dotnet-tools.json"))) {
            dotnet new tool-manifest --force
        }
        
        # Cài đặt local
        dotnet tool install dotnet-ef --version 8.0.0
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✅ dotnet ef tool đã được cài đặt local" -ForegroundColor Green
            $useLocal = $true
        } else {
            Write-Host "❌ Không thể cài đặt dotnet ef tool!" -ForegroundColor Red
            Write-Host "💡 Hãy chạy: dotnet add package Microsoft.EntityFrameworkCore.Tools" -ForegroundColor Yellow
            exit 1
        }
    } else {
        Write-Host "✅ dotnet ef tool đã được cài đặt globally" -ForegroundColor Green
    }
} else {
    Write-Host "✅ dotnet ef tool: $efCheck" -ForegroundColor Green
}

Write-Host "✅ dotnet ef tool đã sẵn sàng" -ForegroundColor Green
Write-Host ""

# Tạo migration mới
Write-Host "📝 Đang tạo migration: $MigrationName..." -ForegroundColor Yellow
Write-Host ""

if ($useLocal) {
    if ([string]::IsNullOrWhiteSpace($ConnectionString)) {
        dotnet dotnet-ef migrations add $MigrationName --project $ProjectFile
    } else {
        dotnet dotnet-ef migrations add $MigrationName --project $ProjectFile --connection $ConnectionString
    }
} else {
    if ([string]::IsNullOrWhiteSpace($ConnectionString)) {
        dotnet ef migrations add $MigrationName --project $ProjectFile
    } else {
        dotnet ef migrations add $MigrationName --project $ProjectFile --connection $ConnectionString
    }
}

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "  ✅ Migration đã được tạo!            " -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "📋 Danh sách migrations:" -ForegroundColor Cyan
    if ($useLocal) {
        if ([string]::IsNullOrWhiteSpace($ConnectionString)) {
            dotnet dotnet-ef migrations list --project $ProjectFile
        } else {
            dotnet dotnet-ef migrations list --project $ProjectFile --connection $ConnectionString
        }
    } else {
        if ([string]::IsNullOrWhiteSpace($ConnectionString)) {
            dotnet ef migrations list --project $ProjectFile
        } else {
            dotnet ef migrations list --project $ProjectFile --connection $ConnectionString
        }
    }
    Write-Host ""
    Write-Host "💡 Bước tiếp theo:" -ForegroundColor Yellow
    Write-Host "   1. Kiểm tra migration file trong thư mục Migrations/" -ForegroundColor White
    Write-Host "   2. Chạy run-migration-simple.ps1 để apply migration lên Neon DB" -ForegroundColor White
} else {
    Write-Host ""
    Write-Host "❌ Có lỗi xảy ra khi tạo migration!" -ForegroundColor Red
    exit 1
}

