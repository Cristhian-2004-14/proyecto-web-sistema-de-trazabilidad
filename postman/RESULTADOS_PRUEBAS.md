# Evidencia de pruebas de endpoints - Palma Verde

Fecha de ejecución: 24 de agosto de 2026  
API evaluada: `http://localhost:5216`  
Colección: `Palma Verde API - Evidencia de autenticación`

## Resultado

| # | Prueba | HTTP esperado | HTTP obtenido | Estado |
|---|---|---:|---:|---|
| 1 | Login correcto con Administrador | 200 | 200 | APROBADA |
| 2 | Login con contraseña incorrecta | 401 | 401 | APROBADA |
| 3 | Consulta de usuarios sin JWT | 401 | 401 | APROBADA |
| 4 | Consulta de usuarios con JWT Administrador | 200 | 200 | APROBADA |
| 5 | Consulta de roles activos | 200 | 200 | APROBADA |
| 6 | Registro de usuario mediante endpoint protegido | 200 | 200 | APROBADA |
| 7 | Login con usuario Gerencia | 200 | 200 | APROBADA |
| 8 | Gerencia intenta administrar usuarios | 403 | 403 | APROBADA |

## Comprobaciones adicionales

- El login correcto devolvió un JWT.
- La respuesta pública de usuarios no contiene `PasswordHash` ni propiedades de contraseña.
- Se confirmó un rol Administrador activo con identificador 1.
- El usuario creado por la prueba utiliza el prefijo `postman.` y una marca de tiempo para evitar duplicados.
- Un usuario autenticado con rol Gerencia recibe 403 al consultar la administración de usuarios.
- La clave JWT utilizada por la API fue cargada desde .NET User Secrets y no desde `appsettings.json`.

## Archivos para Postman

- Colección: `postman/collections/PalmaVerde_API.postman_collection.json`
- Entorno: `postman/environments/PalmaVerde_Local.postman_environment.json`

La colección contiene scripts Post-response que comprueban automáticamente códigos HTTP, recepción del JWT, rol, ausencia de `PasswordHash` y autorización por roles.

Por seguridad, las variables `adminPassword`, `managementPassword` y `testPassword` se publican vacías. Deben completarse solamente en el valor local del entorno de Postman y no sincronizarse como valores compartidos.
