#!/usr/bin/env pwsh

# Script para inicializar y ejecutar la app de Tandas
# Uso: .\run-tandas.ps1

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Angeles Tandas App - Setup & Run" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Verificar si estamos en el directorio raíz del proyecto
if (-not (Test-Path "src\AngelesTandas.Web\AngelesTandas.Web.csproj")) {
    Write-Host "ERROR: No se encontró el proyecto. Ejecuta este script desde la raíz del repositorio." -ForegroundColor Red
    exit 1
}

$webProjectPath = "src\AngelesTandas.Web"

Write-Host "[1/6] Inicializando user-secrets..." -ForegroundColor Yellow
Push-Location $webProjectPath
dotnet user-secrets init --force 2>$null
Pop-Location

Write-Host "[2/6] Configurando secretos..." -ForegroundColor Yellow

# Secrets básicos para dev (LocalDB)
Push-Location $webProjectPath

$secrets = @{
    "ConnectionStrings:DefaultConnection" = "Server=(localdb)\mssqllocaldb;Database=AngelesTandas;Trusted_Connection=true;MultipleActiveResultSets=true"
    "BlobStorage:ConnectionString" = "UseDevelopmentStorage=true"  # Azure Storage Emulator / Azurite
    "BlobStorage:Container" = "receipts"
    "Seed:AdminEmail" = "admin@local"
    "Seed:AdminPassword" = "Admin123!"
}

foreach ($key in $secrets.Keys) {
    $value = $secrets[$key]
    Write-Host "  → Configurando $key" -ForegroundColor Gray
    dotnet user-secrets set $key $value 2>$null
}

Write-Host "[3/6] Restaurando dependencias..." -ForegroundColor Yellow
Pop-Location
dotnet restore

Write-Host "[4/6] Compilando proyecto..." -ForegroundColor Yellow
dotnet build

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "  Configuración completada" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Credenciales seeded:" -ForegroundColor Cyan
Write-Host "  Email:    admin@local" -ForegroundColor White
Write-Host "  Password: Admin123!" -ForegroundColor White
Write-Host ""

# Preguntar si ejecutar
$response = Read-Host "¿Deseas ejecutar la app ahora? (S/n)"
if ($response -eq "" -or $response -eq "s" -or $response -eq "S") {
    Write-Host ""
    Write-Host "[5/6] Ejecutando aplicación..." -ForegroundColor Yellow
    Write-Host "  → Abre tu navegador en: https://localhost:5001" -ForegroundColor Cyan
    Write-Host "  → Presiona CTRL+C para detener" -ForegroundColor Cyan
    Write-Host ""
    dotnet run --project src\AngelesTandas.Web
} else {
    Write-Host "[5/6] Omitido (ejecución manual)" -ForegroundColor Gray
    Write-Host ""
    Write-Host "Para ejecutar después, usa:" -ForegroundColor Yellow
    Write-Host "  dotnet run --project src\AngelesTandas.Web" -ForegroundColor White
}
