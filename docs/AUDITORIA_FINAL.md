# Auditoría final y preparación para entrega

## 1. Resumen

El Sistema de Trazabilidad Logística y Gestión de Inventarios para Palma Verde S.A. completó los cuatro Sprints: seguridad/catálogos, recepción/inventario, producción por lotes y distribución/movimientos/trazabilidad/reportes. La auditoría no agregó alcance funcional.

## 2. Arquitectura

- `POS.Client`: Blazor WebAssembly y experiencia de usuario.
- `POS.Server`: API, JWT, autorización, negocio, transacciones, EF Core y SQL Server.
- `POS.Shared`: entidades y DTO sin dependencia hacia Client/Server.
- `POS.Tests`: xUnit con SQLite en memoria para reglas y rollback.

Dependencias correctas: Client → Shared; Server → Shared; Tests → Server/Shared. No hay dependencia inversa desde Shared.

## 3. Base de datos

SQL Server LocalDB: `(localdb)\\PalmaVerdeLocalDB`, base `PalmaVerdeDb`, EF Core 9.0.19. Se comprobaron físicamente las 12 tablas requeridas: ROL, USUARIO, PROVEEDOR, MATERIA_PRIMA, RECEPCION, DETALLE_RECEPCION, PRODUCTO, LOTE_PRODUCCION, DETALLE_LOTE_MATERIA_PRIMA, DESPACHO, DETALLE_DESPACHO y MOVIMIENTO_INVENTARIO.

Resultado SQL: 12 PK, 14 FK y 10 CHECK. Existen índices únicos para rol, usuario/email, código de lote y detalles sin duplicar. Los checks impiden stocks negativos y cantidades no positivas. No se modificaron migraciones históricas.

## 4. Seguridad

JWT valida firma, emisor, audiencia y expiración. BCrypt se usa al sembrar/registrar/cambiar contraseñas y al iniciar sesión. Los cuatro usuarios demo presentan hash BCrypt de longitud 60 y prefijo `$2a$`; no se almacena contraseña en texto plano en la base.

`UserResponse` excluye `PasswordHash`, contraseña y secretos. Usuario o rol inactivo impide login. Se revisaron todos los controladores: solo `/api/auth/login` es anónimo. Doce rutas representativas de administración, catálogos, recepción, producción, despachos, inventario, movimientos, trazabilidad y reportes devolvieron HTTP 401 sin JWT. Las modificaciones aplican roles en API, además de su visibilidad en Blazor.

La clave JWT fue retirada de `appsettings.json` y se configura mediante User Secrets en cada computadora de desarrollo. El repositorio conserva solamente emisor, audiencia y expiración. `POS.Server` detiene el inicio con un mensaje explícito si falta `JwtSettings:SecretKey`.

## 5. Transacciones

- Recepción: cabecera/detalles e incremento de materia prima dentro de transacción.
- Consumo: detalle, descuento de materia prima y estado EnProceso con rollback total.
- Finalización: lote, cantidad/fecha, incremento de producto y EntradaProduccion en la misma transacción.
- Despacho: cabecera, detalles, descuento, SalidaDespacho y confirmación atómicos.

Las pruebas fuerzan fallos durante los guardados y comprueban que no quedan registros ni stocks parciales.

## 6. Idempotencia

Está cubierta por pruebas: segunda confirmación de recepción no incrementa; segundo consumo no descuenta; segunda finalización no incrementa ni duplica movimiento; segunda confirmación de despacho no descuenta ni duplica salida.

## 7. Roles y permisos

Administrador gestiona administración y supervisa operaciones. Producción modifica lotes, consulta inventario/movimientos/trazabilidad y reporte de producción. Almacén registra recepciones/despachos y consulta inventario, movimientos, trazabilidad y reportes autorizados. Gerencia realiza exclusivamente consultas permitidas. En E2E, Gerencia recibió HTTP 403 intentando registrar un despacho.

## 8. Sprints

- Sprint 1: autenticación, JWT, BCrypt, roles, usuarios, proveedores y materias primas.
- Sprint 2: recepción, detalles, inventario de materia prima y productos.
- Sprint 3: lotes, consumo, producto terminado e idempotencia.
- Sprint 4: despachos, movimientos, trazabilidad y reportes.

## 9. Migraciones aplicadas

1. `20260818173150_InitialDB`
2. `20260819004804_InitialDB`
3. `20260821174316_PalmaVerdeSprint1`
4. `20260821180438_RoleLogicalDeletion`
5. `20260821190217_Sprint2ReceptionInventory`
6. `20260821191807_Sprint3ProductionLots`
7. `20260821193420_Sprint4DistributionMovements`

## 10. Pruebas

`dotnet build POS\\POS.sln`: 0 errores, 0 advertencias. `dotnet test POS\\POS.Tests\\POS.Tests.csproj`: 56 aprobadas, 0 fallidas, 0 omitidas. No se eliminó ninguna prueba.

## 11. End-to-end final

Fecha: 21/08/2026. Datos claramente identificados como auditoría:

- Almacén creó recepción #2 por 5 unidades: materia prima 603 → 608.
- Producción creó lote `LP-AUDITORIA-20260821155038` (#3), consumió 2: materia prima 608 → 606.
- Finalizó 8 unidades: producto 9 → 17 y se creó exactamente una EntradaProduccion.
- Almacén creó despacho #2 por 3: producto 17 → 14 y se creó exactamente una SalidaDespacho.
- Trazabilidad devolvió una materia prima y un movimiento relacionado.
- Reportes del día: 3 filas de producción, 4 de inventario y 2 de despacho.
- Gerencia consultó reportes y recibió HTTP 403 al intentar registrar despacho.

Los registros históricos de Sprint 2–4 se conservaron como evidencia de demostración.

## 12. Limitaciones

No existe FK desde consumo de lote a una recepción específica; por tanto no se afirma trazabilidad exacta proveedor → recepción → unidad producida. Exportación CSV/PDF/Excel queda como mejora futura. El sistema requiere SQL Server/LocalDB disponible y que cada computadora configure `JwtSettings:SecretKey` mediante User Secrets.

## 13. Ejecución

1. Instalar SDK/runtime .NET 9 y SQL Server LocalDB.
2. Configurar `JwtSettings:SecretKey` con `dotnet user-secrets set` en `POS.Server`.
3. Ejecutar `dotnet ef database update --project POS\\POS.Server\\POS.Server.csproj --startup-project POS\\POS.Server\\POS.Server.csproj`.
4. Ejecutar API con `dotnet run --project POS\\POS.Server --launch-profile http`.
5. Ejecutar cliente con `dotnet run --project POS\\POS.Client --launch-profile http`.
6. Abrir `http://localhost:5010`. Usuarios: admin, produccion, almacen y gerencia; contraseña demo `PalmaVerde2026!`.

## 14. Estado final

**LISTO PARA ENTREGA Y DEMOSTRACIÓN UNIVERSITARIA.** No se encontraron regresiones funcionales ni problemas críticos. Se corrigió únicamente la advertencia de redirección HTTPS del perfil HTTP en Development y se actualizó documentación obsoleta. En ambientes no Development la redirección HTTPS permanece activa.
