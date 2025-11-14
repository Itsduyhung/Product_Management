# Script fix lỗi "relation already exists" khi migration
# Kiểm tra và xử lý tables đã tồn tại

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
Write-Host "  Fix Existing Tables Issue             " -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$ProjectPath = $PSScriptRoot
$appsettingsPath = Join-Path $ProjectPath "appsettings.json"

# Đọc connection string
if ([string]::IsNullOrWhiteSpace($NeonConnectionString)) {
    if (Test-Path $appsettingsPath) {
        $appsettings = Get-Content $appsettingsPath | ConvertFrom-Json
        $NeonConnectionString = $appsettings.ConnectionStrings.DefaultConnection
    } else {
        Write-Host "❌ Không tìm thấy appsettings.json!" -ForegroundColor Red
        exit 1
    }
}

Write-Host "✅ Connection string: $($NeonConnectionString -replace 'Password=[^;]+', 'Password=***')" -ForegroundColor Green
Write-Host ""

# Kiểm tra migrations đã apply
Write-Host "📋 Kiểm tra migrations đã được apply trên DB..." -ForegroundColor Yellow
dotnet ef migrations list --project Products_Management.csproj --connection $NeonConnectionString 2>&1
Write-Host ""

# Giải pháp
Write-Host "🔧 Có 2 cách để fix:" -ForegroundColor Yellow
Write-Host ""
Write-Host "Cách 1: Xóa tất cả tables cũ và chạy lại migration từ đầu (MẤT DATA)" -ForegroundColor White
Write-Host "   - An toàn nếu DB chưa có data quan trọng" -ForegroundColor Gray
Write-Host ""
Write-Host "Cách 2: Mark migrations đã apply (giữ nguyên tables hiện có)" -ForegroundColor White
Write-Host "   - Giữ nguyên data hiện có" -ForegroundColor Gray
Write-Host "   - Chỉ đánh dấu migrations đã được apply" -ForegroundColor Gray
Write-Host ""

$choice = Read-Host "Chọn cách (1 hoặc 2)"

if ($choice -eq "1") {
    Write-Host ""
    Write-Host "⚠️  CẢNH BÁO: Tất cả data sẽ bị xóa!" -ForegroundColor Red
    Write-Host ""
    $confirm = Read-Host "Bạn có chắc chắn muốn xóa tất cả tables? (yes/no)"
    
    if ($confirm -eq "yes") {
        Write-Host ""
        Write-Host "🔄 Đang tạo script SQL để xóa tables..." -ForegroundColor Yellow
        
        # Tạo script SQL
        $sqlScript = @"
-- Drop all tables (chạy trên Neon SQL Editor)
DROP SCHEMA public CASCADE;
CREATE SCHEMA public;
GRANT ALL ON SCHEMA public TO postgres;
GRANT ALL ON SCHEMA public TO public;
"@
        
        $sqlPath = Join-Path $ProjectPath "drop-all-tables.sql"
        $sqlScript | Out-File -FilePath $sqlPath -Encoding UTF8
        
        Write-Host "✅ Đã tạo file: drop-all-tables.sql" -ForegroundColor Green
        Write-Host ""
        Write-Host "📝 Các bước tiếp theo:" -ForegroundColor Yellow
        Write-Host "   1. Vào Neon Console: https://console.neon.tech" -ForegroundColor White
        Write-Host "   2. Chọn project -> SQL Editor" -ForegroundColor White
        Write-Host "   3. Copy nội dung file drop-all-tables.sql và chạy" -ForegroundColor White
        Write-Host "   4. Sau đó quay lại và chạy migration:" -ForegroundColor White
        Write-Host "      dotnet ef database update --project Products_Management.csproj" -ForegroundColor Cyan
        Write-Host ""
    } else {
        Write-Host "❌ Đã hủy" -ForegroundColor Red
    }
} else {
    Write-Host ""
    Write-Host "📝 Đang mark migrations đã apply..." -ForegroundColor Yellow
    Write-Host ""
    Write-Host "💡 Cách làm:" -ForegroundColor Yellow
    Write-Host "   1. Vào Neon Console -> SQL Editor" -ForegroundColor White
    Write-Host "   2. Chạy lệnh SQL sau để đánh dấu migrations đã apply:" -ForegroundColor White
    Write-Host ""
    
    $migrationsPath = Join-Path $ProjectPath "Migrations"
    $migrations = Get-ChildItem -Path $migrationsPath -Filter "*_*.cs" | Where-Object { $_.Name -notlike "*.Designer.cs" -and $_.Name -notlike "*ModelSnapshot*" }
    
    $sqlScript = "-- Mark migrations as applied`nINSERT INTO `"__EFMigrationsHistory`" (`"MigrationId`", `"ProductVersion`") VALUES"
    $migrationValues = @()
    
    foreach ($migration in $migrations) {
        $migrationId = $migration.Name -replace '\.cs$', ''
        $migrationValues += "('$migrationId', '9.0.9')"
    }
    
    $sqlScript += "`n" + ($migrationValues -join ",`n") + "`nON CONFLICT (`"MigrationId`") DO NOTHING;"
    
    $sqlPath = Join-Path $ProjectPath "mark-migrations-applied.sql"
    $sqlScript | Out-File -FilePath $sqlPath -Encoding UTF8
    
    Write-Host $sqlScript -ForegroundColor Cyan
    Write-Host ""
    Write-Host "✅ Đã tạo file: mark-migrations-applied.sql" -ForegroundColor Green
    Write-Host ""
    Write-Host "📝 Các bước tiếp theo:" -ForegroundColor Yellow
    Write-Host "   1. Vào Neon Console: https://console.neon.tech" -ForegroundColor White
    Write-Host "   2. Chọn project -> SQL Editor" -ForegroundColor White
    Write-Host "   3. Copy và chạy SQL script từ file mark-migrations-applied.sql" -ForegroundColor White
    Write-Host "   4. Sau đó chạy lại migration:" -ForegroundColor White
    Write-Host "      dotnet ef database update --project Products_Management.csproj" -ForegroundColor Cyan
    Write-Host ""
}

Write-Host "Done!" -ForegroundColor Green

