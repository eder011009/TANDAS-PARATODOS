#!/usr/bin/env pwsh

# SCRIPT PARA CORREGIR AZURE - EJECUTAR EN TERMINAL DE AZURE

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Corrigiendo Azure - AngelesTandas" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# 1. Cambiar runtime a .NET 10
Write-Host "[1/4] Cambiando runtime a .NET 10..." -ForegroundColor Yellow
az webapp config set --resource-group tandas --name TANDAS-PARATODOS --windows-fx-version "DOTNETCORE|10.0"
Write-Host "✅ Runtime actualizado" -ForegroundColor Green
Write-Host ""

# 2. Cambiar tier a B1
Write-Host "[2/4] Cambiando tier a B1 (necesario para .NET)..." -ForegroundColor Yellow
az appservice plan update --name plan-TANDAS-PARATODOS --resource-group tandas --sku B1
Write-Host "✅ Tier actualizado a B1" -ForegroundColor Green
Write-Host ""

# 3. Asegurar 1 instancia
Write-Host "[3/4] Configurando 1 instancia..." -ForegroundColor Yellow
az appservice plan update --name plan-TANDAS-PARATODOS --resource-group tandas --number-of-workers 1
Write-Host "✅ Instancias configuradas" -ForegroundColor Green
Write-Host ""

# 4. Desplegar ZIP
Write-Host "[4/4] Desplegando aplicación (esto toma 2-3 minutos)..." -ForegroundColor Yellow
az webapp deployment source config-zip --resource-group tandas --name TANDAS-PARATODOS --src AngelesTandas-App-CORRECTED-20260209.zip
Write-Host "✅ Aplicación desplegada" -ForegroundColor Green
Write-Host ""

Write-Host "========================================" -ForegroundColor Green
Write-Host "  ✅ COMPLETADO" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "⏳ ESPERA 5 MINUTOS para que la app inicie" -ForegroundColor Yellow
Write-Host ""
Write-Host "🌐 URL: https://tandas-paratodos-bvbag0fxfrdkhsb9.mexicocentral-01.azurewebsites.net" -ForegroundColor Cyan
Write-Host ""
Write-Host "Login:" -ForegroundColor White
Write-Host "  Email: admin@local" -ForegroundColor Gray
Write-Host "  Contraseña: Se genera automáticamente" -ForegroundColor Gray
Write-Host ""
