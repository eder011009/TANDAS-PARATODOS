# ✅ ANGELES TANDAS APP - APP WEB LISTA PARA USAR

## Estado: PRODUCCIÓN (v1.0)

La aplicación web de Tandas está **100% funcional y ejecutándose**.

---

## 🚀 INICIO RÁPIDO

### Windows (PowerShell)
```powershell
.\run-tandas.ps1
```

### Windows (CMD)
```cmd
run-tandas.bat
```

### Linux/macOS
```bash
./run-tandas.sh
```

### Manual (cualquier OS)
```bash
cd src/AngelesTandas.Web
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=(localdb)\mssqllocaldb;Database=AngelesTandas;Trusted_Connection=true;MultipleActiveResultSets=true"
dotnet user-secrets set "BlobStorage:ConnectionString" "UseDevelopmentStorage=true"
dotnet user-secrets set "BlobStorage:Container" "receipts"
dotnet user-secrets set "Seed:AdminEmail" "admin@local"
dotnet user-secrets set "Seed:AdminPassword" "Admin123!"
cd ../..
dotnet run --project src/AngelesTandas.Web
```

---

## 🌐 ACCESO A LA APP

**URL:** https://localhost:5001

### Credenciales (ya seeded)
- **Email:** `admin@local`
- **Password:** `Admin123!`

---

## 📋 FUNCIONALIDADES IMPLEMENTADAS

✅ **Autenticación & Autorización**
- ASP.NET Identity integrado
- Roles (Admin, User)
- Login/Logout
- Protección de páginas admin

✅ **Gestión de Tandas**
- Crear tandas (monto, participantes, turnos)
- Listar tandas
- Activar tandas
- Soft-delete (no hay borrado físico)

✅ **Gestión de Pagos**
- Crear pagos
- Subir comprobantes (desde navegador, multipart)
- Aprobar/rechazar pagos (admin)
- Completar pagos
- Transacciones garantizadas

✅ **Almacenamiento**
- Azure Blob Storage support (O LocalDB dev)
- Base de datos SQL Server (localdb)
- Todas las tablas creadas automáticamente

✅ **Auditoría**
- Log automático de todas las acciones
- CreatedAt, CreatedBy, ModifiedAt, ModifiedBy
- Soft-delete con auditoría

✅ **Admin Panel**
- Pagos pendientes por aprobar
- Edición de plantillas
- Control total

✅ **Upload de Archivos**
- Componente Blazor InputFile
- Multipart upload POST /api/payments/{paymentId}/receipt
- Persistencia en Blob o local

---

## 🏗️ ARQUITECTURA

```
src/
├── AngelesTandas.Web/              # Blazor Server (UI + API endpoints)
│   ├── Pages/                      # Páginas Razor/Blazor
│   ├── Shared/                     # Componentes (FileUpload.razor)
│   ├── Middleware/                 # CurrentUserMiddleware
│   └── Program.cs                  # Configuración, DI, Seeding, API endpoints
├── AngelesTandas.Application/      # Interfaces, DTOs
├── AngelesTandas.Domain/           # Entidades, interfaces de dominio
└── AngelesTandas.Infrastructure/   # EF Core, Servicios
    ├── Data/                       # ApplicationDbContext
    ├── Services/                   # PaymentService, TandaService, AuditService, SecurityService
    └── Identity/                   # ApplicationUser
```

---

## 🔐 SEGURIDAD

✅ HTTPS en desarrollo  
✅ ASP.NET Identity for user management  
✅ Role-based authorization  
✅ Soft-delete (no permanente)  
✅ Auditoría completa  
✅ User-secrets para dev  
✅ Transacciones en operaciones críticas  

---

## 📊 FUNCIONALIDAD END-TO-END

### 1. Admin crea Tanda
```
Login (admin@local/Admin123!) 
→ /tandas/create 
→ Ingresa: nombre, monto, participantes
→ Sistema asigna turnos/orden aleatorio
```

### 2. Usuario crea Pago
```
/payments/create
→ Selecciona monto
→ Sistema crea Payment con ID
→ Se muestra sección para subir comprobante
```

### 3. Usuario sube Comprobante
```
Selecciona archivo (JPG/PNG/PDF)
→ Se sube a servidor via multipart
→ Se persiste en Blob Storage (o local)
→ Se retorna URI
```

### 4. Admin aprueba Pago
```
/admin/payments/pending
→ Lista de pagos pendientes
→ Click "Aprobar" o "Rechazar"
→ Payment → "Approved" o "Rejected"
→ Auditoría registrada
```

### 5. Sistema completa Pago
```
Payment Approved 
→ CompletePaymentAsync
→ Verifica recibo
→ Payment → "Completed"
→ Turn marcado como "IsPaidOut"
```

---

## 🗄️ BASE DE DATOS

**Servidor:** (localdb)\mssqllocaldb  
**Base:** AngelesTandas  

### Tablas creadas automáticamente:
- AspNetUsers, AspNetRoles (Identity)
- Tandas, TandaParticipants, Turns
- Payments, PaymentReceipts
- AuditLogs
- NotificationTemplates, PaymentInstructionTemplates
- Profiles, CommissionRequests, CommissionResponses

---

## 🧪 PRUEBAS

### Verificar que todo funciona:

1. **Login**
   - Email: admin@local
   - Password: Admin123!
   - ✅ Debe loguear sin errores

2. **Crear Tanda**
   - Nombre: "Tanda Test"
   - Monto: 100
   - Participantes: 5
   - ✅ Debe crear exitosamente

3. **Crear Pago**
   - Monto: 100
   - ✅ Debe generar Payment ID

4. **Subir Comprobante**
   - Selecciona archivo
   - ✅ Debe cargarse sin errores

5. **Admin Panel**
   - /admin/payments/pending
   - ✅ Debe mostrar pagos pendientes

---

## 🚀 DEPLOY A PRODUCCIÓN

Para Azure:

1. **Azure SQL Database**
   ```
   ConnectionStrings:DefaultConnection = <azure-sql-connstring>
   ```

2. **Azure Storage Account**
   ```
   BlobStorage:ConnectionString = <azure-blob-connstring>
   BlobStorage:Container = receipts
   ```

3. **Azure Key Vault**
   - Guardar todos los secrets
   - Usar Managed Identity

4. **App Service**
   - Desplegar con `dotnet publish`
   - Ejecutar migrations en startup

5. **GitHub Actions**
   - CI/CD pipeline (build → test → migrate → deploy)

---

## ❓ SOPORTE

Si algo no funciona:

1. **Build OK?**
   ```bash
   dotnet clean
   dotnet restore
   dotnet build
   ```

2. **Database OK?**
   ```bash
   # Verificar en SQL Server Object Explorer (Visual Studio)
   # O usar SSMS en (localdb)\mssqllocaldb
   ```

3. **Secrets OK?**
   ```powershell
   cd src/AngelesTandas.Web
   dotnet user-secrets list
   ```

4. **Logs?**
   - Browser DevTools (F12)
   - Application Insights (una vez en Azure)

---

## 📞 ENTREGABLES

✅ App Web 100% funcional  
✅ Base de datos automática  
✅ Autenticación & Autorización  
✅ Upload de archivos (Blob Storage ready)  
✅ Admin panel  
✅ Auditoría completa  
✅ Transacciones garantizadas  
✅ Scripts de inicio (PowerShell, Batch, Bash)  
✅ Documentación  

---

**¡Tu app de Tandas está lista para usar! 🎉**

Ejecuta `.\run-tandas.ps1` (o el script de tu OS) y accede a https://localhost:5001
