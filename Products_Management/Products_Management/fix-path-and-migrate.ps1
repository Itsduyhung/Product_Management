# Script fix PATH và chạy migration
# Script này sẽ tự động add PATH và chạy migration

param(
    [string]$NeonConnectionString = ""
)

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Fix PATH & Run Migration              " -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Add dotnet tools vào PATH cho session hiện tại
$dotnetToolsPath = "$env:USERPROFILE\.dotnet\tools"
if ($env:PATH -notlike "*$dotnetToolsPath*") {
    Write-Host "🔧 Đang add dotnet tools vào PATH..." -ForegroundColor Yellow
    $env:PATH += ";$dotnetToolsPath"
    Write-Host "✅ Đã add PATH cho session hiện tại" -ForegroundColor Green
    
    # Add vào PATH vĩnh viễn (cho lần sau)
    $currentPath = [Environment]::GetEnvironmentVariable("PATH", "User")
    if ($currentPath -notlike "*$dotnetToolsPath*") {
        Write-Host "💾 Đang lưu PATH vĩnh viễn..." -ForegroundColor Yellow
        [Environment]::SetEnvironmentVariable("PATH", "$currentPath;$dotnetToolsPath", "User")
        Write-Host "✅ Đã lưu PATH vĩnh viễn (cần restart terminal để có hiệu lực)" -ForegroundColor Green
    }
} else {
    Write-Host "✅ PATH đã được cấu hình" -ForegroundColor Green
}

Write-Host ""

# Kiểm tra dotnet ef
Write-Host "🔍 Kiểm tra dotnet ef..." -ForegroundColor Yellow
$efVersion = dotnet ef --version 2>&1
if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ dotnet ef tool: $efVersion" -ForegroundColor Green
} else {
    Write-Host "❌ dotnet ef tool không khả dụng!" -ForegroundColor Red
    Write-Host "💡 Hãy chạy: dotnet tool install --global dotnet-ef --version 8.0.0" -ForegroundColor Yellow
    exit 1
}

Write-Host ""

# Lấy Neon Connection String
if ([string]::IsNullOrWhiteSpace($NeonConnectionString)) {
    Write-Host "📋 Chọn cách cung cấp connection string:" -ForegroundColor Yellow
    Write-Host "   1. Đọc từ appsettings.json" -ForegroundColor White
    Write-Host "   2. Nhập Neon connection string thủ công" -ForegroundColor White
    Write-Host ""
    $choice = Read-Host "Nhập lựa chọn (1 hoặc 2)"
    
    if ($choice -eq "2") {
        Write-Host ""
        Write-Host "🔐 Nhập Neon Connection String:" -ForegroundColor Yellow
        Write-Host "   Format: Host=ep-xxx.neon.tech;Database=neondb;Username=user;Password=pass;SSL Mode=Require;" -ForegroundColor Gray
        Write-Host ""
        $NeonConnectionString = Read-Host "Connection String"
    } else {
        $appsettingsPath = Join-Path $PSScriptRoot "appsettings.json"
        if (Test-Path $appsettingsPath) {
            $appsettings = Get-Content $appsettingsPath | ConvertFrom-Json
            $NeonConnectionString = $appsettings.ConnectionStrings.DefaultConnection
            
            if ($NeonConnectionString -like "*localhost*") {
                Write-Host "⚠️  Connection string đang trỏ đến localhost!" -ForegroundColor Yellow
                Write-Host "   Bạn có muốn nhập Neon connection string thủ công không?" -ForegroundColor Yellow
                $change = Read-Host "Nhập Neon connection string (hoặc Enter để dùng localhost)"
                if (-not [string]::IsNullOrWhiteSpace($change)) {
                    $NeonConnectionString = $change
                }
            }
            
            Write-Host "✅ Đã đọc connection string từ appsettings.json" -ForegroundColor Green
        } else {
            Write-Host "❌ Không tìm thấy appsettings.json" -ForegroundColor Red
            Write-Host "💡 Vui lòng nhập Neon connection string:" -ForegroundColor Yellow
            $NeonConnectionString = Read-Host "Connection String"
        }
    }
}

Write-Host ""

# Kiểm tra migrations
Write-Host "📋 Migrations hiện có:" -ForegroundColor Cyan
if ([string]::IsNullOrWhiteSpace($NeonConnectionString)) {
    dotnet ef migrations list --project Products_Management.csproj
} else {
    dotnet ef migrations list --project Products_Management.csproj --connection $NeonConnectionString
}
Write-Host ""

# Kiểm tra Initial Migration
$migrationsPath = Join-Path $PSScriptRoot "Migrations"
$hasInitial = Get-ChildItem -Path $migrationsPath -Filter "*Initial*.cs" -ErrorAction SilentlyContinue

if (-not $hasInitial) {
    Write-Host "📝 Tạo Initial Migration..." -ForegroundColor Yellow
    Write-Host "   (Migration hiện tại chỉ update cột, cần Initial Migration để tạo toàn bộ tables)" -ForegroundColor Gray
    Write-Host ""
    
    $confirm = Read-Host "Tạo Initial Migration? (yes/no)"
    if ($confirm -eq "yes") {
        dotnet ef migrations add InitialCreate --project Products_Management.csproj
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✅ Initial Migration đã được tạo!" -ForegroundColor Green
        } else {
            Write-Host "❌ Lỗi khi tạo migration!" -ForegroundColor Red
            exit 1
        }
        Write-Host ""
    }
} else {
    Write-Host "✅ Đã có Initial Migration" -ForegroundColor Green
    Write-Host ""
}

# Apply migrations
Write-Host "🚀 Apply migrations lên database..." -ForegroundColor Yellow
Write-Host ""

$confirm = Read-Host "Sẵn sàng apply migrations? (yes/no)"
if ($confirm -ne "yes") {
    Write-Host "❌ Đã hủy" -ForegroundColor Red
    exit 0
}

Write-Host ""
Write-Host "🔄 Đang apply migrations..." -ForegroundColor Yellow
Write-Host ""

if ([string]::IsNullOrWhiteSpace($NeonConnectionString)) {
    dotnet ef database update --project Products_Management.csproj
} else {
    dotnet ef database update --project Products_Management.csproj --connection $NeonConnectionString
}

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "  ✅ Migration thành công!              " -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "🎉 Database đã được cập nhật!" -ForegroundColor Green
    Write-Host ""
    Write-Host "📊 Kiểm tra migrations:" -ForegroundColor Cyan
    if ([string]::IsNullOrWhiteSpace($NeonConnectionString)) {
        dotnet ef migrations list --project Products_Management.csproj
    } else {
        dotnet ef migrations list --project Products_Management.csproj --connection $NeonConnectionString
    }
} else {
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Red
    Write-Host "  ❌ Migration thất bại!                " -ForegroundColor Red
    Write-Host "========================================" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "✅ Hoàn tất!" -ForegroundColor Green

