# Script nhanh để dừng app Products_Management
# Usage: .\stop-app.ps1

Write-Host ""
Write-Host "🔍 Đang tìm process Products_Management..." -ForegroundColor Yellow

$processes = Get-Process | Where-Object { 
    $_.ProcessName -like "*Products_Management*" -or 
    $_.ProcessName -like "*Products*Management*" -or
    $_.MainWindowTitle -like "*Products*"
}

if ($processes) {
    Write-Host "✅ Tìm thấy $($processes.Count) process:" -ForegroundColor Green
    foreach ($proc in $processes) {
        Write-Host "   - $($proc.ProcessName) (PID: $($proc.Id))" -ForegroundColor White
    }
    Write-Host ""
    
    foreach ($proc in $processes) {
        try {
            Stop-Process -Id $proc.Id -Force
            Write-Host "✅ Đã dừng: $($proc.ProcessName) (PID: $($proc.Id))" -ForegroundColor Green
        } catch {
            Write-Host "⚠️  Không thể dừng process $($proc.Id): $_" -ForegroundColor Yellow
        }
    }
    Write-Host ""
    Write-Host "✅ Đã dừng tất cả processes!" -ForegroundColor Green
} else {
    Write-Host "ℹ️  Không có process nào đang chạy" -ForegroundColor Cyan
}

Write-Host ""

