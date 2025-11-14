# Script chạy migration với Neon - Dễ sử dụng nhất
# Chỉ cần nhập connection string khi được hỏi

param(
    [string]$NeonConnectionString = ""
)

# Fix PATH
$dotnetToolsPath = "$env:USERPROFILE\.dotnet\tools"
if ($env:PATH -notlike "*$dotnetToolsPath*") {
    $env:PATH += ";$dotnetToolsPath"
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Apply Migration to Neon Database      " -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Kiểm tra dotnet ef
$efCheck = dotnet ef --version 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ dotnet ef tool không khả dụng!" -ForegroundColor Red
    Write-Host "💡 Chạy: `$env:PATH += `";C:\Users\duyhu\.dotnet\tools`"" -ForegroundColor Yellow
    exit 1
}

Write-Host "✅ dotnet ef tool: $efCheck" -ForegroundColor Green
Write-Host ""

# Lấy connection string
if ([string]::IsNullOrWhiteSpace($NeonConnectionString)) {
    Write-Host "📋 Nhập Neon Connection String:" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "   Format 1 (PostgreSQL URL):" -ForegroundColor Gray
    Write-Host "   postgresql://user:pass@ep-xxx.us-east-2.aws.neon.tech/neondb?sslmode=require" -ForegroundColor White
    Write-Host ""
    Write-Host "   Format 2 (EF Core - Khuyến nghị):" -ForegroundColor Gray
    Write-Host "   Host=ep-xxx.us-east-2.aws.neon.tech;Database=neondb;Username=user;Password=pass;SSL Mode=Require;" -ForegroundColor White
    Write-Host ""
    Write-Host "   Lấy từ: https://console.neon.tech -> Chọn project -> Connection Details" -ForegroundColor Cyan
    Write-Host ""
    $NeonConnectionString = Read-Host "Connection String"
    
    # Convert PostgreSQL URL format sang EF Core format nếu cần
    if ($NeonConnectionString -like "postgresql://*") {
        Write-Host ""
        Write-Host "🔄 Đang convert PostgreSQL URL sang EF Core format..." -ForegroundColor Yellow
        
        # Parse PostgreSQL URL
        $uri = [System.Uri]$NeonConnectionString
        $host = $uri.Host
        $db = $uri.AbsolutePath.TrimStart('/')
        $user = $uri.UserInfo.Split(':')[0]
        $pass = $uri.UserInfo.Split(':')[1]
        
        $NeonConnectionString = "Host=$host;Database=$db;Username=$user;Password=$pass;SSL Mode=Require;"
        
        Write-Host "✅ Đã convert sang EF Core format" -ForegroundColor Green
        Write-Host "   $NeonConnectionString" -ForegroundColor Gray
    }
    
    if ([string]::IsNullOrWhiteSpace($NeonConnectionString) -or $NeonConnectionString -like "*xxx*" -or $NeonConnectionString -like "*YOUR_*") {
        Write-Host ""
        Write-Host "❌ Connection string không hợp lệ!" -ForegroundColor Red
        Write-Host "💡 Vui lòng nhập connection string thật từ Neon Console" -ForegroundColor Yellow
        exit 1
    }
} else {
    Write-Host "✅ Đã có connection string từ parameter" -ForegroundColor Green
}

Write-Host ""
Write-Host "🔍 Đang kiểm tra kết nối..." -ForegroundColor Yellow

# Kiểm tra migrations hiện có
Write-Host "📋 Migrations hiện có:" -ForegroundColor Cyan
dotnet ef migrations list --project Products_Management.csproj --connection $NeonConnectionString 2>&1

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "⚠️  Có vấn đề khi kết nối database!" -ForegroundColor Yellow
    Write-Host "💡 Kiểm tra lại:" -ForegroundColor Yellow
    Write-Host "   - Connection string có đúng không?" -ForegroundColor White
    Write-Host "   - Database có tồn tại trên Neon không?" -ForegroundColor White
    Write-Host "   - Có kết nối internet không?" -ForegroundColor White
    Write-Host ""
    $continue = Read-Host "Bạn có muốn tiếp tục thử apply migration? (yes/no)"
    if ($continue -ne "yes") {
        exit 1
    }
}

Write-Host ""
Write-Host "🚀 Sẵn sàng apply migrations lên Neon Database?" -ForegroundColor Yellow
$confirm = Read-Host "Nhập 'yes' để tiếp tục"

if ($confirm -ne "yes") {
    Write-Host "❌ Đã hủy" -ForegroundColor Red
    exit 0
}

Write-Host ""
Write-Host "🔄 Đang apply migrations..." -ForegroundColor Yellow
Write-Host ""

dotnet ef database update --project Products_Management.csproj --connection $NeonConnectionString

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "  ✅ Migration thành công!              " -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "🎉 Database trên Neon đã được cập nhật!" -ForegroundColor Green
    Write-Host ""
    Write-Host "📊 Kiểm tra migrations đã được apply:" -ForegroundColor Cyan
    dotnet ef migrations list --project Products_Management.csproj --connection $NeonConnectionString
    Write-Host ""
    Write-Host "💡 Kiểm tra trên Neon Console để xem các tables đã được tạo!" -ForegroundColor Yellow
    Write-Host "   https://console.neon.tech -> SQL Editor" -ForegroundColor Cyan
} else {
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Red
    Write-Host "  ❌ Migration thất bại!                " -ForegroundColor Red
    Write-Host "========================================" -ForegroundColor Red
    Write-Host ""
    Write-Host "💡 Vui lòng kiểm tra:" -ForegroundColor Yellow
    Write-Host "   - Connection string có đúng không? (Host, Database, Username, Password)" -ForegroundColor White
    Write-Host "   - Database có tồn tại trên Neon không?" -ForegroundColor White
    Write-Host "   - Có kết nối internet không?" -ForegroundColor White
    Write-Host "   - Connection string có 'SSL Mode=Require;' ở cuối không?" -ForegroundColor White
    exit 1
}

Write-Host ""
Write-Host "Done!" -ForegroundColor Green

