# Granja Lechera API

Taller final de Lenguajes de Programación (USC). API Web en .NET 10 con arquitectura en capas para
gestionar una granja lechera: producción de leche, transformación en queso o yogurt, inventario por
lotes con salida FIFO y vencimiento, ventas y reportes.

## Tecnologías

- .NET 10, ASP.NET Core Web API
- Entity Framework Core 10 con migraciones (SQLite por defecto, SQL Server opcional)
- AutoMapper (entidades → DTOs)
- Swagger / OpenAPI (Swashbuckle)
- ProblemDetails para el manejo global de errores
- xUnit para pruebas unitarias

## Estructura y dependencias

```text
ApiSampleFinal/
├── Domain/Domain/                 Núcleo del negocio, sin dependencias externas
│   ├── Entities/                  Granja, ProductoParametro, Vaca, ProduccionLeche, LoteProducto, Venta, VentaDetalle
│   ├── Enums/                     TipoProducto, EstadoLote
│   ├── Exceptions/                InventarioInsuficiente, LecheInsuficiente, ProduccionDuplicada, ...
│   └── Interfaces/                IRepository, ILoteRepository, IVentaRepository, IUnitOfWork, IClock, ...
├── Infrastructure/Infrastructure/ Acceso a datos
│   ├── Data/                      GranjaDbContext, Configurations (Fluent API + datos semilla), Migrations
│   ├── Repositories/              Implementación de los repositorios
│   └── UnitOfWork.cs              UnitOfWork y SystemClock
├── Services/Services/             Casos de uso
│   ├── Dtos/                      DTOs de entrada/salida y de reportes
│   ├── Implementations/           GranjaService, VacaService, ProduccionService, InventarioService, VentaService, ReporteService
│   └── Mapping/                   Perfil de AutoMapper
├── Web/                           Exposición HTTP y raíz de composición
│   ├── Controllers/               Granja, Vacas, Produccion, Inventario, Ventas, Reportes
│   ├── Middleware/                GlobalExceptionHandler (ProblemDetails)
│   ├── Program.cs                 Inyección de dependencias, Swagger, migraciones al iniciar
│   ├── ApiSampleFinal.http        Colección de pruebas (escenario de referencia completo)
│   └── ApiSampleFinal.sln         Solución
└── Test/Granja.Tests/             Pruebas unitarias xUnit
```

```text
Web ──► Services ──► Domain ◄── Infrastructure
 └─────────────────────────────────┘ (solo en Program.cs para registrar dependencias)
```

- **Domain** no referencia a nadie. Define las entidades con sus reglas (vencimiento, descuento,
  paso a dañado) y las interfaces que necesita (repositorios, `IUnitOfWork`, `IClock`).
- **Infrastructure** implementa esas interfaces con EF Core.
- **Services** usa solo las interfaces del dominio; contiene la lógica FIFO, validaciones y reportes,
  y devuelve DTOs (las entidades no salen de esta capa).
- **Web** referencia Infrastructure solo para registrar dependencias. Los controllers no usan el
  `DbContext` ni tienen lógica de negocio: reciben la petición, llaman un servicio y responden.

## Modelo y reglas de negocio

| Entidad | Atributos principales |
|---|---|
| Granja | Nombre, NumeroVacas (n), LitrosPorVaca (l); `ProduccionDiariaEsperada = n × l` |
| ProductoParametro | Tipo, Unidad, LitrosPorUnidad (rendimiento), VidaUtilDias, Precio |
| Vaca | Codigo, Raza, Activa |
| ProduccionLeche | VacaId, Fecha, Litros (única por fecha y vaca) |
| LoteProducto | Tipo, FechaProduccion, FechaVencimiento, LitrosUsados, CantidadInicial, CantidadDisponible, CantidadDanada |
| Venta | Fecha, TipoProducto, Cantidad, PrecioUnitario, Total, Detalles |
| VentaDetalle | VentaId, LoteId, Cantidad (trazabilidad FIFO) |

Respecto al diagrama del enunciado (Farm–Cow–Milk / Inventory–Product–Venta): la leche se modela como
`ProduccionLeche` por vaca y día (para poder hacer el reporte de vacas); el inventario es el conjunto
de `LoteProducto`, y Queso/Yogurt se distinguen con `TipoProducto` más los parámetros configurables en
`ProductoParametro`, en lugar de subclases. Así el rendimiento (10 L/kg o 150 g/L), la vida útil
(30/15 días o 2 semanas) y el precio se cambian por API sin tocar el código.

| Regla | Dónde se cumple |
|---|---|
| RN-01 producción n × l, una vez por fecha y vaca | `ProduccionService` + índice único (Fecha, VacaId) |
| RN-02 no transformar más litros de los producidos ese día | `ProduccionService.TransformarAsync` → `LecheInsuficienteException` (409) |
| RN-03 cantidad = litros ÷ rendimiento | `ProductoParametro.CalcularCantidad` |
| RN-04 lote con producción, vencimiento, cantidades | constructor de `LoteProducto` |
| RN-05/06 ventas ad libitum, FIFO por fecha de producción | `LoteRepository.ObtenerVendiblesAsync` + `VentaService` |
| RN-07 venta atómica, 409 si no alcanza | validación previa + un solo `SaveChangesAsync` |
| RN-08 vencido si fecha > vencimiento; procesar pasa a dañado | `LoteProducto.EstaVencido` / `ProcesarVencimiento` |
| RN-09 trazabilidad de lotes en la venta | `Venta.AgregarDetalle` |
| RN-10 fecha opcional, por defecto `IClock` | todos los servicios |

## Ejecutar

Requisitos: SDK de .NET 10.

