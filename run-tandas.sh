#!/bin/bash
# Script para inicializar y ejecutar la app de Tandas (Linux/macOS)
# Uso: chmod +x run-tandas.sh && ./run-tandas.sh

set -e  # Exit on error

echo ""
echo "========================================"
echo "  Angeles Tandas App - Setup & Run"
echo "========================================"
echo ""

# Verificar si estamos en el directorio raíz del proyecto
if [ ! -f "src/AngelesTandas.Web/AngelesTandas.Web.csproj" ]; then
    echo "ERROR: No se encontró el proyecto. Ejecuta este script desde la raíz del repositorio."
    exit 1
fi

WEBPROJ="src/AngelesTandas.Web"

echo "[1/6] Inicializando user-secrets..."
cd "$WEBPROJ"
dotnet user-secrets init --force 2>/dev/null || true
cd ../..

echo "[2/6] Configurando secretos..."
cd "$WEBPROJ"

declare -A secrets=(
    ["ConnectionStrings:DefaultConnection"]="Server=(localdb)\\mssqllocaldb;Database=AngelesTandas;Trusted_Connection=true;MultipleActiveResultSets=true"
    ["BlobStorage:ConnectionString"]="UseDevelopmentStorage=true"
    ["BlobStorage:Container"]="receipts"
    ["Seed:AdminEmail"]="admin@local"
    ["Seed:AdminPassword"]="Admin123!"
)

for key in "${!secrets[@]}"; do
    echo "  → Configurando $key"
    dotnet user-secrets set "$key" "${secrets[$key]}" 2>/dev/null || true
done

cd ../..

echo "[3/6] Restaurando dependencias..."
dotnet restore

echo "[4/6] Compilando proyecto..."
dotnet build

echo ""
echo "========================================"
echo "  Configuración completada"
echo "========================================"
echo ""
echo "Credenciales seeded:"
echo "  Email:    admin@local"
echo "  Password: Admin123!"
echo ""

read -p "¿Deseas ejecutar la app ahora? (S/n): " -r RUNAPP
RUNAPP=${RUNAPP:-S}

if [[ "$RUNAPP" =~ ^[Ss]$ ]] || [ -z "$RUNAPP" ]; then
    echo ""
    echo "[5/6] Ejecutando aplicación..."
    echo "  → Abre tu navegador en: https://localhost:5001"
    echo "  → Presiona CTRL+C para detener"
    echo ""
    dotnet run --project src/AngelesTandas.Web
else
    echo "[5/6] Omitido (ejecución manual)"
    echo ""
    echo "Para ejecutar después, usa:"
    echo "  dotnet run --project src/AngelesTandas.Web"
fi
