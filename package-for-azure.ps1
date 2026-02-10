#!/usr/bin/env pwsh

# Script para empaquetar la app para Azure
# Uso: .\package-for-azure.ps1

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Empaquetando Angelo Tandas para Azure" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Limpiar
Write-Host "[1/4] Limpiando compilaciones previas..." -ForegroundColor Yellow
dotnet clean src/AngelesTandas.Web

# Restaurar
Write-Host "[2/4] Restaurando dependencias..." -ForegroundColor Yellow
dotnet restore

# Compilar en Release
Write-Host "[3/4] Compilando en modo Release..." -ForegroundColor Yellow
dotnet build -c Release

# Publicar
Write-Host "[4/4] Publicando aplicación..." -ForegroundColor Yellow
$publishPath = ".\publish\AngelesTandas"
dotnet publish src/AngelesTandas.Web -c Release -o $publishPath

if (Test-Path $publishPath) {
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "  ✓ Empaquetamiento completado" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "Ubicación: $publishPath" -ForegroundColor Cyan
    Write-Host "Tamaño: $(Get-Item $publishPath | Measure-Object -Recurse -Sum -Property Length | Select-Object @{Name='Size'; Expression={'{0:N0} KB' -f ($_.Sum/1024)}} | Select-Object -ExpandProperty Size)" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Próximos pasos:" -ForegroundColor Yellow
    Write-Host "1. Commit & Push a GitHub" -ForegroundColor White
    Write-Host "   git add ." -ForegroundColor Gray
    Write-Host "   git commit -m 'Release version'" -ForegroundColor Gray
    Write-Host "   git push origin main" -ForegroundColor Gray
    Write-Host ""
    Write-Host "2. GitHub Actions compilará y desplegará automáticamente" -ForegroundColor White
    Write-Host ""
    Write-Host "3. O desplegar manualmente:" -ForegroundColor White
    Write-Host "   az webapp deployment source config-zip --resource-group <rg> --name <app> --src publish.zip" -ForegroundColor Gray
} else {
    Write-Host "❌ Error: No se pudo crear el paquete" -ForegroundColor Red
    exit 1
}
