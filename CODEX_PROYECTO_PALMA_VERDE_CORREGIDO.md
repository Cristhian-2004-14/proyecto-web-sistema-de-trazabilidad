# Proyecto: Sistema de Trazabilidad Logística y Gestión de Inventarios para Palma Verde S.A.

## 1. Instrucción principal para Codex

Desarrollar un **prototipo académico funcional** de un Sistema Web de
Gestión de Producción e Inventario para **Palma Verde S.A.**, siguiendo
estrictamente los requerimientos, modelos y planificación definidos en
este documento.

El proyecto corresponde a un práctico universitario. La prioridad es
construir una solución clara, mantenible y demostrable, sin agregar
módulos empresariales fuera del alcance definido.

### Nombre oficial del proyecto

**Sistema de Trazabilidad Logística y Gestión de Inventarios para Palma
Verde S.A.**

Codex debe utilizar este nombre como denominación oficial en el
`README.md`, títulos visibles de la aplicación, documentación y
referencias generales al proyecto. No reemplazarlo por "Sistema de
Producción e Inventario" ni por otra variante.

### Regla importante

No inventar funcionalidades que contradigan este documento. Si una
decisión técnica no está especificada, elegir una alternativa simple,
convencional y fácil de explicar en una exposición universitaria,
documentándola en el README.

------------------------------------------------------------------------

## 2. Objetivo del sistema

Centralizar el flujo de producción e inventario de Palma Verde S.A.,
permitiendo:

-   Registrar proveedores y materias primas.
-   Registrar recepciones de materia prima.
-   Controlar existencias de materias primas.
-   Gestionar productos terminados.
-   Crear y dar seguimiento a lotes de producción.
-   Registrar las materias primas utilizadas en cada lote.
-   Registrar cantidades de producto terminado.
-   Controlar inventario de productos.
-   Registrar despachos.
-   Registrar movimientos de inventario.
-   Consultar trazabilidad de lotes.
-   Generar reportes básicos.
-   Controlar el acceso mediante usuarios y roles.

El sistema se limita al alcance académico de **producción e
inventario**. No implementar contabilidad, facturación, ventas, recursos
humanos ni otros módulos no especificados.

------------------------------------------------------------------------

## 3. Tipo de aplicación y arquitectura

Construir una **aplicación web responsiva de uso interno**.

Debe utilizar una arquitectura cliente-servidor con:

-   Front-end web.
-   Back-end con lógica de negocio.
-   Base de datos relacional centralizada.
-   Autenticación.
-   Autorización por roles.

La interfaz debe funcionar correctamente en navegadores modernos y ser
utilizable tanto desde escritorio como desde resoluciones móviles.

### Stack tecnológico

El informe académico no fija un framework o motor de base de datos
específico.

Por tanto, antes de generar el proyecto, Codex debe:

1.  Revisar el repositorio actual, si ya existe.
2.  Reutilizar el stack existente si es razonable.
3.  Si el repositorio está vacío, crear una arquitectura web
    convencional y documentar en `README.md` las tecnologías
    seleccionadas y la razón de su elección.
4.  Mantener una separación clara entre interfaz, lógica de negocio y
    persistencia.

------------------------------------------------------------------------

## 4. Roles del sistema

### Administrador

Puede:

-   Iniciar sesión.
-   Gestionar usuarios.
-   Gestionar roles.
-   Gestionar materias primas.
-   Gestionar productos.
-   Consultar inventario.
-   Consultar movimientos.
-   Generar reportes.
-   Supervisar operaciones.

### Encargado de Producción

Puede:

-   Iniciar sesión.
-   Registrar lotes de producción.
-   Registrar consumo de materia prima.
-   Registrar producto terminado.
-   Actualizar el estado de los lotes.
-   Consultar inventario.

### Encargado de Almacén

Puede:

-   Iniciar sesión.
-   Registrar recepciones.
-   Controlar inventario.
-   Registrar despachos.
-   Consultar movimientos.

### Gerencia

Puede:

