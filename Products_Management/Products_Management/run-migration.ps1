# Script PowerShell để chạy EF Core Migrations lên Neon Database
# Usage: .\run-migration.ps1 [connection_string]

param(
    [string]$ConnectionString = ""
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  EF Core Migration Script for Neon DB  " -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Đường dẫn đến project
$ProjectPath = $PSScriptRoot
$ProjectFile = Join-Path $ProjectPath "Products_Management.csproj"

# Kiểm tra file project có tồn tại không
if (-not (Test-Path $ProjectFile)) {
    Write-Host "❌ Error: Không tìm thấy file project tại $ProjectFile" -ForegroundColor Red
    exit 1
}

Write-Host "✅ Project file found: $ProjectFile" -ForegroundColor Green
Write-Host ""

# Kiểm tra dotnet ef tool
Write-Host "🔍 Checking dotnet ef tool..." -ForegroundColor Yellow
$efTool = dotnet ef --version 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ dotnet ef tool chưa được cài đặt!" -ForegroundColor Red
    Write-Host "📦 Đang cài đặt dotnet ef tool..." -ForegroundColor Yellow
    dotnet tool install --global dotnet-ef
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Không thể cài đặt dotnet ef tool" -ForegroundColor Red
        exit 1
    }
    Write-Host "✅ dotnet ef tool đã được cài đặt!" -ForegroundColor Green
} else {
    Write-Host "✅ dotnet ef tool đã được cài đặt: $efTool" -ForegroundColor Green
}
Write-Host ""

# Nếu không có connection string từ parameter, hỏi user
if ([string]::IsNullOrWhiteSpace($ConnectionString)) {
    Write-Host "📋 Chọn cách cung cấp connection string:" -ForegroundColor Yellow
    Write-Host "   1. Sử dụng connection string từ appsettings.json" -ForegroundColor White
    Write-Host "   2. Nhập connection string Neon thủ công" -ForegroundColor White
    Write-Host ""
    $choice = Read-Host "Nhập lựa chọn (1 hoặc 2)"
    
    if ($choice -eq "2") {
        Write-Host ""
        Write-Host "🔐 Nhập Neon Connection String:" -ForegroundColor Yellow
        Write-Host "   Format: Host=ep-xxx.neon.tech;Database=neondb;Username=user;Password=pass;SSL Mode=Require;" -ForegroundColor Gray
        Write-Host ""
        $ConnectionString = Read-Host "Connection String"
        
        if ([string]::IsNullOrWhiteSpace($ConnectionString)) {
            Write-Host "❌ Connection string không được để trống!" -ForegroundColor Red
            exit 1
        }
    } else {
        Write-Host ""
        Write-Host "📖 Đang đọc connection string từ appsettings.json..." -ForegroundColor Yellow
        $appsettingsPath = Join-Path $ProjectPath "appsettings.json"
        
        if (Test-Path $appsettingsPath) {
            $appsettings = Get-Content $appsettingsPath | ConvertFrom-Json
            $ConnectionString = $appsettings.ConnectionStrings.DefaultConnection
            
            if ([string]::IsNullOrWhiteSpace($ConnectionString)) {
                Write-Host "❌ Không tìm thấy DefaultConnection trong appsettings.json" -ForegroundColor Red
                Write-Host "💡 Hãy cập nhật appsettings.json hoặc chọn option 2 để nhập thủ công" -ForegroundColor Yellow
                exit 1
            }
            
            Write-Host "✅ Đã đọc connection string từ appsettings.json" -ForegroundColor Green
            Write-Host "   (Connection string đã được ẩn vì lý do bảo mật)" -ForegroundColor Gray
        } else {
            Write-Host "❌ Không tìm thấy appsettings.json" -ForegroundColor Red
            exit 1
        }
    }
} else {
    Write-Host "✅ Sử dụng connection string từ parameter" -ForegroundColor Green
}

Write-Host ""
Write-Host "🚀 Bắt đầu chạy migration..." -ForegroundColor Yellow
Write-Host ""

# Liệt kê migrations hiện có
Write-Host "📋 Danh sách migrations:" -ForegroundColor Cyan
if ([string]::IsNullOrWhiteSpace($ConnectionString)) {
    dotnet ef migrations list --project $ProjectFile
} else {
    dotnet ef migrations list --project $ProjectFile --connection $ConnectionString
}
Write-Host ""

# Hỏi xác nhận trước khi chạy migration
Write-Host "⚠️  Bạn có chắc chắn muốn apply migrations lên database?" -ForegroundColor Yellow
$confirm = Read-Host "Nhập 'yes' để tiếp tục hoặc 'no' để hủy"

if ($confirm -ne "yes") {
    Write-Host "❌ Migration đã bị hủy" -ForegroundColor Red
    exit 0
}

Write-Host ""
Write-Host "🔄 Đang apply migrations..." -ForegroundColor Yellow
Write-Host ""

# Chạy migration
if ([string]::IsNullOrWhiteSpace($ConnectionString)) {
    $result = dotnet ef database update --project $ProjectFile
} else {
    $result = dotnet ef database update --project $ProjectFile --connection $ConnectionString
}

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "  ✅ Migration thành công!              " -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "🎉 Database đã được cập nhật với tất cả migrations!" -ForegroundColor Green
} else {
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Red
    Write-Host "  ❌ Migration thất bại!                " -ForegroundColor Red
    Write-Host "========================================" -ForegroundColor Red
    Write-Host ""
    Write-Host "💡 Kiểm tra lại:" -ForegroundColor Yellow
    Write-Host "   - Connection string có đúng không?" -ForegroundColor White
    Write-Host "   - Database có tồn tại không?" -ForegroundColor White
    Write-Host "   - Network có kết nối được không?" -ForegroundColor White
    exit 1
}

Write-Host ""
Write-Host "📊 Kiểm tra migrations đã được apply:" -ForegroundColor Cyan
if ([string]::IsNullOrWhiteSpace($ConnectionString)) {
    dotnet ef migrations list --project $ProjectFile
} else {
    dotnet ef migrations list --project $ProjectFile --connection $ConnectionString
}

Write-Host ""
Write-Host "✅ Hoàn tất!" -ForegroundColor Green

