# Script chạy migration với connection string từ appsettings.json
# Chỉ cần chạy: .\RUN-MIGRATION-NOW.ps1

# Fix PATH
$dotnetToolsPath = "$env:USERPROFILE\.dotnet\tools"
if ($env:PATH -notlike "*$dotnetToolsPath*") {
    $env:PATH += ";$dotnetToolsPath"
    Write-Host "✅ Đã add PATH cho dotnet tools" -ForegroundColor Green
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Run Migration to Neon DB              " -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$ProjectPath = $PSScriptRoot
$appsettingsPath = Join-Path $ProjectPath "appsettings.json"

# Đọc connection string từ appsettings.json
Write-Host "📖 Đang đọc connection string từ appsettings.json..." -ForegroundColor Yellow

if (Test-Path $appsettingsPath) {
    $appsettings = Get-Content $appsettingsPath | ConvertFrom-Json
    $connectionString = $appsettings.ConnectionStrings.DefaultConnection
    
    Write-Host "✅ Đã đọc connection string từ appsettings.json" -ForegroundColor Green
    Write-Host "   Host: $($connectionString -replace 'Password=[^;]+', 'Password=***')" -ForegroundColor Gray
    Write-Host ""
} else {
    Write-Host "❌ Không tìm thấy appsettings.json!" -ForegroundColor Red
    exit 1
}

# Kiểm tra dotnet ef
Write-Host "🔍 Kiểm tra dotnet ef..." -ForegroundColor Yellow
$efCheck = dotnet ef --version 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ dotnet ef tool không khả dụng!" -ForegroundColor Red
    exit 1
}
Write-Host "✅ dotnet ef tool: $efCheck" -ForegroundColor Green
Write-Host ""

# Build project
Write-Host "🔨 Đang build project..." -ForegroundColor Yellow
dotnet build --no-incremental | Out-Null

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Build thất bại!" -ForegroundColor Red
    Write-Host "💡 Đảm bảo app đã được dừng" -ForegroundColor Yellow
    exit 1
}
Write-Host "✅ Build thành công!" -ForegroundColor Green
Write-Host ""

# Hiển thị migrations
Write-Host "📋 Migrations hiện có:" -ForegroundColor Cyan
dotnet ef migrations list --project Products_Management.csproj --connection $connectionString
Write-Host ""

# Xác nhận
Write-Host "⚠️  Sẵn sàng apply migrations lên Neon Database?" -ForegroundColor Yellow
Write-Host "   Connection: $($connectionString -replace 'Password=[^;]+', 'Password=***')" -ForegroundColor Gray
Write-Host ""
$confirm = Read-Host "Nhập 'yes' để tiếp tục"

if ($confirm -ne "yes") {
    Write-Host "❌ Đã hủy" -ForegroundColor Red
    exit 0
}

Write-Host ""
Write-Host "🔄 Đang apply migrations..." -ForegroundColor Yellow
Write-Host ""

# Chạy migration
dotnet ef database update --project Products_Management.csproj --connection $connectionString

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "  ✅ Migration thành công!              " -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "🎉 Database trên Neon đã được cập nhật!" -ForegroundColor Green
    Write-Host ""
    Write-Host "📊 Migrations đã được apply:" -ForegroundColor Cyan
    dotnet ef migrations list --project Products_Management.csproj --connection $connectionString
    Write-Host ""
    Write-Host "💡 Kiểm tra trên Neon Console:" -ForegroundColor Yellow
    Write-Host "   https://console.neon.tech -> SQL Editor" -ForegroundColor Cyan
    Write-Host "   Sẽ thấy các tables: Cart, CartItem, Order, OrderItem, products, users" -ForegroundColor White
} else {
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Red
    Write-Host "  ❌ Migration thất bại!                " -ForegroundColor Red
    Write-Host "========================================" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Done!" -ForegroundColor Green