-   Iniciar sesión.
-   Consultar inventario.
-   Consultar trazabilidad.
-   Consultar reportes de producción, inventario y despachos.

Implementar autorización para impedir que un usuario acceda a funciones
no permitidas por su rol.

------------------------------------------------------------------------

## 5. Requerimientos funcionales

### RF01 - Autenticación de Usuario

**Prioridad:** Alta.

Permitir el acceso seguro de usuarios registrados.

Entrada:

-   Usuario.
-   Contraseña.

Proceso:

-   Validar las credenciales.
-   Identificar el rol.
-   Habilitar únicamente las opciones correspondientes.

Salida:

-   Acceso al menú principal, o
-   Mensaje de credenciales inválidas.

------------------------------------------------------------------------

### RF02 - Gestión de Usuarios

**Prioridad:** Alta.

El Administrador debe poder:

-   Crear usuarios.
-   Asignar roles.
-   Modificar información básica.
-   Habilitar o deshabilitar cuentas.

Datos mínimos:

-   Nombre.
-   Apellido.
-   Usuario.
-   Contraseña.
-   Email.
-   Rol.
-   Estado.

------------------------------------------------------------------------

### RF03 - Registro de Materia Prima

**Prioridad:** Alta.

Registrar el ingreso de materias primas al almacén.

Debe permitir registrar:

-   Proveedor.
-   Materia prima.
-   Cantidad.
-   Unidad de medida.
-   Fecha.
-   Observaciones.

Al confirmar una recepción, actualizar las existencias correspondientes.

------------------------------------------------------------------------

### RF04 - Gestión de Lotes de Producción

**Prioridad:** Alta.

El Encargado de Producción podrá crear un lote indicando:

-   Producto.
-   Código de lote.
-   Fecha de inicio.
-   Cantidad planificada.
-   Materias primas asociadas.
-   Cantidad utilizada de cada materia prima.
-   Estado.

También podrá actualizar el estado del lote.

Cuando corresponda, el consumo de materia prima debe reflejarse en
inventario.

------------------------------------------------------------------------

### RF05 - Registro de Producto Terminado

**Prioridad:** Alta.

Registrar la cantidad de producto terminado obtenida de un lote.

Entrada:

-   Lote.
-   Producto.
-   Cantidad producida.

Al registrar el producto terminado:

-   Actualizar `cantidadProducida` del lote.
-   Incrementar el stock del producto correspondiente.
-   Registrar el movimiento necesario para mantener trazabilidad.

------------------------------------------------------------------------

### RF06 - Consulta de Inventario

**Prioridad:** Alta.

Permitir consultar existencias actuales de:

-   Materias primas.
-   Productos terminados.

Permitir búsqueda o filtros.

Mostrar al menos:

-   Nombre.
-   Unidad de medida.
-   Stock actual.
-   Stock mínimo.
-   Estado.

------------------------------------------------------------------------

### RF07 - Registro de Despacho

**Prioridad:** Alta.

El Encargado de Almacén podrá registrar la salida de productos
terminados.

Datos:

-   Usuario responsable.
-   Fecha.
-   Destino.
-   Observación.
-   Productos.
-   Cantidades.

Reglas:

-   Verificar disponibilidad antes de confirmar.
-   No permitir cantidades superiores al stock disponible.
-   Descontar las cantidades del inventario al confirmar.
-   Registrar los movimientos correspondientes.

------------------------------------------------------------------------

### RF08 - Historial de Movimientos

**Prioridad:** Media.

Permitir consultar movimientos por:

-   Producto.
-   Rango de fechas.
-   Tipo de movimiento.
-   Usuario responsable.

Los movimientos deben permitir identificar entradas y salidas relevantes
del inventario.

------------------------------------------------------------------------

### RF09 - Trazabilidad de Lotes

**Prioridad:** Alta.

Permitir seleccionar un lote y consultar:

-   Código del lote.
-   Fechas.
-   Producto.
-   Materias primas utilizadas.
-   Cantidades utilizadas.
-   Cantidad planificada.
-   Cantidad producida.
-   Estado.

