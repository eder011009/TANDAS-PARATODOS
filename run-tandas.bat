@echo off
REM Script para inicializar y ejecutar la app de Tandas (Batch)
REM Uso: run-tandas.bat

setlocal enabledelayedexpansion

echo.
echo ========================================
echo   Angeles Tandas App - Setup ^& Run
echo ========================================
echo.

REM Verificar si estamos en el directorio raíz
if not exist "src\AngelesTandas.Web\AngelesTandas.Web.csproj" (
    echo ERROR: No se encontró el proyecto. Ejecuta este script desde la raíz del repositorio.
    exit /b 1
)

set WEBPROJ=src\AngelesTandas.Web

echo [1/6] Inicializando user-secrets...
cd %WEBPROJ%
dotnet user-secrets init --force >nul 2>&1
cd ..\..

echo [2/6] Configurando secretos...
cd %WEBPROJ%

echo   - ConnectionStrings:DefaultConnection
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=(localdb)\mssqllocaldb;Database=AngelesTandas;Trusted_Connection=true;MultipleActiveResultSets=true" >nul 2>&1

echo   - BlobStorage:ConnectionString
dotnet user-secrets set "BlobStorage:ConnectionString" "UseDevelopmentStorage=true" >nul 2>&1

echo   - BlobStorage:Container
dotnet user-secrets set "BlobStorage:Container" "receipts" >nul 2>&1

echo   - Seed:AdminEmail
dotnet user-secrets set "Seed:AdminEmail" "admin@local" >nul 2>&1

echo   - Seed:AdminPassword
dotnet user-secrets set "Seed:AdminPassword" "Admin123!" >nul 2>&1

cd ..\..

echo [3/6] Restaurando dependencias...
dotnet restore

echo [4/6] Compilando proyecto...
dotnet build

echo.
echo ========================================
echo   Configuración completada
echo ========================================
echo.
echo Credenciales seeded:
echo   Email:    admin@local
echo   Password: Admin123!
echo.

set /p RUNAPP="Deseas ejecutar la app ahora? (S/n): "
if "%RUNAPP%"=="" set RUNAPP=S
if /i "%RUNAPP%"=="S" (
    echo.
    echo [5/6] Ejecutando aplicación...
    echo   - Abre tu navegador en: https://localhost:5001
    echo   - Presiona CTRL+C para detener
    echo.
    dotnet run --project src\AngelesTandas.Web
) else (
    echo [5/6] Omitido (ejecución manual)
    echo.
    echo Para ejecutar después, usa:
    echo   dotnet run --project src\AngelesTandas.Web
)

endlocal
