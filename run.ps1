# SkyNet — Stop old instance, build, start fresh
Write-Host "Stopping old SkyNet.API..." -ForegroundColor Yellow
Get-Process -Name "SkyNet.API" -ErrorAction SilentlyContinue | Stop-Process -Force -Confirm:$false
Get-Process -Name "dotnet"     -ErrorAction SilentlyContinue | Stop-Process -Force -Confirm:$false
Start-Sleep -Seconds 2

Write-Host "Building..." -ForegroundColor Cyan
Set-Location $PSScriptRoot
dotnet build SkyNet.sln --nologo -q
if ($LASTEXITCODE -ne 0) { Write-Host "BUILD FAILED!" -ForegroundColor Red; exit 1 }

Write-Host "Starting SkyNet.API..." -ForegroundColor Green
Start-Process -FilePath "$PSScriptRoot\SkyNet.API\bin\Debug\net8.0\SkyNet.API.exe" `
              -WorkingDirectory "$PSScriptRoot\SkyNet.API" `
              -WindowStyle Hidden
Start-Sleep -Seconds 6

try {
    $r = Invoke-WebRequest "http://localhost:5000/api/airports" -UseBasicParsing -TimeoutSec 5
    $j = $r.Content | ConvertFrom-Json
    Write-Host "OK — Backend ishlayapdi: $($j.Count) ta aeroport" -ForegroundColor Green
    Write-Host "http://localhost:5000" -ForegroundColor Cyan
} catch {
    Write-Host "Backend ishlamayapdi: $($_.Exception.Message)" -ForegroundColor Red
}