------------------------------------------------------------------------

### RF10 - Generación de Reportes

**Prioridad:** Media.

Generar reportes por rango de fechas de:

-   Producción.
-   Inventario.
-   Despachos.

Como mínimo, permitir visualización en pantalla. Si la tecnología
elegida permite una exportación sencilla, puede implementarse sin
alterar el alcance principal.

------------------------------------------------------------------------

## 6. Requerimientos no funcionales

### RNF01 - Look and Feel

-   Interfaz consistente.
-   Menús claros.
-   Formularios legibles.
-   Mensajes comprensibles.

### RNF02 - Seguridad

-   Autenticación obligatoria.
-   No almacenar contraseñas en texto plano.
-   Utilizar hash de contraseña apropiado.
-   Aplicar autorización por rol.

### RNF03 - Restricción de contenido

Las pantallas y operaciones disponibles dependen del rol del usuario.

### RNF04 - Integridad de datos

Antes de guardar:

-   Validar campos obligatorios.
-   Validar cantidades.
-   Validar claves y relaciones.
-   Evitar registros inconsistentes.
-   Evitar stock negativo.

### RNF05 - Rendimiento

Las consultas comunes de inventario, lotes y movimientos deben responder
fluidamente bajo la carga esperada para un prototipo académico.

### RNF06 - Disponibilidad

El sistema estará disponible mientras servidor, base de datos y red
estén funcionando.

### RNF07 - Usabilidad

Un usuario con capacitación básica debe poder completar las operaciones
principales sin conocimientos de programación.

### RNF08 - Compatibilidad

La aplicación debe funcionar en navegadores actuales y adaptarse a
escritorio y móvil.

------------------------------------------------------------------------

## 7. Modelo de datos obligatorio

Implementar como mínimo las siguientes tablas.

### ROL

  Campo         Tipo sugerido   Restricción
  ------------- --------------- ---------------------
  idRol         INT             PK, autoincremental
  nombre        VARCHAR(50)     requerido
  descripcion   VARCHAR(150)    

### USUARIO

  Campo       Tipo sugerido   Restricción
  ----------- --------------- -----------------------------------
  idUsuario   INT             PK, autoincremental
  idRol       INT             FK -\> ROL.idRol
  nombre      VARCHAR(80)     requerido
  apellido    VARCHAR(80)     requerido
  usuario     VARCHAR(80)     requerido, único
  password    VARCHAR(225)    almacenar hash, nunca texto plano
  email       VARCHAR(100)    
  estado      BIT/BOOLEAN     requerido

### PROVEEDOR

  Campo         Tipo sugerido   Restricción
  ------------- --------------- ---------------------
  idProveedor   INT             PK, autoincremental
  nombre        VARCHAR(100)    requerido
  nit           VARCHAR(30)     
  telefono      VARCHAR(30)     
  direccion     VARCHAR(150)    
  estado        BIT/BOOLEAN     requerido

### RECEPCION

  Campo         Tipo sugerido   Restricción
  ------------- --------------- ------------------------------
  idRecepcion   INT             PK, autoincremental
  idProveedor   INT             FK -\> PROVEEDOR.idProveedor
  idUsuario     INT             FK -\> USUARIO.idUsuario
  fecha         DATETIME        requerido
  observacion   VARCHAR(300)    
  estado        VARCHAR(30)     

### DETALLE_RECEPCION

  Campo                Tipo sugerido   Restricción
  -------------------- --------------- -------------------------------------
  idDetalleRecepcion   INT             PK, autoincremental
  idRecepcion          INT             FK -\> RECEPCION.idRecepcion
  idMateriaPrima       INT             FK -\> MATERIA_PRIMA.idMateriaPrima
  cantidad             DECIMAL(18,2)   \> 0

### MATERIA_PRIMA

  Campo            Tipo sugerido   Restricción
  ---------------- --------------- ---------------------
  idMateriaPrima   INT             PK, autoincremental
  nombre           VARCHAR(100)    requerido
  unidadMedida     VARCHAR(30)     requerido
  stockActual      DECIMAL(18,2)   \>= 0
  stockMinimo      DECIMAL(18,2)   \>= 0
  estado           BIT/BOOLEAN     requerido

