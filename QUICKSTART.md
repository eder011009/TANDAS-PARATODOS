# Angelo Tandas App - Guía Rápida de Ejecución

## 🚀 Inicio Rápido (Choose One)

### Windows (PowerShell)
```powershell
# 1. Abre PowerShell como administrador en la raíz del proyecto
# 2. Ejecuta:
.\run-tandas.ps1

# O manualmente paso a paso:
cd src\AngelesTandas.Web
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=(localdb)\mssqllocaldb;Database=AngelesTandas;Trusted_Connection=true;MultipleActiveResultSets=true"
dotnet user-secrets set "BlobStorage:ConnectionString" "UseDevelopmentStorage=true"
dotnet user-secrets set "BlobStorage:Container" "receipts"
dotnet user-secrets set "Seed:AdminEmail" "admin@local"
dotnet user-secrets set "Seed:AdminPassword" "Admin123!"
cd ..\..
dotnet restore
dotnet build
dotnet run --project src/AngelesTandas.Web
```

### Windows (Batch/CMD)
```cmd
REM 1. Abre CMD en la raíz del proyecto
REM 2. Ejecuta:
run-tandas.bat
```

### Linux / macOS (Bash)
```bash
# 1. Abre terminal en la raíz del proyecto
# 2. Ejecuta:
chmod +x run-tandas.sh
./run-tandas.sh
```

---

## 📝 Credenciales por defecto (seeded)
- **Email:** `admin@local`
- **Contraseña:** `Admin123!`

---

## 🌐 URL de acceso
Una vez que la app esté ejecutando (después de ver el output de dotnet run):
- **App:** https://localhost:5001
- **Admin Panel:** https://localhost:5001/admin (una vez logueado como admin)

---

## ✅ Flujo de prueba end-to-end

1. **Login como admin**
   - Email: `admin@local`
   - Password: `Admin123!`

2. **Crear una Tanda** (Admin)
   - Ve a /tandas/create
   - Completa el formulario (nombre, monto, participantes, etc.)
   - Click "Crear"

3. **Crear un Pago** (Usuario)
   - Ve a /payments/create
   - Ingresa monto
   - Click "Crear pago"
   - El sistema genera un Payment ID

4. **Subir Comprobante**
   - En la sección de FileUpload que aparece
   - Selecciona un archivo (imagen JPG/PNG o PDF)
   - Se sube al servidor (localDB o Azure Blob Storage)
   - Recibirás la URI del archivo

5. **Aprobar Pago** (Admin)
   - Ve a /admin/payments/pending
   - Lista de pagos pendientes aparece
   - Click "Aprobar" o "Rechazar"
   - Se registra en auditoría automáticamente

6. **Ver Plantillas** (Admin)
   - Ve a /admin/templates
   - Edita plantillas de notificación
   - Guarda cambios

---

## 🔧 Configuración Avanzada

### SQL Server (en lugar de LocalDB)
En lugar de usar LocalDB, puedes usar SQL Server completo:

```powershell
# En PowerShell, en src\AngelesTandas.Web:
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=TU_SERVIDOR;Database=AngelesTandas;User Id=sa;Password=TU_PASSWORD;"
```

### Azure Blob Storage
Si tienes una cuenta de Azure:

```powershell
# En PowerShell, en src\AngelesTandas.Web:
dotnet user-secrets set "BlobStorage:ConnectionString" "DefaultEndpointsProtocol=https;AccountName=NOMBRE;AccountKey=CLAVE;EndpointSuffix=core.windows.net"
dotnet user-secrets set "BlobStorage:Container" "receipts"
```

### Azure Storage Emulator (Desarrollo)
Para pruebas locales con un emulador (requiere Azurite):

```bash
# Instala Azurite (Node.js):
npm install -g azurite

# Ejecuta en otra terminal:
azurite --silent --location .azurite --debug .azurite/debug.log

# Luego en PowerShell en src\AngelesTandas.Web:
dotnet user-secrets set "BlobStorage:ConnectionString" "UseDevelopmentStorage=true"
```

---

## 📊 Base de Datos

### Ver la BD localmente
La BD se crea automáticamente en:
```
(localdb)\mssqllocaldb
Database: AngelesTandas
```

Puedes conectarte desde Visual Studio > SQL Server Object Explorer o desde SQL Server Management Studio:
```
Server: (localdb)\mssqllocaldb
Database: AngelesTandas
```

### Verificar tablas creadas
Una vez ejecutando la app, se crean automáticamente:
- AspNetUsers (Identity)
- AspNetRoles (Identity)
- Tandas
- TandaParticipants
- Turns
- Payments
- PaymentReceipts
- AuditLogs
- NotificationTemplates
- PaymentInstructionTemplates

---

## 🐛 Troubleshooting

### "Port 5001 already in use"
```powershell
# Cambia el puerto en src/AngelesTandas.Web/Properties/launchSettings.json
# O termina el proceso que usa el puerto:
netstat -ano | findstr :5001
taskkill /PID <PID> /F
```

### "User secrets not working"
```powershell
# Reinstancia los secrets:
cd src\AngelesTandas.Web
dotnet user-secrets list  # Ver actuales
dotnet user-secrets clear  # Limpiar
dotnet user-secrets init  # Reiniciar
# Luego vuelve a configurar
```

### "Certificate error on HTTPS"
```powershell
# Confía en el certificado de desarrollo:
dotnet dev-certs https --trust
```

---

## 📚 Estructura del Proyecto

```
src/
├── AngelesTandas.Web/          # Blazor Server Web App
│   ├── Pages/                  # Páginas Razor/Blazor
│   ├── Shared/                 # Componentes compartidos
│   ├── Program.cs              # Configuración y startup
│   └── appsettings.json        # Configuración (no incluye secrets)
├── AngelesTandas.Application/  # DTOs, Interfaces de servicios
├── AngelesTandas.Domain/       # Entidades, agregados
└── AngelesTandas.Infrastructure/  # DbContext, Servicios implementados

tests/
└── AngelesTandas.UnitTests/    # Pruebas unitarias (opcional)
```

---

## 🚀 Deploy a Azure (Próximos pasos)

Para desplegar a Azure:
1. Crear App Service en Azure
2. Crear Azure SQL Database
3. Crear Azure Storage Account
4. Configurar GitHub Actions para CI/CD
5. Guardar secrets en Azure Key Vault

(Puedo ayudarte con estos pasos si lo necesitas)

---

**¡Disfruta tu app de Tandas!** 🎉
