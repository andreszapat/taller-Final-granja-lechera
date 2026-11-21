# Taller final — Granja Lechera (Universidad Santiago de Cali)

**Asignatura:** Lenguajes de Programación  
**Docente:** Camilo Ernesto Charry Caicedo  
**Fecha de la guía:** 20 de noviembre de 2026  
**Tecnologías:** ASP.NET Core / .NET 10, C#, Entity Framework Core, SQLite, AutoMapper 17, Swagger/OpenAPI y xUnit.

## 1. ¿Qué hace el sistema?

API REST con cuatro capas independientes más proyecto de pruebas:

- `Granja.Domain`: entidades, reglas del negocio y contratos de repositorios / reloj / unidad de trabajo. No referencia otras capas.
- `Granja.Infrastructure`: SQLite mediante EF Core, migración inicial, repositorios, `SystemClock`, `UnitOfWork` y datos iniciales.
- `Granja.Services`: servicios de casos de uso, lógica FIFO, DTOs, validaciones, reportes y perfil AutoMapper.
- `Granja.Web`: controladores, rutas HTTP, inyección de dependencias, Swagger y manejo global de errores `ProblemDetails`.
- `Granja.Tests`: pruebas automatizadas xUnit con SQLite en memoria y reloj fijo.

Las dependencias son `Web -> Services -> Domain`; `Web -> Infrastructure -> Domain`; `Infrastructure -> Domain`. `Services` depende exclusivamente de `Domain` y no conoce EF Core.

## 2. Requisitos previos

1. Instalar **SDK .NET 10** (no solo el runtime).
2. Tener conexión a internet la primera vez para restaurar los paquetes NuGet.
3. Abrir una terminal en la carpeta que contiene `Granja.sln`.
4. Puede utilizarse Visual Studio 2026 o VS Code con C# Dev Kit.

## 3. Ejecución paso a paso

```bash
dotnet restore Granja.sln
dotnet build Granja.sln
dotnet test Granja.sln
dotnet run --project Granja.Web --urls http://localhost:5000
```

- Swagger: **http://localhost:5000/swagger**
- Las peticiones se hacen a `http://localhost:5000/api/...`.
- La base de datos SQLite `granja.db` se crea automáticamente al arrancar por primera vez. EF Core aplica la migración `Inicial` y carga datos de referencia (10 vacas, 15 L por vaca, queso 10 L/kg con vida 30 días y precio $28.000 COP/kg, yogurt 1 L/L con vida 15 días y precio de ejemplo $6.000 COP/L).
- Para empezar un **escenario limpio**, detener el servidor, eliminar el archivo `granja.db` si es una base de pruebas, y volver a arrancar. **Esto elimina todos los datos guardados; no hacerlo con datos reales.**

**Alternativa para migraciones manuales:** si dispone de `dotnet-ef` y desea inspeccionar el estado, ejecute `dotnet ef migrations list --project Granja.Infrastructure --startup-project Granja.Web`. El proyecto ya contiene una migración inicial escrita para SQLite.

**Licencia de AutoMapper:** la versión 17 registra avisos si falta una clave de licencia; esos avisos no bloquean la ejecución técnica, pero debe revisarse el tipo de licencia aplicable antes de distribuir o utilizar comercialmente la aplicación.

## Modelo del dominio (apartado 5 de la guía)

El código completa y adapta el diagrama inicial de la página 4. Estas son las entidades con sus atributos principales (algunos atributos auxiliares de persistencia no se muestran):

| Entidad | Atributos principales y función |
|---|---|
| `Granja` | `Id`, `NumeroVacas`, `LitrosPorVaca`: configuración de n y l. |
| `Vaca` | `Id`, `Codigo`, `Nombre`, `Activa`: individualiza a las vacas para sus informes. |
| `ProduccionLeche` | `Id`, `VacaId`, `Fecha`, `Litros`: litros diarios por vaca, sin registros repetidos. |
| `ConfiguracionProducto` | `Tipo`, `RendimientoLitrosPorUnidad`, `VidaUtilDias`, `PrecioUnitario`: parámetros editables. |
| `Transformacion` | `Id`, `Fecha`, `Tipo`, `LitrosUsados`, `CantidadObtenida`: leche convertida en producto. |
| `LoteProducto` | `Id`, `TransformacionId`, `Tipo`, `FechaProduccion`, `FechaVencimiento`, `CantidadInicial`, `CantidadDisponible`, `CantidadDanada`: inventario por lote. |
| `Venta` | `Id`, `Fecha`, `Tipo`, `Cantidad`, `PrecioUnitario`, `Total`: venta registrada. |
| `VentaDetalle` | `Id`, `VentaId`, `LoteId`, `FechaProduccion`, `Cantidad`: salida FIFO de cada lote. |