### PRODUCTO

  Campo          Tipo sugerido   Restricción
  -------------- --------------- ---------------------
  idProducto     INT             PK, autoincremental
  nombre         VARCHAR(120)    requerido
  descripcion    VARCHAR(250)    
  unidadMedida   VARCHAR(30)     requerido
  stockActual    DECIMAL(18,2)   \>= 0
  stockMinimo    DECIMAL(18,2)   \>= 0
  estado         BIT/BOOLEAN     requerido

### LOTE_PRODUCCION

  Campo                 Tipo sugerido   Restricción
  --------------------- --------------- ----------------------------
  idLote                INT             PK, autoincremental
  idProducto            INT             FK -\> PRODUCTO.idProducto
  idUsuario             INT             FK -\> USUARIO.idUsuario
  codigoLote            VARCHAR(50)     requerido, único
  fechaInicio           DATETIME        requerido
  fechaFin              DATETIME        nullable
  cantidadPlanificada   DECIMAL(18,2)   \> 0
  cantidadProducida     DECIMAL(18,2)   \>= 0
  estado                VARCHAR(30)     requerido

### DETALLE_LOTE_MATERIA_PRIMA

  Campo               Tipo sugerido   Restricción
  ------------------- --------------- -------------------------------------
  idDetalle           INT             PK, autoincremental
  idLote              INT             FK -\> LOTE_PRODUCCION.idLote
  idMateriaPrima      INT             FK -\> MATERIA_PRIMA.idMateriaPrima
  cantidadUtilizada   DECIMAL(18,2)   \> 0

### DESPACHO

  Campo         Tipo sugerido   Restricción
  ------------- --------------- --------------------------
  idDespacho    INT             PK, autoincremental
  idUsuario     INT             FK -\> USUARIO.idUsuario
  fecha         DATETIME        requerido
  destino       VARCHAR(150)    requerido
  observacion   VARCHAR(300)    
  estado        VARCHAR(30)     

### DETALLE_DESPACHO

  Campo               Tipo sugerido   Restricción
  ------------------- --------------- ----------------------------
  idDetalleDespacho   INT             PK, autoincremental
  idDespacho          INT             FK -\> DESPACHO.idDespacho
  idProducto          INT             FK -\> PRODUCTO.idProducto
  cantidad            DECIMAL(18,2)   \> 0

### MOVIMIENTO_INVENTARIO

  Campo            Tipo sugerido   Restricción
  ---------------- --------------- ----------------------------
  idMovimiento     INT             PK, autoincremental
  idProducto       INT             FK -\> PRODUCTO.idProducto
  idUsuario        INT             FK -\> USUARIO.idUsuario
  tipoMovimiento   VARCHAR(30)     requerido
  cantidad         DECIMAL(18,2)   \> 0
  fecha            DATETIME        requerido
  referencia       VARCHAR(100)    

> Nota de alcance: el modelo académico proporcionado vincula
> `MOVIMIENTO_INVENTARIO` con `PRODUCTO`. Mantener ese modelo como base.
> No rediseñar silenciosamente el esquema para materias primas;
> cualquier mejora estructural adicional debe documentarse antes de
> aplicarse.

------------------------------------------------------------------------

## 8. Relaciones obligatorias

Implementar exactamente estas relaciones base:

``` text
ROL
1 --------- N
     USUARIO

PROVEEDOR
1 --------- N
     RECEPCION

USUARIO
1 --------- N
     RECEPCION

RECEPCION
1 --------- N
     DETALLE_RECEPCION

MATERIA_PRIMA
1 --------- N
     DETALLE_RECEPCION

PRODUCTO
1 --------- N
     LOTE_PRODUCCION

USUARIO
1 --------- N
     LOTE_PRODUCCION

LOTE_PRODUCCION
1 --------- N
     DETALLE_LOTE_MATERIA_PRIMA

MATERIA_PRIMA
1 --------- N
     DETALLE_LOTE_MATERIA_PRIMA

USUARIO
1 --------- N
     DESPACHO

DESPACHO
1 --------- N
     DETALLE_DESPACHO

PRODUCTO
1 --------- N
     DETALLE_DESPACHO

PRODUCTO
1 --------- N
     MOVIMIENTO_INVENTARIO

USUARIO
1 --------- N
     MOVIMIENTO_INVENTARIO
```

