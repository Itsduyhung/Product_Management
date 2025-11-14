# Script tổng hợp: Setup dotnet ef và chạy migration
# Script này sẽ tự động xử lý mọi thứ

param(
    [string]$NeonConnectionString = ""
)

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Setup EF Core & Run Migration          " -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$ProjectPath = $PSScriptRoot
$ProjectFile = Join-Path $ProjectPath "Products_Management.csproj"

if (-not (Test-Path $ProjectFile)) {
    Write-Host "❌ Không tìm thấy file project!" -ForegroundColor Red
    exit 1
}

Write-Host "✅ Project file: $ProjectFile" -ForegroundColor Green
Write-Host ""

# Kiểm tra dotnet version
Write-Host "🔍 Kiểm tra dotnet version..." -ForegroundColor Yellow
$dotnetVersion = dotnet --version
Write-Host "   .NET SDK: $dotnetVersion" -ForegroundColor White
Write-Host ""

# Thử cài đặt dotnet ef với version cụ thể
Write-Host "📦 Đang cài đặt/cập nhật dotnet-ef tool..." -ForegroundColor Yellow

# Thử cài đặt với version 8.0 (tương thích với .NET 9)
Write-Host "   Đang thử cài đặt dotnet-ef version 8.0..." -ForegroundColor Gray
$result = dotnet tool install --global dotnet-ef --version 8.0.0 2>&1

if ($LASTEXITCODE -ne 0) {
    Write-Host "   Thử cách khác: cài đặt latest version..." -ForegroundColor Gray
    # Nếu thất bại, thử update
    $result = dotnet tool update --global dotnet-ef 2>&1
    
    if ($LASTEXITCODE -ne 0) {
        # Nếu vẫn thất bại, thử uninstall và install lại
        Write-Host "   Đang uninstall tool cũ..." -ForegroundColor Gray
        dotnet tool uninstall --global dotnet-ef 2>&1 | Out-Null
        
        Write-Host "   Đang cài đặt lại..." -ForegroundColor Gray
        $result = dotnet tool install --global dotnet-ef --version 8.0.0 2>&1
    }
}

# Kiểm tra xem đã cài đặt thành công chưa
$efCheck = dotnet ef --version 2>&1
if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ dotnet ef tool: $efCheck" -ForegroundColor Green
} else {
    Write-Host "⚠️  Không thể cài đặt dotnet ef tool globally" -ForegroundColor Yellow
    Write-Host "   Đang thử cài đặt local tool..." -ForegroundColor Yellow
    
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
        Write-Host ""
        Write-Host "💡 Giải pháp thay thế:" -ForegroundColor Yellow
        Write-Host "   1. Cài đặt thủ công từ Visual Studio:" -ForegroundColor White
        Write-Host "      - Mở Package Manager Console" -ForegroundColor White
        Write-Host "      - Chạy: Install-Package Microsoft.EntityFrameworkCore.Tools" -ForegroundColor White
        Write-Host ""
        Write-Host "   2. Hoặc cài đặt thủ công:" -ForegroundColor White
        Write-Host "      dotnet add package Microsoft.EntityFrameworkCore.Tools" -ForegroundColor White
        exit 1
    }
}

Write-Host ""

# Nếu không có connection string, hỏi user
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
        $appsettingsPath = Join-Path $ProjectPath "appsettings.json"
        if (Test-Path $appsettingsPath) {
            $appsettings = Get-Content $appsettingsPath | ConvertFrom-Json
            $NeonConnectionString = $appsettings.ConnectionStrings.DefaultConnection
            Write-Host "✅ Đã đọc connection string từ appsettings.json" -ForegroundColor Green
        } else {
            Write-Host "❌ Không tìm thấy appsettings.json" -ForegroundColor Red
            exit 1
        }
    }
}

