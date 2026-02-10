# 🎉 AngelesTandas - App Web de Tandas

## Punto. APP WEB. Sin contenedores, sin registros, sin complicaciones.

---

## ✅ QUÉ ES

Una app web simple y funcional para gestionar tandas online:

- ✅ Crear tandas
- ✅ Gestionar pagos
- ✅ Subir comprobantes
- ✅ Admin aprueba/rechaza
- ✅ Auditoría de acciones
- ✅ Login seguro

**Tecnología:** Blazor Server (.NET 10) + SQL Server

---

## 🚀 EJECUTAR (1 minuto)

```powershell
.\run-tandas.ps1
```

Abre: https://localhost:5001

**Login:**
- Email: `admin@local`
- Contraseña: Se muestra en consola (copiar)

---

## 📂 CARPETAS PRINCIPALES

```
src/
├── AngelesTandas.Web/          ← LA APP (Blazor UI)
├── AngelesTandas.Application/  ← Servicios
├── AngelesTandas.Domain/       ← Entidades
└── AngelesTandas.Infrastructure/ ← Base de datos
```

---

## 💡 USAR LA APP

### 1. Login
Ingresa con admin@local

### 2. Crear Tanda
Menú → Crear Tanda → Llena form → Guardar

### 3. Crear Pago
Menú → Crear Pago → Ingresa monto → Subir comprobante

### 4. Admin aprueba
Menú → Admin → Pagos Pendientes → Aprobar/Rechazar

---

## ✨ CARACTERÍSTICAS

- **Seguro:** Contraseñas fuertes, login seguro, auditoría
- **Rápido:** Responde al instante
- **Confiable:** Base de datos con soft-delete (sin borrar físico)
- **Limpio:** Interfaz simple y fácil de usar

---

## 🆘 PROBLEMAS

### No arranca
```powershell
dotnet clean
dotnet restore
dotnet run --project src/AngelesTandas.Web
```

### Puerto ocupado
```powershell
netstat -ano | findstr :5001
taskkill /PID <número> /F
```

### Contraseña admin perdida
Se regenera cada vez que ejecutas (se muestra en consola)

---

## 📚 DOCUMENTACIÓN

- `QUICKSTART.md` - Inicio rápido
- `SECURITY.md` - Seguridad implementada
- `DEPLOY_TO_AZURE.md` - Desplegar a Azure (opcional)

---

## 🎯 ESO ES TODO

Una app web de tandas. Funciona. Punto.

**Ejecuta `.\run-tandas.ps1` y listo.**