```powershell
cd D:\Projects\taller-granja\ApiSampleFinal\Web
dotnet run --launch-profile http
```

Abrir http://localhost:5081/swagger. Al iniciar se aplican las migraciones y se crea `granja.db`
con los datos semilla: n = 10 vacas (V-001 … V-010), l = 15 L, queso 10 L/kg · 30 días · $28.000,
yogurt 1 L/L · 15 días · $6.000. Para empezar de cero basta con borrar `granja.db`.

Para usar SQL Server, en `appsettings.json` cambie `"DatabaseProvider": "SqlServer"` y la cadena
`DefaultConnection` (las migraciones incluidas están generadas para SQLite; para SQL Server genere
unas nuevas).

Migraciones (herramienta local incluida en `dotnet-tools.json`):

```powershell
cd D:\Projects\taller-granja\ApiSampleFinal
dotnet tool restore
dotnet ef migrations add NombreMigracion --project Infrastructure\Infrastructure --startup-project Web --output-dir Data\Migrations
```

## Pruebas

```powershell
cd D:\Projects\taller-granja\ApiSampleFinal\Web
dotnet test ApiSampleFinal.sln
```

19 pruebas: reglas del lote en el dominio y, sobre los servicios reales con SQLite en memoria y un
reloj falso, todas las pruebas mínimas del punto 9 más el escenario de referencia completo del punto 8.

La colección `Web/ApiSampleFinal.http` (VS / VS Code REST Client / Rider) ejecuta el escenario de
referencia paso a paso, los reportes y los casos de error 400/404/409. Correrla sobre una base nueva.

## Endpoints

### Operaciones

| Método | Ruta | Descripción | Respuestas |
|---|---|---|---|
| GET | `/api/granja` | Configuración (n, l) y parámetros por producto | 200 |
| PUT | `/api/granja` | Actualiza n, l, rendimiento, vida útil y precio; deja n vacas activas | 200, 400, 404 |
| GET | `/api/vacas` | Lista de vacas | 200 |
| GET | `/api/vacas/{id}` | Una vaca | 200, 404 |
| POST | `/api/vacas` | Agrega una vaca (n + 1) | 201, 400, 409 |
| POST | `/api/produccion` | Registra la producción del día de todas las vacas (n × l, con ajustes opcionales por vaca) | 201, 400, 404, 409 |
| POST | `/api/produccion/vaca` | Registra la producción de una vaca | 201, 400, 404, 409 |
| GET | `/api/produccion?desde=&hasta=` | Producción por día | 200, 400 |
| GET | `/api/produccion/{fecha}` | Producción de un día con detalle por vaca | 200, 404 |
| POST | `/api/produccion/transformaciones` | Transforma litros de una fecha en queso o yogurt (crea un lote) | 201, 400, 409 |
| GET | `/api/inventario?tipo=&fecha=&incluirAgotados=` | Inventario a una fecha: vendible por producto y lotes | 200 |
| GET | `/api/inventario/lotes/{id}` | Un lote | 200, 404 |
| POST | `/api/inventario/procesar-vencimientos` | Pasa a dañado lo disponible de los lotes vencidos | 200 |
| GET | `/api/inventario/proximos-a-vencer?dias=3&fecha=` | Alerta de lotes próximos a vencer | 200, 400 |
| POST | `/api/ventas` | Registra una venta FIFO | 201, 400, 409 |
| GET | `/api/ventas/{id}` | Una venta con su detalle de lotes | 200, 404 |
| GET | `/api/ventas?desde=&hasta=&tipo=` | Lista de ventas | 200, 400 |

### Reportes

| Ruta | Contenido |
|---|---|
| `/api/reportes/ventas?desde=&hasta=&tipo=` | Ventas: fecha, producto, cantidad, precio, total y total general |
| `/api/reportes/leche-diaria?desde=&hasta=` | Litros producidos, transformados y no aprovechados por día |
| `/api/reportes/producto-diario?desde=&hasta=` | Producto elaborado por día y por tipo |
| `/api/reportes/producto-total` | Por producto: producido, vendido, disponible y dañado |
| `/api/reportes/producto-danado?desde=&hasta=` | Dañado por lote y por tipo, con fecha de vencimiento |
| `/api/reportes/vacas?desde=&hasta=` | Producción promedio, mayor y menor productora, vacas bajo el promedio |

### Ejemplo de venta

```http
POST /api/ventas
{ "tipoProducto": "Queso", "cantidad": 12, "fecha": "2026-12-03" }
```

```json
{
  "id": 1, "fecha": "2026-12-03", "tipoProducto": "Queso", "cantidad": 12,
  "precioUnitario": 28000, "total": 336000,
  "detalle": [
    { "loteId": 1, "fechaProduccion": "2026-12-01", "cantidad": 10 },
    { "loteId": 3, "fechaProduccion": "2026-12-02", "cantidad": 2 }
  ]
}
```

### Resultado del escenario de referencia (verificado)

| Producto | Producido | Vendido | Disponible | Dañado |
|---|---|---|---|---|
| Queso (kg) | 25 | 12 | 13 | 0 |
| Yogurt (L) | 50 | 20 | 0 | 30 |

Leche por día: 150 L el día 1 y 150 L el día 2. La venta de 5 L de yogurt el día 17 responde 409.

## Errores

Todas las respuestas de error son `ProblemDetails`:

| Excepción | HTTP |
|---|---|
| Validación de DTO / `DomainException` | 400 |
| `EntidadNoEncontradaException` | 404 |
| `InventarioInsuficienteException`, `LecheInsuficienteException`, `ProduccionDuplicadaException`, `DatoDuplicadoException` | 409 |
| Cualquier otra | 500 |
