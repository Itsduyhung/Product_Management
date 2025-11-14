# Script PowerShell đơn giản - Chỉ cần cập nhật Connection String ở đây
# Usage: Chỉ cần chạy .\run-migration-simple.ps1

# ============================================
# ⚙️ CẤU HÌNH - Cập nhật connection string Neon của bạn ở đây
# ============================================
# Lấy connection string từ Neon Console: https://console.neon.tech
# Format: Host=ep-xxx.us-east-2.aws.neon.tech;Database=neondb;Username=user;Password=pass;SSL Mode=Require;
$NeonConnectionString = "Host=YOUR_HOST.neon.tech;Database=YOUR_DATABASE;Username=YOUR_USERNAME;Password=YOUR_PASSWORD;SSL Mode=Require;"

# ============================================
# Không cần sửa phần dưới đây
# ============================================

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  EF Core Migration - Neon Database     " -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Kiểm tra connection string đã được cấu hình chưa
if ($NeonConnectionString -like "*YOUR_*") {
    Write-Host "❌ Lỗi: Bạn chưa cấu hình Neon Connection String!" -ForegroundColor Red
    Write-Host ""
    Write-Host "📝 Vui lòng mở file run-migration-simple.ps1 và cập nhật:" -ForegroundColor Yellow
    Write-Host "   `$NeonConnectionString = `"Host=ep-xxx.neon.tech;Database=neondb;Username=user;Password=pass;SSL Mode=Require;`"" -ForegroundColor White
    Write-Host ""
    exit 1
}

# Đường dẫn project
$ProjectPath = $PSScriptRoot
$ProjectFile = Join-Path $ProjectPath "Products_Management.csproj"

if (-not (Test-Path $ProjectFile)) {
    Write-Host "❌ Không tìm thấy file project!" -ForegroundColor Red
    exit 1
}

# Kiểm tra dotnet ef tool
Write-Host "🔍 Kiểm tra dotnet ef tool..." -ForegroundColor Yellow
$null = dotnet ef --version 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "📦 Đang cài đặt dotnet ef tool..." -ForegroundColor Yellow
    dotnet tool install --global dotnet-ef
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌ Không thể cài đặt dotnet ef tool" -ForegroundColor Red
        exit 1
    }
}

Write-Host "✅ dotnet ef tool đã sẵn sàng" -ForegroundColor Green
Write-Host ""

# Hiển thị migrations hiện có
Write-Host "📋 Danh sách migrations:" -ForegroundColor Cyan
dotnet ef migrations list --project $ProjectFile --connection $NeonConnectionString
Write-Host ""

# Xác nhận
Write-Host "⚠️  Sẵn sàng apply migrations lên Neon Database?" -ForegroundColor Yellow
$confirm = Read-Host "Nhập 'yes' để tiếp tục"

if ($confirm -ne "yes") {
    Write-Host "❌ Đã hủy" -ForegroundColor Red
    exit 0
}

Write-Host ""
Write-Host "🔄 Đang apply migrations..." -ForegroundColor Yellow
Write-Host ""

# Chạy migration
dotnet ef database update --project $ProjectFile --connection $NeonConnectionString

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "  ✅ Migration thành công!              " -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "🎉 Database trên Neon đã được cập nhật!" -ForegroundColor Green
} else {
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Red
    Write-Host "  ❌ Migration thất bại!                " -ForegroundColor Red
    Write-Host "========================================" -ForegroundColor Red
    Write-Host ""
    Write-Host "💡 Vui lòng kiểm tra:" -ForegroundColor Yellow
    Write-Host "   - Connection string có đúng không?" -ForegroundColor White
    Write-Host "   - Database có tồn tại trên Neon không?" -ForegroundColor White
    Write-Host "   - Có kết nối internet không?" -ForegroundColor White
    exit 1
}

Write-Host ""
Write-Host "📊 Kiểm tra lại migrations:" -ForegroundColor Cyan
dotnet ef migrations list --project $ProjectFile --connection $NeonConnectionString

Write-Host ""
Write-Host "Done!" -ForegroundColor Green

