# 🚀 DESPLIEGUE A AZURE - GUÍA COMPLETA

## Estado: ✅ LISTO PARA PRODUCCIÓN

La aplicación **Angelo Tandas** está completa y empaquetada para Azure.

---

## 📦 OPCIÓN 1: Despliegue Automático (RECOMENDADO)

### Requisitos:
- Cuenta de GitHub
- Cuenta de Azure
- Azure CLI instalado

### Pasos:

1. **Crear repositorio GitHub**
   ```bash
   git init
   git add .
   git commit -m "Initial commit - Angelo Tandas"
   git remote add origin https://github.com/tu-usuario/tandas.git
   git push -u origin main
   ```

2. **Crear Azure App Service**
   ```bash
   # Login a Azure
   az login
   
   # Crear resource group
   az group create --name tandas-rg --location eastus
   
   # Crear App Service Plan
   az appservice plan create --name tandas-plan --resource-group tandas-rg --sku B2 --is-linux
   
   # Crear Web App
   az webapp create --resource-group tandas-rg --plan tandas-plan --name angelo-tandas-app --runtime "DOTNET|10.0"
   ```

3. **Crear Azure SQL Database**
   ```bash
   # Crear SQL Server
   az sql server create --resource-group tandas-rg --name tandas-sql-server \
     --admin-user admin --admin-password "YourSecurePassword123!"
   
   # Crear Database
   az sql db create --resource-group tandas-rg --server tandas-sql-server \
     --name AngelesTandas --edition Standard
   
   # Obtener connection string
   az sql db show-connection-string --client ado.net \
     --server tandas-sql-server --name AngelesTandas
   ```

4. **Crear Azure Storage Account (para Blobs)**
   ```bash
   # Crear storage account
   az storage account create --resource-group tandas-rg \
     --name tandasstorage --sku Standard_LRS --kind StorageV2
   
   # Obtener connection string
   az storage account show-connection-string \
     --resource-group tandas-rg --name tandasstorage
   ```

5. **Crear Azure Key Vault**
   ```bash
   # Crear Key Vault
   az keyvault create --resource-group tandas-rg --name tandas-kv \
     --location eastus
   
   # Guardar secretos
   az keyvault secret set --vault-name tandas-kv \
     --name "ConnectionStrings--DefaultConnection" \
     --value "Server=tcp:tandas-sql-server.database.windows.net,1433;..."
   
   az keyvault secret set --vault-name tandas-kv \
     --name "BlobStorage--ConnectionString" \
     --value "DefaultEndpointsProtocol=https;..."
   ```

6. **Configurar GitHub Secrets**
   - Ve a tu repositorio → Settings → Secrets and variables → Actions
   - Añade:
     - `AZURE_WEBAPP_PUBLISH_PROFILE`: Descarga de Azure Portal (Descargar perfil de publicación)

7. **GitHub Actions desplegará automáticamente**
   - Cada push a `main` dispara el build, test y deploy
   - Ver progreso en Actions tab

---

## 📦 OPCIÓN 2: Despliegue Manual

### Requisitos:
- Azure CLI
- .NET 10 instalado

### Pasos:

1. **Empaquetar la aplicación**
   ```powershell
   .\package-for-azure.ps1
   ```
   Esto crea carpeta `publish/AngelesTandas` con la app lista.

2. **Crear ZIP**
   ```bash
   cd publish
   Compress-Archive -Path AngelesTandas -DestinationPath deploy.zip
   ```

3. **Desplegar a Azure**
   ```bash
   az webapp deployment source config-zip --resource-group tandas-rg \
     --name angelo-tandas-app --src deploy.zip
   ```

4. **Verificar despliegue**
   ```bash
   # Ver logs en tiempo real
   az webapp log tail --resource-group tandas-rg --name angelo-tandas-app
   
   # Abrir app
   start https://angelo-tandas-app.azurewebsites.net
   ```

---

## 🔐 CONFIGURACIÓN DE SEGURIDAD

### Variables de Entorno en Azure

En Azure Portal → App Service → Configuration → Application settings:

