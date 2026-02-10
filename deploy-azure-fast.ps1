#!/usr/bin/env pwsh

# DEPLOY RAPIDO A AZURE - CON DATOS PRECARGADOS
# Solo ejecuta: .\deploy-azure-fast.ps1

# ===== CONFIGURACION =====
$resourceGroup = "tandas-rg"              # Cambiar si quieres otro nombre
$appServiceName = "tandas-app"            # Cambiar si quieres otro nombre
$location = "eastus"                      # Cambiar ubicacion si quieres
# =======================

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Deploy AngelesTandas a Azure" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Recursos que se crearán:" -ForegroundColor Yellow
Write-Host "  Resource Group: $resourceGroup" -ForegroundColor White
Write-Host "  App Service: $appServiceName" -ForegroundColor White
Write-Host "  URL: https://$appServiceName.azurewebsites.net" -ForegroundColor White
Write-Host "  Ubicación: $location" -ForegroundColor White
Write-Host ""

# Verificar ZIP
$zipFile = Get-ChildItem "AngelesTandas-App-v1.0-*.zip" -ErrorAction SilentlyContinue | Select-Object -First 1
if (-not $zipFile) {
    Write-Host "❌ No se encontró ZIP" -ForegroundColor Red
    exit 1
}

Write-Host "✅ Archivo: $($zipFile.Name)" -ForegroundColor Green
Write-Host ""

# Confirm
$confirm = Read-Host "¿Desplegar ahora? (S/n)"
if ($confirm -ne "" -and $confirm -ne "S" -and $confirm -ne "s") {
    Write-Host "Cancelado" -ForegroundColor Yellow
    exit 0
}

Write-Host ""
Write-Host "Desplegando..." -ForegroundColor Cyan
Write-Host ""

# 1. Resource Group
Write-Host "[1/5] Creando Resource Group: $resourceGroup" -ForegroundColor Yellow
az group create --name $resourceGroup --location $location | Out-Null

# 2. App Service Plan
Write-Host "[2/5] Creando App Service Plan" -ForegroundColor Yellow
az appservice plan create `
  --name "$appServiceName-plan" `
  --resource-group $resourceGroup `
  --sku B1 `
  --is-linux | Out-Null

# 3. Web App
Write-Host "[3/5] Creando Web App" -ForegroundColor Yellow
az webapp create `
  --resource-group $resourceGroup `
  --plan "$appServiceName-plan" `
  --name $appServiceName `
  --runtime "DOTNETCORE|10.0" | Out-Null

# 4. Deploy
Write-Host "[4/5] Desplegando aplicación (esto toma 2-3 minutos)..." -ForegroundColor Yellow
az webapp deployment source config-zip `
  --resource-group $resourceGroup `
  --name $appServiceName `
  --src $($zipFile.FullName) | Out-Null

# 5. HTTPS
Write-Host "[5/5] Configurando HTTPS" -ForegroundColor Yellow
az webapp update `
  --resource-group $resourceGroup `
  --name $appServiceName `
  --https-only true | Out-Null

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "  ✅ DEPLOY COMPLETADO" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "🌐 URL: https://$appServiceName.azurewebsites.net" -ForegroundColor Cyan
Write-Host ""
Write-Host "⏳ ESPERA 2-3 MINUTOS para que la app inicie" -ForegroundColor Yellow
Write-Host ""
Write-Host "Login:" -ForegroundColor White
Write-Host "  Email: admin@local" -ForegroundColor Gray
Write-Host "  Contraseña: Se genera automáticamente (ver logs)" -ForegroundColor Gray
Write-Host ""
Write-Host "Ver logs:" -ForegroundColor White
Write-Host "  az webapp log tail --resource-group $resourceGroup --name $appServiceName" -ForegroundColor Gray
Write-Host ""