### Relaciones N:M resultantes

Conceptualmente:

-   RECEPCION N:M MATERIA_PRIMA, resuelta mediante `DETALLE_RECEPCION`.
-   LOTE_PRODUCCION N:M MATERIA_PRIMA, resuelta mediante
    `DETALLE_LOTE_MATERIA_PRIMA`.
-   DESPACHO N:M PRODUCTO, resuelta mediante `DETALLE_DESPACHO`.

------------------------------------------------------------------------

## 9. Reglas de negocio

Implementar estas reglas desde la capa de negocio, no solamente desde la
interfaz:

1.  No permitir acceso sin autenticación.
2.  Un usuario solo puede ejecutar operaciones permitidas por su rol.
3.  No guardar contraseñas en texto plano.
4.  No permitir cantidades menores o iguales a cero en detalles de
    recepción, producción o despacho.
5.  No permitir que el stock sea negativo.
6.  Una recepción confirmada debe incrementar el stock de materia prima.
7.  El consumo de materia prima de un lote debe disminuir el stock
    disponible cuando la operación quede confirmada.
8.  No permitir consumir más materia prima que la disponible.
9.  Registrar producto terminado debe incrementar el stock del producto.
10. No permitir despachar más producto que el stock disponible.
11. Confirmar un despacho debe disminuir el stock del producto.
12. Los movimientos que afecten inventario deben registrar fecha y
    usuario responsable.
13. El código de lote debe permitir identificar un lote de manera única.
14. Las operaciones críticas de inventario deben ejecutarse de forma
    transaccional para evitar actualizaciones parciales.
15. Preferir cambios de `estado` antes que eliminación física de
    registros que formen parte del historial.

------------------------------------------------------------------------

## 10. Product Backlog

Usar el siguiente backlog como guía de implementación:

  ID     Funcionalidad                             Prioridad   Puntos
  ------ --------------------------------------- ----------- --------
  PB01   Inicio de sesión                               Alta        3
  PB02   Gestión de usuarios y roles                    Alta        5
  PB03   Gestión de proveedores                         Alta        3
  PB04   Gestión de materias primas                     Alta        5
  PB05   Registrar recepción                            Alta        8
  PB06   Consultar inventario de materia prima          Alta        5
  PB07   Gestión de productos                          Media        3
  PB08   Registrar lote de producción                   Alta        8
  PB09   Registrar consumo de materia prima             Alta        8
  PB10   Registrar producto terminado                   Alta        5
  PB11   Registrar despacho                             Alta        8
  PB12   Movimientos de inventario                     Media        5
  PB13   Trazabilidad de lotes                         Media        8
  PB14   Generar reportes                              Media        8

------------------------------------------------------------------------

## 11. Plan de implementación

El práctico se planifica entre el **18 y el 26 de agosto de 2026**.

### Sprint 1 - 18 y 19 de agosto

Implementar PB01-PB04:

-   Inicio de sesión.
-   Usuarios.
-   Roles.
-   Proveedores.
-   Materias primas.

**Resultado esperado:** módulo de acceso y administración básica
funcional.

### Sprint 2 - 20 y 21 de agosto

Implementar PB05-PB07:

-   Recepciones.
-   Detalle de recepción.
-   Actualización de stock de materia prima.
-   Consulta de inventario.
-   Gestión de productos.

**Resultado esperado:** recepción e inventario de materias primas
funcional.

### Sprint 3 - 22 y 23 de agosto

Implementar PB08-PB10:

-   Lotes de producción.
-   Detalle de materias primas por lote.
-   Consumo de insumos.
-   Registro de producto terminado.
-   Actualización de existencias.