**Relaciones:** una granja configura sus vacas; una vaca tiene muchos registros de producción; cada transformación origina un lote; una venta contiene uno o más detalles y cada detalle identifica el lote que suministró el producto. Las operaciones transforman la leche de una fecha sin almacenar leche no utilizada como inventario.

## 4. API: rutas y métodos

| Método | Ruta | Función |
|---|---|---|
| GET | `/api/granja` | Configuración general y parámetros de los dos productos |
| PUT | `/api/granja` | Cambiar cantidad de vacas, litros, rendimiento, vida útil y precios |
| GET | `/api/vacas` | Consultar vacas y sus identificadores |
| PUT | `/api/vacas/{id}` | Editar nombre de una vaca |
| POST | `/api/produccion/leche` | Registrar n×l litros para una fecha; distribuidos por vaca |
| POST | `/api/produccion/transformaciones` | Convertir litros de una fecha a un producto y crear su lote |
| GET | `/api/inventario?tipo=&fecha=` | Consultar lotes y estado al día indicado |
| POST | `/api/inventario/procesar-vencimientos` | Mover existencias vencidas a dañadas |
| POST | `/api/ventas` | Vender aplicando FIFO y trazabilidad de lotes |
| GET | `/api/reportes/ventas?desde=&hasta=&tipo=` | Ventas del período y total general |
| GET | `/api/reportes/leche-diaria?desde=&hasta=` | Producido, transformado, no aprovechado |
| GET | `/api/reportes/producto-diario?desde=&hasta=` | Producción diaria por producto |
| GET | `/api/reportes/producto-total` | Producido, vendido, disponible y dañado por producto |
| GET | `/api/reportes/producto-danado?desde=&hasta=` | Cantidad dañada, por lote, con vencimiento |
| GET | `/api/reportes/vacas?desde=&hasta=` | Promedio, mayor, menor y por debajo del promedio |

Fechas: `yyyy-MM-dd` (ISO). Son opcionales en las operaciones que las permiten: sin fecha se usa `IClock.Today`. Los tipos de producto deben ir como `"Queso"` o `"Yogurt"`.

## 5. Reglas de negocio

- **RN-01:** se registra una vez por fecha, para todas las vacas activas; índice único `(Fecha, VacaId)`. Por defecto todas generan `l` litros; opcionalmente se admiten litros diferentes por vaca, pero la suma debe seguir siendo `n×l`, para hacer relevante el informe de mejores/peores vacas.
- **RN-02:** cada transformación utiliza leche de la fecha indicada; no puede superar la leche de ese día menos lo ya transformado. La no transformada no se almacena como inventario.
- **RN-03 / RN-04:** el rendimiento, precio y vida útil se consultan en la configuración; cada transformación crea un lote independiente.
- **RN-05 / RN-06:** las ventas no tienen tope diario, solo límite de existencia; se despacha FIFO por fecha de producción y por Id en empates.
- **RN-07:** comprobación total del stock antes del primer descuento; 409 si es insuficiente; EF Core guarda venta y lotes conjuntamente en una transacción. La columna `ConcurrencyToken` en cada lote ayuda a detectar escrituras concurrentes.
- **RN-08:** fecha de venta `>` fecha de vencimiento significa vencido. El lote todavía se puede vender en su fecha de vencimiento. El procesamiento de lotes vencidos es idempotente.
- **RN-09:** cada venta registra el precio de la fecha de venta, cantidad, total y cantidades tomadas de cada lote.
- **RN-10:** fecha explícita opcional o reloj `IClock` inyectado, reemplazable en pruebas.