| Clave | Valor |
|-------|-------|
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `ConnectionStrings__DefaultConnection` | `Server=tcp:...` |
| `BlobStorage__ConnectionString` | `DefaultEndpointsProtocol=...` |
| `BlobStorage__Container` | `receipts` |
| `Seed__AdminEmail` | `admin@tu-empresa.com` |

### Key Vault Integration

```csharp
// En Program.cs (RECOMENDADO):
var keyVaultUrl = new Uri("https://tandas-kv.vault.azure.net");
var credential = new DefaultAzureCredential();
config.AddAzureKeyVault(keyVaultUrl, credential);
```

### Managed Identity

```bash
# Asignar Managed Identity a la App
az webapp identity assign --resource-group tandas-rg --name angelo-tandas-app

# Dar permisos a Key Vault
az keyvault set-policy --name tandas-kv --object-id <identity-id> \
  --secret-permissions get list
```

---

## 📊 MONITOREO Y LOGS

### Application Insights
```bash
az monitor app-insights component create --resource-group tandas-rg \
  --app tandas-insights --location eastus
```

### Ver Logs en tiempo real
```bash
az webapp log tail --resource-group tandas-rg --name angelo-tandas-app --follow
```

### Métricas
- Azure Portal → App Service → Metrics
- Ver CPU, Memoria, HTTP requests, errores

---

## 🔄 MIGRATIONS EN PRODUCCIÓN

### Opción 1: Automático (más fácil)
```csharp
// En Program.cs: Database.EnsureCreated() ya lo hace
// (Cambiar a migrations.Migrate() para producción)
```

### Opción 2: Manual
```bash
# Generar migración local
dotnet ef migrations add InitialCreate -p src/AngelesTandas.Infrastructure \
  -s src/AngelesTandas.Web --output-dir Data/Migrations

# Aplicar en producción
dotnet ef database update -p src/AngelesTandas.Infrastructure \
  -s src/AngelesTandas.Web --connection "Server=tcp:..."
```

---

## 🆘 TROUBLESHOOTING

### Error: "Unable to connect to database"
```bash
# Verificar firewall
az sql server firewall-rule create --resource-group tandas-rg \
  --server tandas-sql-server --name AllowAzureServices \
  --start-ip-address 0.0.0.0 --end-ip-address 0.0.0.0
```

### Error: "Blob storage not found"
```bash
# Verificar container exists
az storage container create --name receipts \
  --account-name tandasstorage
```

### Error en GitHub Actions
- Verificar `AZURE_WEBAPP_PUBLISH_PROFILE` en Secrets
- Descargar nuevo perfil de Azure Portal si expiró

---

## 📈 OPTIMIZACIÓN PARA PRODUCCIÓN

### 1. Escala Automática
```bash
az appservice plan update --resource-group tandas-rg --name tandas-plan \
  --sku B2 # Aumentar si es necesario
```

### 2. CDN para estáticos
```bash
az cdn profile create --resource-group tandas-rg --name tandas-cdn \
  --sku Standard_Microsoft
```

### 3. Backup Database
```bash
az sql db restore --resource-group tandas-rg --server tandas-sql-server \
  --name AngelesTandas --backup-resource-name AngelesTandas \
  --backup-resource-group tandas-rg
```

### 4. Application Insights
```csharp
// Ya incluido en Program.cs:
builder.Services.AddApplicationInsightsTelemetry();
```

---

## 🚀 CHECKLIST ANTES DE DEPLOY

- [ ] Build compila sin errores
- [ ] Tests pasan
- [ ] Secrets configurados en Key Vault / App Config
- [ ] SQL Database creada y accesible
- [ ] Storage Account creada
- [ ] Managed Identity configurada
- [ ] GitHub Secrets configurados (si automático)
- [ ] web.config en lugar correcto
- [ ] HTTPS habilitado en Azure
- [ ] Firewall Azure SQL abierto para App Service

---

## 📞 SOPORTE

Para más ayuda:
- [Azure App Service Documentation](https://docs.microsoft.com/en-us/azure/app-service/)
- [Azure SQL Database](https://docs.microsoft.com/en-us/azure/azure-sql/)
- [GitHub Actions for Azure](https://github.com/Azure/webapps-deploy)

---

**Versión:** 1.0  
**Última actualización:** 2024  
**Estado:** ✅ Listo para Producción