**Resultado esperado:** producción por lotes y actualización de producto
terminado funcional.

### Sprint 4 - 24 al 26 de agosto

Implementar PB11-PB14:

-   Despachos.
-   Detalles de despacho.
-   Movimientos de inventario.
-   Trazabilidad.
-   Reportes.
-   Integración.
-   Pruebas finales.

**Resultado esperado:** distribución, movimientos, trazabilidad,
reportes e integración final.

------------------------------------------------------------------------

## 12. Definition of Done

Una funcionalidad solamente se considera terminada cuando:

-   Cumple la historia o requerimiento correspondiente.
-   Cumple sus validaciones.
-   Almacena correctamente la información.
-   Respeta las relaciones de la base de datos.
-   Está integrada con los módulos relacionados.
-   Supera pruebas funcionales sin errores críticos.
-   Está integrada con la versión principal.
-   Puede demostrarse durante una Sprint Review.

------------------------------------------------------------------------

## 13. Riesgos que deben considerarse durante el desarrollo

  -----------------------------------------------------------------------
  ID                      Riesgo                  Mitigación
  ----------------------- ----------------------- -----------------------
  R01                     Cambios de              Repriorizar el Product
                          requerimientos          Backlog

  R02                     Retraso en tareas       Redistribuir trabajo y
                                                  revisar avances

  R03                     Errores en base de      Revisar modelos y
                          datos                   probar con datos

  R04                     Pérdida o               Respaldos, validaciones
                          inconsistencia de       y control de
                          información             operaciones

  R05                     Falta de experiencia    Investigación y apoyo
                          tecnológica             entre integrantes

  R06                     Problemas de            Integración incremental
                          integración             y pruebas continuas

  R07                     Ausencia temporal de un Documentar avances y
                          integrante              compartir repositorio

  R08                     Tiempo insuficiente     Realizar pruebas en
                          para pruebas            cada Sprint

  R09                     Fallas en control de    Validar entradas,
                          stock                   salidas y evitar stock
                                                  negativo

  R10                     Alcance mayor al tiempo Mantener prioridades y
                          disponible              posponer funciones no
                                                  esenciales
  -----------------------------------------------------------------------

------------------------------------------------------------------------

## 14. Orden recomendado para Codex

Trabajar incrementalmente. No intentar implementar todo el sistema en un
único cambio.

### Fase 0 - Preparación

1.  Inspeccionar el repositorio.
2.  Identificar stack existente.
3.  Crear o actualizar `README.md`.
4.  Configurar variables de entorno.
5.  Configurar conexión a base de datos.
6.  Crear estructura inicial del proyecto.
7.  Crear migraciones/esquema inicial.
8.  Agregar datos de prueba mínimos.

### Fase 1 - Acceso y catálogos

1.  ROL.
2.  USUARIO.
3.  Login/logout.
4.  Autorización.
5.  PROVEEDOR.
6.  MATERIA_PRIMA.

### Fase 2 - Recepción e inventario

1.  RECEPCION.
2.  DETALLE_RECEPCION.
3.  Transacción de confirmación.
4.  Actualización de stock.
5.  Consulta de inventario.
6.  PRODUCTO.

### Fase 3 - Producción

1.  LOTE_PRODUCCION.
2.  DETALLE_LOTE_MATERIA_PRIMA.
3.  Validación de disponibilidad.
4.  Descuento de materia prima.
5.  Registro de producto terminado.
6.  Incremento de stock del producto.

### Fase 4 - Distribución

1.  DESPACHO.
2.  DETALLE_DESPACHO.
3.  Validación de stock.
4.  Descuento de producto.
5.  MOVIMIENTO_INVENTARIO.

### Fase 5 - Consultas finales

1.  Historial de movimientos.
2.  Trazabilidad de lotes.
3.  Reportes.
4.  Pruebas.
5.  Corrección de errores.
6.  Datos demo.

------------------------------------------------------------------------

## 15. Pantallas mínimas

