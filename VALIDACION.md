# Registro de revisión del proyecto

## Revisión de correspondencia con `taller final.pdf`

- Se identificaron las 10 reglas RN-01 a RN-10.
- Se modelaron 4 proyectos de arquitectura más 1 de pruebas.
- Se añadieron todos los endpoints de reportes de la página 6 y el reporte de vacas adicional.
- Se incluyó migración inicial, SQLite, repositorios, IUnitOfWork, IClock, AutoMapper, Swagger y ProblemDetails.
- Se añadieron solicitudes `.http` ordenadas para repetir el escenario de referencia de las páginas 7 y 8.
- Las pruebas xUnit contienen los supuestos y las cifras esperadas del escenario de la guía.
- Se revisaron los archivos de proyecto, referencias y JSON con herramientas locales de lectura.

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
