# Script kiểm tra và fix tables trên Neon DB
# Script này sẽ giúp verify tables hiện có và hướng dẫn xóa

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
Write-Host "  Verify & Fix Tables on Neon DB        " -ForegroundColor Cyan
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

Write-Host "📋 Hướng dẫn kiểm tra và xóa tables:" -ForegroundColor Yellow
Write-Host ""
Write-Host "1️⃣  KIỂM TRA TABLES HIỆN CÓ:" -ForegroundColor Cyan
Write-Host "   Vào Neon Console -> SQL Editor" -ForegroundColor White
Write-Host "   Chạy script từ file: CHECK-TABLES.sql" -ForegroundColor White
Write-Host ""
Write-Host "2️⃣  XÓA TẤT CẢ TABLES (nếu vẫn còn):" -ForegroundColor Cyan
Write-Host "   Vào Neon Console -> SQL Editor" -ForegroundColor White
Write-Host "   Chạy script từ file: DELETE-ALL-TABLES.sql" -ForegroundColor White
Write-Host "   Script này sẽ:" -ForegroundColor Gray
Write-Host "     - Xóa tất cả tables" -ForegroundColor Gray
Write-Host "     - Xóa tất cả sequences" -ForegroundColor Gray
Write-Host "     - Xóa tất cả views" -ForegroundColor Gray
Write-Host "     - Reset schema public" -ForegroundColor Gray
Write-Host ""
Write-Host "3️⃣  SAU KHI XÓA, CHẠY MIGRATION LẠI:" -ForegroundColor Cyan
Write-Host "   dotnet ef database update --project Products_Management.csproj" -ForegroundColor White
Write-Host ""

# Hiển thị nội dung scripts
Write-Host "════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  CHECK-TABLES.sql                      " -ForegroundColor Cyan
Write-Host "════════════════════════════════════════" -ForegroundColor Cyan
Get-Content (Join-Path $ProjectPath "CHECK-TABLES.sql") | Write-Host

Write-Host ""
Write-Host "════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "  DELETE-ALL-TABLES.sql                 " -ForegroundColor Cyan
Write-Host "════════════════════════════════════════" -ForegroundColor Cyan
Get-Content (Join-Path $ProjectPath "DELETE-ALL-TABLES.sql") | Write-Host

Write-Host ""
Write-Host "💡 Lưu ý:" -ForegroundColor Yellow
Write-Host "   - Script DELETE-ALL-TABLES.sql sẽ XÓA TẤT CẢ data!" -ForegroundColor White
Write-Host "   - Sau khi xóa, chạy migration lại" -ForegroundColor White
Write-Host "   - Kiểm tra bằng CHECK-TABLES.sql trước và sau khi xóa" -ForegroundColor White