Crear como mínimo:

1.  Inicio de sesión.
2.  Dashboard o menú principal según rol.
3.  Gestión de usuarios.
4.  Gestión de roles.
5.  Gestión de proveedores.
6.  Gestión de materias primas.
7.  Registro/listado de recepciones.
8.  Consulta de inventario de materias primas.
9.  Gestión de productos.
10. Registro/listado de lotes.
11. Pantalla para consumo de materia prima.
12. Registro de producto terminado.
13. Registro/listado de despachos.
14. Historial de movimientos.
15. Trazabilidad de lote.
16. Reportes.

No priorizar diseño visual complejo. Priorizar claridad, funcionamiento,
validaciones y facilidad de demostración.

------------------------------------------------------------------------

## 16. Datos de prueba

Crear datos demo que permitan mostrar el sistema sin configuración
manual extensa.

Incluir como mínimo:

-   Roles: Administrador, Producción, Almacén y Gerencia.
-   Un usuario demo por rol.
-   Algunos proveedores.
-   Varias materias primas.
-   Algunos productos.
-   Al menos una recepción.
-   Al menos un lote.
-   Al menos un despacho cuando el flujo lo permita.

Las contraseñas demo deben configurarse mediante el mecanismo seguro de
autenticación seleccionado y documentarse únicamente como credenciales
de entorno de desarrollo.

------------------------------------------------------------------------

## 17. Pruebas mínimas

Agregar pruebas o verificaciones para los flujos críticos:

-   Login válido.
-   Login inválido.
-   Restricción por rol.
-   Crear proveedor.
-   Registrar recepción.
-   Aumento de stock tras recepción.
-   Crear lote.
-   Rechazar consumo superior al stock.
-   Registrar producto terminado.
-   Incrementar stock de producto.
-   Rechazar despacho superior al stock.
-   Descontar stock al despachar.
-   Consultar trazabilidad.

------------------------------------------------------------------------

## 18. README del proyecto

El `README.md` del repositorio debe explicar:

-   Nombre del proyecto.
-   Propósito.
-   Tecnologías utilizadas.
-   Arquitectura general.
-   Requisitos previos.
-   Instalación.
-   Configuración de base de datos.
-   Migraciones.
-   Cómo ejecutar el proyecto.
-   Usuarios demo.
-   Roles.
-   Funcionalidades implementadas.
-   Estructura del proyecto.
-   Estado de cada Sprint.
-   Limitaciones conocidas.

------------------------------------------------------------------------

## 19. Criterio para tomar decisiones técnicas

Cuando exista una decisión que este documento no especifique:

1.  No modificar los requerimientos funcionales.
2.  Elegir la alternativa más simple que sea segura y mantenible.
3.  Evitar dependencias innecesarias.
4.  Evitar sobreingeniería.
5.  Mantener el código fácil de explicar a un docente.
6.  Registrar la decisión en el README si afecta la arquitectura.

------------------------------------------------------------------------

## 20. Primera tarea para Codex

Comenzar por **Sprint 1**.

Antes de escribir código:

1.  Analizar el repositorio completo.
2.  Resumir qué existe actualmente.
3.  Proponer la estructura concreta que se utilizará.
4.  Identificar archivos que se crearán o modificarán.
5.  Verificar que el modelo de datos respete este documento.

Después implementar:

-   Base del proyecto.
-   Base de datos y migración inicial.
-   ROL.
-   USUARIO.
-   Autenticación.
-   Autorización por roles.
-   PROVEEDOR.
-   MATERIA_PRIMA.
-   CRUDs necesarios del Sprint 1.
-   Validaciones.
-   Datos demo.
-   Pruebas básicas.

Al terminar, entregar un resumen de:

-   Archivos creados/modificados.
-   Funcionalidades terminadas.
-   Pruebas ejecutadas.
-   Resultado de las pruebas.
-   Pendientes.
-   Próximo paso correspondiente al Sprint 2.

**No avanzar automáticamente al Sprint 2 hasta comprobar que Sprint 1
funciona correctamente.**
