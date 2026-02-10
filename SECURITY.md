# 🔐 SEGURIDAD DE ACCESO - ANGELO TANDAS

## Especificaciones de Seguridad Implementadas

### ✅ Contraseñas
- **Requisitos Mínimos:**
  - 12 caracteres mínimo
  - 1 mayúscula (A-Z)
  - 1 minúscula (a-z)
  - 1 dígito (0-9)
  - 1 carácter especial (!@#$%^&*)
  - 4 caracteres únicos mínimo

- **Generación de Contraseñas Seguras:**
  - Admin: Se genera automáticamente en primer startup
  - Mostrada UNA sola vez en consola (guardar seguro)
  - NO almacenada en appsettings.json

- **Cambio de Contraseña:**
  - Admin DEBE cambiar contraseña en primer login
  - Todos pueden cambiar contraseña en `/account/change-password`
  - Se requiere contraseña actual para cambiar

### 🔒 Autenticación

- **ASP.NET Identity:**
  - Hash seguro de contraseñas (PBKDF2 con SHA256)
  - Email único requerido
  - Confirmación de email (configurable)

- **Bloqueo de Cuenta:**
  - 5 intentos fallidos → bloqueo 15 minutos automático
  - Desbloquer automático después del tiempo
  - Log de intentos fallidos en auditoría

- **Roles:**
  - `Admin` - gestión completa, aprobación de pagos
  - `User` - participante estándar

- **Sesiones:**
  - Timeout: 4 horas de inactividad
  - Cookie HttpOnly (no accesible desde JS)
  - Secure flag (HTTPS only en producción)
  - SameSite=Strict (CSRF protection)

### 🌐 Transporte

- **HTTPS Obligatorio:**
  - Redirect automático HTTP → HTTPS
  - HSTS Header (Strict-Transport-Security)
  - HSTS Preload habilitado

- **Cookies Seguras:**
  - HttpOnly (no accesible desde JavaScript)
  - Secure (solo HTTPS en producción)
  - SameSite=Strict (CSRF protection)

### 📋 Auditoría

- **Registros Automáticos:**
  - Creación de usuarios admin
  - Cambios de contraseña
  - Intentos de login fallidos
  - Acciones de admin (aprobación/rechazo)
  - Uploads de archivos

- **Campos Auditados:**
  - CreatedAt / CreatedBy
  - ModifiedAt / ModifiedBy
  - Soft-delete (no borrado físico)

### 💾 Almacenamiento

- **Credenciales NO en:**
  - appsettings.json
  - Código fuente
  - Repositorio Git

- **Usar en lugar:**
  - User-Secrets (desarrollo local)
  - Azure Key Vault (producción)
  - Environment Variables (containerizado)

### 📸 Archivos de Usuarios

- **Validación:**
  - Solo formatos permitidos: .jpg, .jpeg, .png, .pdf
  - Tamaño máximo: 10 MB
  - Tipo MIME validado

- **Almacenamiento:**
  - Azure Blob Storage (encriptado en reposo)
  - URI accesible solo para usuario + admin

### 🛡️ Protección Cross-Site

- **CSRF (Cross-Site Request Forgery):**
  - Tokens automáticos en formularios Blazor
  - Validation en side-by-side

- **XSS (Cross-Site Scripting):**
  - Escape automático en Razor
  - Content-Security-Policy headers

### 2️⃣ Autenticación de Dos Factores (2FA)

- **Estado:** ✅ Habilitado para Admin
- **Pendiente:** Implementar QR code generator + TOTP

### 🔑 Gestión de Secretos (Producción)

```powershell
# Desarrollo (User-Secrets)
cd src/AngelesTandas.Web
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<conn-string>"
dotnet user-secrets set "Seed:AdminEmail" "admin@empresa.com"

# Producción (Azure Key Vault)
# Usar Managed Identity + Key Vault reference en App Configuration
```

### 📊 Credenciales Iniciales

**Admin (Generado automáticamente):**
- Email: `admin@local` (configurable)
- Contraseña: Generada automáticamente
  - Mostrada en consola al startup
  - DEBE ser cambiada en primer login
  - No almacenada permanentemente

**Participantes:**
- Pueden registrarse (si habilitado)
- Deben cumplir política de contraseñas
- Bloqueo automático por intentos fallidos

### ✅ Checklist de Seguridad

- [x] Política de contraseñas fuerte
- [x] Bloqueo por intentos fallidos
- [x] Timeout de sesión
- [x] Cookies seguras (HttpOnly, Secure, SameSite)
- [x] HTTPS obligatorio
- [x] HSTS header
- [x] Auditoría completa
- [x] No hardcoding de credenciales
- [x] Validación de archivos
- [x] 2FA para admin (habilitado)
- [x] Soft-delete (no borrado físico)
- [ ] Recuperación de contraseña por email
- [ ] TOTP/QR para 2FA (próximo)
- [ ] Rate limiting en login
- [ ] IP whitelist para admin

### 🚀 Deploy Seguro (Azure)

```bash
# 1. Key Vault
az keyvault create --name tandas-kv --resource-group tandas

# 2. Guardar secretos
az keyvault secret set --vault-name tandas-kv --name "ConnectionStrings--DefaultConnection" --value "<connstr>"
az keyvault secret set --vault-name tandas-kv --name "BlobStorage--ConnectionString" --value "<blobstr>"

# 3. App Service con Managed Identity
az appservice identity assign --name tandas-app --resource-group tandas
az keyvault set-policy --name tandas-kv --object-id <managed-identity-id> --secret-permissions get list

# 4. App Configuration para referencia
{
  "ConnectionStrings": {
    "DefaultConnection": "@Microsoft.KeyVault(VaultName=tandas-kv;SecretName=ConnectionStrings--DefaultConnection)"
  }
}
```

### 📞 Soporte

Para cambiar política de contraseñas o 2FA:
- Editar `Program.cs` → opciones de `IdentityOptions`
- Requiere rebuild y redeploy

---

**Última actualización:** 2024  
**Estado:** ✅ Producción Ready