**Nota sobre cifras:** `Queso` se mide en **kg** y `Yogurt` en **L**; en los reportes no debe sumarse indiscriminadamente kg con L.

**Nota sobre el reporte de leche:** el valor `litrosNoAprovechados` representa la leche registrada que no se transformó; no se conserva para otro día.

**Nota sobre el reporte de producto total:** la columna `Disponible` expresa existencias físicas pendientes de procesar o vender. La columna adicional `VencidoPendienteDeProcesar` informa cuánto de ese stock ya no es vendible a la fecha del reloj. Para obtener la clasificación definitiva, ejecute `procesar-vencimientos` con la fecha de corte de sus reportes.

**Reporte de producto dañado:** `desde` y `hasta` filtran **fecha de vencimiento**, no fecha de procesamiento; se devuelve además la fecha de procesamiento.

## 6. Escenario obligatorio de la guía (diciembre de 2026)

Los ejemplos ordenados se pueden enviar desde Swagger o desde `Granja.http` (con VS Code REST Client).

1. `2026-12-01`: registrar leche: 150 L.
2. `2026-12-01`: transformar 100 L en queso y 50 L en yogurt. Genera Q1 = 10 kg (vence 2026-12-31) y Y1 = 50 L (vence 2026-12-16).
3. `2026-12-02`: registrar leche 150 L y transformar 150 L en queso: Q2 = 15 kg.
4. `2026-12-03`: vender 12 kg de queso. FIFO toma 10 de Q1 y 2 de Q2. Precio 28.000, total **336.000 COP**.
5. `2026-12-03`: vender 20 L de yogurt, dejando 30 L.
6. `2026-12-17`: procesar vencimientos (Y1: 30 L dañados).
7. `2026-12-17`: intentar vender otros 5 L de yogurt. Resultado: **409 Conflict**.

**Producto total esperado:**

| Producto | Producido | Vendido | Disponible | Dañado |
|---|---:|---:|---:|---:|
| Queso (kg) | 25 | 12 | 13 | 0 |
| Yogurt (L) | 50 | 20 | 0 | 30 |

**Leche diaria esperada:** 150 L el 1 de diciembre y 150 L el 2 de diciembre. El proyecto de pruebas contiene `EscenarioDeReferenciaCoincideConLaGuia` para verificar esas cantidades.

## 7. Reporte adicional de las vacas

Se calcula el promedio **por día registrado de cada vaca** (sumatoria de litros / número de días registrados). El promedio global corresponde al promedio de los promedios de las vacas con datos dentro del período. Se identifican todas las vacas empatadas en el máximo o el mínimo, y las que quedan por debajo del promedio general. Si ninguna tiene registros en el período, se devuelve una colección vacía y promedio 0.

Ejemplo de producción diferenciada con `POST /api/produccion/leche`: 16 L para la primera vaca, 14 L para la segunda y 15 L para las 8 restantes. Total = 150 L; promedio = 15 L, mayor = 16 L, menor = 14 L y solo la segunda queda por debajo de la media.

### Comprobación concreta del reporte de vacas (página 6)

El archivo `Granja.http` contiene, después del escenario principal, dos solicitudes que demuestran el informe adicional. Utilice la base recién inicializada. La primera registra el día **2026-12-18** con 10 vacas: V0001 = 16 L, V0002 = 14 L y las otras ocho = 15 L cada una. La suma es **150 L**, respetando `n × l`. La segunda solicita el informe filtrado a ese día:

```http
GET http://localhost:5000/api/reportes/vacas?desde=2026-12-18&hasta=2026-12-18
```

**Resultado matemáticamente esperado (no presentado como ejecución verificada):**

| Indicador solicitado por el docente | Resultado esperado |
|---|---|
| Promedio de producción por vaca | **15 L/día** |
| Vaca que más produce | **V0001**, 16 L/día |
| Vaca que menos produce | **V0002**, 14 L/día |
| Vacas por debajo del promedio | **V0002**, 14 L/día |

En el ejemplo del escenario principal (días 1 y 2), las diez vacas registran 15 L diarios y **todas empatan** como mayor y menor producción; no hay ninguna por debajo del promedio. El ejemplo adicional demuestra también el caso en que sí hay diferencias. La prueba `ProduccionPorVacaEsUnicaYDebeSumarNxL` compara este mismo resultado y comprueba los cuatro requisitos.

