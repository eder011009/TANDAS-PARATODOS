#!/usr/bin/env pwsh

# DEPLOY A AZURE - APP WEB DE TANDAS
# NO CONTENEDORES. WEB APP DIRECTO.

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Deploy a Azure App Service" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Requisitos
Write-Host "REQUISITOS:" -ForegroundColor Yellow
Write-Host "1. Tener Azure CLI instalado: https://learn.microsoft.com/en-us/cli/azure/install-azure-cli" -ForegroundColor Gray
Write-Host "2. Estar logueado en Azure: az login" -ForegroundColor Gray
Write-Host "3. Tener una suscripción activa" -ForegroundColor Gray
Write-Host ""

# Verificar que tenemos el ZIP
if (-not (Test-Path "AngelesTandas-App-v1.0-*.zip")) {
    Write-Host "❌ No se encontró ZIP. Ejecuta primero: dotnet publish src/AngelesTandas.Web -c Release -o ./publish/AngelesTandas" -ForegroundColor Red
    exit 1
}

$zipFile = Get-ChildItem "AngelesTandas-App-v1.0-*.zip" | Select-Object -First 1
Write-Host "✅ Encontrado: $($zipFile.Name)" -ForegroundColor Green
Write-Host ""

# Solicitar datos
$resourceGroup = Read-Host "Resource Group (ej: tandas-rg)"
$appServiceName = Read-Host "App Service name (ej: tandas-app)"
$location = Read-Host "Ubicación (ej: eastus, westus, centralus)"

Write-Host ""
Write-Host "========================================" -ForegroundColor Yellow
Write-Host "  Desplegando..." -ForegroundColor Yellow
Write-Host "========================================" -ForegroundColor Yellow
Write-Host ""

# 1. Crear Resource Group
Write-Host "[1/5] Creando Resource Group..." -ForegroundColor Cyan
az group create --name $resourceGroup --location $location

# 2. Crear App Service Plan (B1 = Basic, económico)
Write-Host "[2/5] Creando App Service Plan..." -ForegroundColor Cyan
az appservice plan create `
  --name "$appServiceName-plan" `
  --resource-group $resourceGroup `
  --sku B1 `
  --is-linux

# 3. Crear Web App
Write-Host "[3/5] Creando Web App..." -ForegroundColor Cyan
az webapp create `
  --resource-group $resourceGroup `
  --plan "$appServiceName-plan" `
  --name $appServiceName `
  --runtime "DOTNETCORE|10.0"

# 4. Desplegar ZIP
Write-Host "[4/5] Desplegando aplicación..." -ForegroundColor Cyan
az webapp deployment source config-zip `
  --resource-group $resourceGroup `
  --name $appServiceName `
  --src $($zipFile.FullName)

# 5. Configurar HTTPS
Write-Host "[5/5] Configurando HTTPS..." -ForegroundColor Cyan
az webapp update `
  --resource-group $resourceGroup `
  --name $appServiceName `
  --https-only true

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "  ✅ DEPLOY COMPLETADO" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Tu app está en: https://$appServiceName.azurewebsites.net" -ForegroundColor Cyan
Write-Host ""
Write-Host "⚠️ IMPORTANTE:" -ForegroundColor Yellow
Write-Host "1. Espera 2-3 minutos para que la app inicie" -ForegroundColor Gray
Write-Host "2. La contraseña de admin se genera automáticamente" -ForegroundColor Gray
Write-Host "3. Ver logs: az webapp log tail --resource-group $resourceGroup --name $appServiceName" -ForegroundColor Gray
Write-Host ""
