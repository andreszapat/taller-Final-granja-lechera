# Registro de revisión del proyecto

## Revisión de correspondencia con `taller final.pdf`

- Se identificaron las 10 reglas RN-01 a RN-10.
- Se modelaron 4 proyectos de arquitectura más 1 de pruebas.
- Se añadieron todos los endpoints de reportes de la página 6 y el reporte de vacas adicional.
- Se incluyó migración inicial, SQLite, repositorios, IUnitOfWork, IClock, AutoMapper, Swagger y ProblemDetails.
- Se añadieron solicitudes `.http` ordenadas para repetir el escenario de referencia de las páginas 7 y 8, más un escenario independiente de vacas con producción distinta para demostrar el reporte solicitado en la página 6.
- Las pruebas xUnit contienen los supuestos y las cifras esperadas del escenario de la guía, y verifican mayor, menor, promedio y vacas bajo promedio.
- Se revisaron los archivos de proyecto y sus referencias, los archivos JSON y la estructura de carpetas.
- Se ejecutó el SQL de la migración inicial en SQLite en memoria; se crearon las ocho tablas esperadas y se comprobó que el índice único impide duplicar producción para una vaca en la misma fecha.
- Se verificaron mediante operaciones matemáticas independientes los resultados del escenario de referencia y el ejemplo de vacas (16, 14 y 8 veces 15 L).

## Restricción de verificación

**No se ha ejecutado ni compilado el código C# dentro de este entorno** porque no hay SDK de .NET instalado y la conexión a los servidores de instalación está bloqueada. Por esa razón, el estado final de compilación y el resultado real de `dotnet test` permanecen **pendientes de verificar en un equipo con .NET 10**. No se debe afirmar que las pruebas pasan sin haber ejecutado el proyecto.

## Comandos de validación indispensables

```bash
dotnet restore Granja.sln
dotnet build Granja.sln --no-restore
dotnet test Granja.sln --no-restore
dotnet run --project Granja.Web --urls http://localhost:5000
```

Usar Swagger en `/swagger` o `Granja.http` y comprobar las respuestas contra la tabla de referencia del README.

## Alcance de las conclusiones

Las cifras de referencia se pueden verificar aritméticamente con los datos del enunciado. La validación estática de archivos y cálculos **no equivale** a una compilación, a pruebas HTTP reales ni a una evaluación del profesor.

## Corrección de fallo real detectado en Visual Studio (20/11/2026)

Captura: `System.InvalidOperationException` al iniciar, `PendingModelChangesWarning`, llamado desde `DbSeeder.InicializarAsync` y `Program.cs`. La migración inicial fue escrita a mano: su snapshot no es del diseñador de EF Core.

**Modificaciones incorporadas:**
- `Granja.Web/Program.cs`: se omite el diagnóstico de divergencia del snapshot manual, sin omitir la aplicación de migraciones (`MigrateAsync` sigue en `DbSeeder`). Es una excepción documentada para el taller, **no** sustituye una migración regenerada por EF en un proyecto en evolución.
- `Granja.Domain/Interfaces/IRepositorio.cs`: se desambigua la entidad `Granja` para resolver el CS0118 que apareció antes.
- `Granja.Infrastructure/Granja.Infrastructure.csproj` y `Granja.Tests/Granja.Tests.csproj`: EF Core 10.0.15.
- `Granja.Tests/ReglasNegocioTests.cs`: prueba `MigracionInicialTests.LaMigracionInicialPermiteArrancarYSembrarLaGranja()` verifica aplicación y registro de la migración inicial real, seeding y segunda inicialización.

**Estado honesto de la verificación:** se validó con SQLite estándar que las instrucciones SQL de migración se pueden ejecutar y que se crean las tablas, índices y claves foráneas; esta comprobación **no** equivale a ejecutar ASP.NET Core o xUnit. Falta ejecutar en Visual Studio la prueba de integración, compilar y revisar NuGet.