## 8. Validaciones y errores

- `400 Bad Request`: producto inválido, parámetros negativos, fecha mal formada, entradas de vacas incompletas o litros diarios que no suman n×l.
- `404 Not Found`: configuración o vaca no existentes.
- `409 Conflict`: registro duplicado de fecha, falta de leche, inventario insuficiente o colisión de escritura.
- `201 Created`: alta de producción, transformación y venta.
- `200 OK`: consultas, actualización de configuración, procesamiento de vencimientos.

Los errores de negocio se convierten globalmente a JSON `ProblemDetails`; los controladores no exponen entidades de EF Core.

## 9. Pruebas

`dotnet test Granja.sln` ejecuta los tests de `Granja.Tests/ReglasNegocioTests.cs`, cubriendo:

- Producción n×l y rechazo de duplicación.
- Suma n×l con producciones individuales distintas y reporte por vaca.
- Transformación que excede la leche disponible.
- FIFO (lote viejo antes que nuevo) y venta que cubre varios lotes.
- Vencidos no vendidos.
- Venta insuficiente sin cambios en inventario ni venta persistida.
- Proceso de vencimiento, idempotencia y daño reportado.
- Escenario completo de referencia con reportes.

**Verificación pendiente en el equipo de ejecución:** este paquete se preparó en un entorno sin SDK de .NET; por tanto, no se ejecutó aquí `dotnet restore`, `dotnet build` ni `dotnet test`. Es imprescindible realizar esos comandos en un computador con SDK .NET 10 antes de sostener que el programa compila y pasa todas las pruebas.

## 10. Entrega Git

El repositorio local está organizado en la carpeta `GranjaLechera`. Si todavía no existe repositorio Git, en su equipo ejecute:

```bash
git init
git add .
git commit -m "Taller final API granja lechera"
```

Después cree un repositorio en GitHub o en el servicio indicado por el profesor y configure el remoto usando las instrucciones que le proporcione el servicio. **El archivo ZIP no es por sí mismo un enlace remoto de GitHub.**

## 11. Nota sobre decisiones que no estaban completamente fijadas en la guía

Los precios iniciales (28.000 COP/kg de queso y 6.000 COP/L de yogurt) son datos de ejemplo para reproducir la venta de la página 6; el docente exige que sean configurables. El texto que aparece junto al diagrama en la página 4 menciona dos semanas, mientras que la tabla de la página 2 y el escenario de prueba usan 30 días para queso y 15 para yogurt: se aplicó la tabla y ambos valores permanecen configurables. Las extensiones opcionales (alertas, exportación CSV/Excel y paginación avanzada) **no están incluidas** para priorizar el alcance obligatorio.

## 12. ¿Qué se entrega según el punto 10 de la guía?

**El entregable académico es el repositorio Git con el código fuente de la solución**, no un PDF de respuestas. El repositorio debe incluir:

1. Los cuatro proyectos obligatorios (`Domain`, `Infrastructure`, `Services`, `Web`) y `Granja.Tests` (en este paquete se incluyen).
2. La configuración de Entity Framework Core, SQLite y la migración inicial (incluidas).
3. Swagger/OpenAPI y manejo de errores (incluidos en el proyecto Web; **ejecución aún no validada**).
4. Un `README.md` con instrucciones y una colección de pruebas (`Granja.http`) (incluidos).
5. Código con las pruebas unitarias del punto 9 y el reporte de vacas adicional del punto 6.2 (incluidos; **resultados de ejecución pendientes de comprobar con el SDK .NET 10**).

La guía **no exige capturas de pantalla, presentación en PowerPoint ni PDF de respuestas**. Sí califica la capacidad de **sustentar** las decisiones técnicas. La publicación en un servicio de Git remoto requiere una cuenta o acceso del estudiante; este paquete contiene un repositorio local con historial, **no** un enlace a un repositorio remoto. Antes de enviarlo, se necesita compilar, probar, ejecutar las solicitudes de referencia y publicar el repositorio según las instrucciones del docente o de la plataforma.
