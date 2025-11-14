# Script đơn giản nhất - Tự động xử lý mọi thứ
# Chỉ cần cập nhật Neon Connection String ở dòng 7 và chạy script

param(
    [string]$NeonConnectionString = ""
)

# ============================================
# ⚙️ CẤU HÌNH - Cập nhật connection string Neon của bạn ở đây
# ============================================
if ([string]::IsNullOrWhiteSpace($NeonConnectionString)) {
    $NeonConnectionString = "Host=YOUR_HOST.neon.tech;Database=YOUR_DATABASE;Username=YOUR_USERNAME;Password=YOUR_PASSWORD;SSL Mode=Require;"
}

# ============================================
# Không cần sửa phần dưới đây
# ============================================

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Easy Migration Script                 " -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$ProjectPath = $PSScriptRoot
$ProjectFile = Join-Path $ProjectPath "Products_Management.csproj"

# Kiểm tra connection string
if ($NeonConnectionString -like "*YOUR_*") {
    Write-Host "❌ Lỗi: Bạn chưa cấu hình Neon Connection String!" -ForegroundColor Red
    Write-Host ""
    Write-Host "📝 Cách 1: Cập nhật trong script (dòng 7)" -ForegroundColor Yellow
    Write-Host "   `$NeonConnectionString = `"Host=ep-xxx.neon.tech;Database=neondb;Username=user;Password=pass;SSL Mode=Require;`"" -ForegroundColor White
    Write-Host ""
    Write-Host "📝 Cách 2: Truyền qua parameter" -ForegroundColor Yellow
    Write-Host "   .\easy-migrate.ps1 -NeonConnectionString `"Host=ep-xxx.neon.tech;...`"" -ForegroundColor White
    Write-Host ""
    exit 1
}

Write-Host "✅ Neon Connection String đã được cấu hình" -ForegroundColor Green
Write-Host ""

# Kiểm tra dotnet ef
Write-Host "🔍 Kiểm tra dotnet ef..." -ForegroundColor Yellow
$efCheck = dotnet ef --version 2>&1
$useDotnetEf = $true

if ($LASTEXITCODE -ne 0) {
    Write-Host "⚠️  dotnet ef tool chưa được cài đặt" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "💡 Giải pháp:" -ForegroundColor Yellow
    Write-Host "   1. Mở Visual Studio → Package Manager Console" -ForegroundColor White
    Write-Host "   2. Chạy: Add-Migration InitialCreate" -ForegroundColor White
    Write-Host "   3. Chạy: Update-Database -Connection `"$NeonConnectionString`"" -ForegroundColor White
    Write-Host ""
    Write-Host "   Hoặc cài đặt tool thủ công:" -ForegroundColor White
    Write-Host "   dotnet tool install --global dotnet-ef --version 8.0.0" -ForegroundColor White
    Write-Host ""
    
    $continue = Read-Host "Bạn muốn thử cài đặt tool không? (yes/no)"
    if ($continue -eq "yes") {
        Write-Host "📦 Đang cài đặt dotnet ef tool..." -ForegroundColor Yellow
        dotnet tool install --global dotnet-ef --version 8.0.0 2>&1 | Out-Null
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✅ dotnet ef tool đã được cài đặt!" -ForegroundColor Green
            $useDotnetEf = $true
        } else {
            Write-Host "❌ Không thể cài đặt tool" -ForegroundColor Red
            Write-Host "💡 Vui lòng sử dụng Visual Studio Package Manager Console" -ForegroundColor Yellow
            exit 1
        }
    } else {
        Write-Host "❌ Đã hủy" -ForegroundColor Red
        exit 0
    }
} else {
    Write-Host "✅ dotnet ef tool: $efCheck" -ForegroundColor Green
}

Write-Host ""

# Kiểm tra migrations hiện có
Write-Host "📋 Migrations hiện có:" -ForegroundColor Cyan
dotnet ef migrations list --project $ProjectFile --connection $NeonConnectionString 2>&1
Write-Host ""

# Kiểm tra xem có Initial Migration chưa
$hasInitial = Get-ChildItem -Path (Join-Path $ProjectPath "Migrations") -Filter "*.cs" | Where-Object { 
    $_.Name -like "*Initial*" -or $_.Name -like "*InitialCreate*"
}

if (-not $hasInitial) {
    Write-Host "📝 Tạo Initial Migration..." -ForegroundColor Yellow
    Write-Host "   (Migration hiện tại chỉ update cột, cần Initial Migration để tạo toàn bộ tables)" -ForegroundColor Gray
    Write-Host ""
    
    $confirm = Read-Host "Tạo Initial Migration? (yes/no)"
    if ($confirm -eq "yes") {
        dotnet ef migrations add InitialCreate --project $ProjectFile
        if ($LASTEXITCODE -ne 0) {
            Write-Host "❌ Lỗi khi tạo migration!" -ForegroundColor Red
            exit 1
        }
        Write-Host "✅ Initial Migration đã được tạo!" -ForegroundColor Green
        Write-Host ""
    }
}

# Apply migrations
Write-Host "🚀 Apply migrations lên Neon DB..." -ForegroundColor Yellow
Write-Host ""

$confirm = Read-Host "Sẵn sàng apply migrations? (yes/no)"
if ($confirm -ne "yes") {
    Write-Host "❌ Đã hủy" -ForegroundColor Red
    exit 0
}

Write-Host ""
Write-Host "🔄 Đang apply migrations..." -ForegroundColor Yellow
Write-Host ""

dotnet ef database update --project $ProjectFile --connection $NeonConnectionString

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "  ✅ Migration thành công!              " -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "🎉 Database trên Neon đã được cập nhật!" -ForegroundColor Green
    Write-Host ""
    Write-Host "📊 Kiểm tra migrations:" -ForegroundColor Cyan
    dotnet ef migrations list --project $ProjectFile --connection $NeonConnectionString
    Write-Host ""
    Write-Host "💡 Kiểm tra trên Neon Console để xem các tables đã được tạo!" -ForegroundColor Yellow
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
Write-Host "✅ Hoàn tất!" -ForegroundColor Green

