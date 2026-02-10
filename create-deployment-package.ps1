#!/usr/bin/env pwsh

# Script para crear ZIP empaquetado para desplegar a Azure
# Uso: .\create-deployment-package.ps1

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Creando paquete de despliegue" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Verificar que la carpeta publish existe
if (-not (Test-Path ".\publish\AngelesTandas")) {
    Write-Host "❌ Carpeta publish no existe. Ejecuta primero: .\package-for-azure.ps1" -ForegroundColor Red
    exit 1
}

Write-Host "[1/3] Preparando paquete..." -ForegroundColor Yellow

# Crear ZIP
$zipPath = ".\deploy-$(Get-Date -Format 'yyyyMMdd-HHmmss').zip"
Compress-Archive -Path ".\publish\AngelesTandas\*" -DestinationPath $zipPath -Force

Write-Host "[2/3] Validando ZIP..." -ForegroundColor Yellow
if (Test-Path $zipPath) {
    $size = Get-Item $zipPath | Select-Object -ExpandProperty Length
    $sizeMB = [math]::Round($size / 1MB, 2)
    Write-Host "✓ ZIP creado: $zipPath" -ForegroundColor Green
    Write-Host "  Tamaño: $sizeMB MB" -ForegroundColor Cyan
} else {
    Write-Host "❌ Error al crear ZIP" -ForegroundColor Red
    exit 1
}

Write-Host "[3/3] Listo para desplegar" -ForegroundColor Yellow
Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "  ✓ Paquete listo para Azure" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Para desplegar a Azure:" -ForegroundColor Yellow
Write-Host "  az webapp deployment source config-zip --resource-group <rg> --name <app> --src $zipPath" -ForegroundColor White
Write-Host ""
