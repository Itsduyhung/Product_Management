# Script dừng app đang chạy và chạy migration
# Script này sẽ tự động kill process app nếu đang chạy, sau đó chạy migration

param(
    [string]$NeonConnectionString = ""
)

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Stop App & Run Migration              " -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Add PATH cho dotnet tools
$dotnetToolsPath = "$env:USERPROFILE\.dotnet\tools"
if ($env:PATH -notlike "*$dotnetToolsPath*") {
    $env:PATH += ";$dotnetToolsPath"
}

# Bước 1: Tìm và kill process Products_Management đang chạy
Write-Host "🔍 Bước 1: Kiểm tra app đang chạy..." -ForegroundColor Yellow
$processes = Get-Process | Where-Object { $_.ProcessName -like "*Products_Management*" -or $_.ProcessName -like "*Products*Management*" }

if ($processes) {
    Write-Host "⚠️  Tìm thấy $($processes.Count) process đang chạy:" -ForegroundColor Yellow
    foreach ($proc in $processes) {
        Write-Host "   - Process: $($proc.ProcessName) (PID: $($proc.Id))" -ForegroundColor White
    }
    Write-Host ""
    
    $confirm = Read-Host "Bạn có muốn dừng các process này để chạy migration? (yes/no)"
    if ($confirm -eq "yes") {
        foreach ($proc in $processes) {
            try {
                Stop-Process -Id $proc.Id -Force
                Write-Host "✅ Đã dừng process $($proc.ProcessName) (PID: $($proc.Id))" -ForegroundColor Green
            } catch {
                Write-Host "⚠️  Không thể dừng process $($proc.Id): $_" -ForegroundColor Yellow
            }
        }
        Write-Host ""
        Start-Sleep -Seconds 2
    } else {
        Write-Host "❌ Đã hủy. Vui lòng dừng app thủ công rồi thử lại." -ForegroundColor Red
        exit 1
    }
} else {
    Write-Host "✅ Không có app nào đang chạy" -ForegroundColor Green
}

Write-Host ""

# Bước 2: Kiểm tra dotnet ef
Write-Host "🔍 Bước 2: Kiểm tra dotnet ef..." -ForegroundColor Yellow
$efCheck = dotnet ef --version 2>&1
if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ dotnet ef tool: $efCheck" -ForegroundColor Green
} else {
    Write-Host "❌ dotnet ef tool không khả dụng!" -ForegroundColor Red
    Write-Host "💡 Hãy chạy: `$env:PATH += `";C:\Users\duyhu\.dotnet\tools`"" -ForegroundColor Yellow
    exit 1
}

Write-Host ""

# Bước 3: Build project
Write-Host "🔨 Bước 3: Build project..." -ForegroundColor Yellow
dotnet build --no-incremental

if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Build thất bại!" -ForegroundColor Red
    Write-Host "💡 Đảm bảo app đã được dừng hoàn toàn" -ForegroundColor Yellow
    exit 1
}

Write-Host "✅ Build thành công!" -ForegroundColor Green
Write-Host ""

# Bước 4: Lấy Neon Connection String
if ([string]::IsNullOrWhiteSpace($NeonConnectionString)) {
    Write-Host "📋 Bước 4: Cấu hình connection string..." -ForegroundColor Yellow
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
        } else {
            Write-Host "❌ Không tìm thấy appsettings.json" -ForegroundColor Red
            Write-Host "💡 Vui lòng nhập Neon connection string:" -ForegroundColor Yellow
            $NeonConnectionString = Read-Host "Connection String"
        }
    }
}

Write-Host ""

# Bước 5: Kiểm tra migrations
Write-Host "📋 Bước 5: Kiểm tra migrations hiện có..." -ForegroundColor Yellow
if ([string]::IsNullOrWhiteSpace($NeonConnectionString)) {
    dotnet ef migrations list --project Products_Management.csproj 2>&1
} else {
    dotnet ef migrations list --project Products_Management.csproj --connection $NeonConnectionString 2>&1
}
Write-Host ""

# Bước 6: Tạo Initial Migration nếu chưa có
$migrationsPath = Join-Path $PSScriptRoot "Migrations"
$hasInitial = Get-ChildItem -Path $migrationsPath -Filter "*Initial*.cs" -ErrorAction SilentlyContinue

if (-not $hasInitial) {
    Write-Host "📝 Bước 6: Tạo Initial Migration..." -ForegroundColor Yellow
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

# Bước 7: Apply migrations
Write-Host "🚀 Bước 7: Apply migrations lên database..." -ForegroundColor Yellow
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
    Write-Host ""
    Write-Host "💡 Kiểm tra trên Neon Console để xem các tables đã được tạo!" -ForegroundColor Yellow
} else {
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Red
    Write-Host "  ❌ Migration thất bại!                " -ForegroundColor Red
    Write-Host "========================================" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "✅ Hoàn tất!" -ForegroundColor Green