# Kiểm tra connection string có đúng format Neon không
if ($NeonConnectionString -like "*localhost*") {
    Write-Host "⚠️  Cảnh báo: Connection string đang trỏ đến localhost!" -ForegroundColor Yellow
    Write-Host "   Nếu muốn migrate lên Neon, vui lòng cập nhật connection string Neon" -ForegroundColor Yellow
    Write-Host ""
    $continue = Read-Host "Bạn có muốn tiếp tục với localhost? (yes/no)"
    if ($continue -ne "yes") {
        Write-Host "❌ Đã hủy" -ForegroundColor Red
        exit 0
    }
}

Write-Host ""

# Bước 1: Kiểm tra migrations hiện có
Write-Host "📋 Bước 1: Kiểm tra migrations hiện có..." -ForegroundColor Cyan
if ($useLocal) {
    dotnet dotnet-ef migrations list --project $ProjectFile
} else {
    dotnet ef migrations list --project $ProjectFile
}
Write-Host ""

# Bước 2: Tạo Initial Migration nếu chưa có
$migrations = Get-ChildItem -Path (Join-Path $ProjectPath "Migrations") -Filter "*.cs" | Where-Object { $_.Name -like "*Initial*" -or $_.Name -like "*InitialCreate*" }

if (-not $migrations) {
    Write-Host "📝 Bước 2: Tạo Initial Migration..." -ForegroundColor Cyan
    Write-Host "   Migration hiện tại chỉ update columns, cần tạo Initial Migration để tạo toàn bộ tables" -ForegroundColor Yellow
    Write-Host ""
    
    $confirm = Read-Host "Tạo Initial Migration? (yes/no)"
    if ($confirm -eq "yes") {
        if ($useLocal) {
            dotnet dotnet-ef migrations add InitialCreate --project $ProjectFile
        } else {
            dotnet ef migrations add InitialCreate --project $ProjectFile
        }
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✅ Initial Migration đã được tạo!" -ForegroundColor Green
        } else {
            Write-Host "❌ Có lỗi khi tạo migration!" -ForegroundColor Red
            exit 1
        }
    } else {
        Write-Host "⚠️  Đã bỏ qua tạo migration. Có thể migration sẽ thất bại nếu tables chưa tồn tại." -ForegroundColor Yellow
    }
    Write-Host ""
} else {
    Write-Host "✅ Đã có Initial Migration" -ForegroundColor Green
    Write-Host ""
}

# Bước 3: Chạy migration
Write-Host "🚀 Bước 3: Apply migrations lên database..." -ForegroundColor Cyan
Write-Host ""

$confirm = Read-Host "Sẵn sàng apply migrations? (yes/no)"
if ($confirm -ne "yes") {
    Write-Host "❌ Đã hủy" -ForegroundColor Red
    exit 0
}

Write-Host ""
Write-Host "🔄 Đang apply migrations..." -ForegroundColor Yellow
Write-Host ""

if ($useLocal) {
    if ([string]::IsNullOrWhiteSpace($NeonConnectionString)) {
        dotnet dotnet-ef database update --project $ProjectFile
    } else {
        dotnet dotnet-ef database update --project $ProjectFile --connection $NeonConnectionString
    }
} else {
    if ([string]::IsNullOrWhiteSpace($NeonConnectionString)) {
        dotnet ef database update --project $ProjectFile
    } else {
        dotnet ef database update --project $ProjectFile --connection $NeonConnectionString
    }
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
    if ($useLocal) {
        if ([string]::IsNullOrWhiteSpace($NeonConnectionString)) {
            dotnet dotnet-ef migrations list --project $ProjectFile
        } else {
            dotnet dotnet-ef migrations list --project $ProjectFile --connection $NeonConnectionString
        }
    } else {
        if ([string]::IsNullOrWhiteSpace($NeonConnectionString)) {
            dotnet ef migrations list --project $ProjectFile
        } else {
            dotnet ef migrations list --project $ProjectFile --connection $NeonConnectionString
        }
    }
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
Write-Host "✅ Hoàn tất!" -ForegroundColor Green

