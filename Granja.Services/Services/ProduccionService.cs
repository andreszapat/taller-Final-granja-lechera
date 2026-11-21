using Granja.Domain.Entities;
using Granja.Domain.Enums;
using Granja.Domain.Exceptions;
using Granja.Domain.Interfaces;
using Granja.Services.Dtos;
namespace Granja.Services.Services;
public sealed class ProduccionService(IGranjaRepository granjas, IVacaRepository vacas,
    IProduccionRepository producciones, ITransformacionRepository transformaciones,
    IConfiguracionProductoRepository productos, IClock clock, IUnitOfWork uow)
{
    public async Task<ProduccionResponse> RegistrarLecheAsync(ProduccionRequest req, CancellationToken ct = default)
    {
        if (req is null) throw new ValidacionNegocioException("Envíe la fecha y opcionalmente las vacas.");
        var fecha = req.Fecha ?? clock.Today;
        var g = await granjas.ObtenerAsync(ct) ?? throw new RecursoNoEncontradoException("Falta la configuración de granja.");
        var activas = (await vacas.ListarAsync(ct)).Where(v => v.Activa).ToList();
        if (activas.Count != g.NumeroVacas) throw new ConflictoNegocioException("La cantidad de vacas activas no coincide con la configuración.");
        var delDia = await producciones.ListarAsync(fecha, fecha, ct);
        if (delDia.Count != 0) throw new ConflictoNegocioException($"Ya existe producción de leche para {fecha:yyyy-MM-dd}.");
        var entradas = req.Vacas is { Count: > 0 }
            ? req.Vacas : activas.Select(x => new ProduccionVacaRequest(x.Id, g.LitrosPorVaca)).ToList();
        if (entradas.Count != activas.Count || entradas.GroupBy(x => x.VacaId).Any(x => x.Count() > 1)
            || entradas.Any(x => x.Litros < 0) || entradas.Any(x => activas.All(v => v.Id != x.VacaId)))
            throw new ValidacionNegocioException("Debe registrar cada vaca activa exactamente una vez con litros no negativos.");
        var esperado = g.NumeroVacas * g.LitrosPorVaca;
        if (entradas.Sum(x => x.Litros) != esperado)
            throw new ValidacionNegocioException($"La producción total debe ser n × l = {esperado} L. Puede distribuirlos entre las vacas para el reporte.");
        await producciones.AgregarVariasAsync(entradas.Select(x => new ProduccionLeche(x.VacaId, fecha, x.Litros)), ct);
        await uow.SaveChangesAsync(ct);
        return new(fecha, esperado, entradas);
    }
    public async Task<TransformacionResponse> TransformarAsync(TransformacionRequest req, CancellationToken ct = default)
    {
        if (req is null || !Enum.IsDefined(req.TipoProducto) || req.Litros <= 0)
            throw new ValidacionNegocioException("Indique un producto válido y litros positivos.");
        var fecha = req.Fecha ?? clock.Today;
        var delDia = await producciones.ListarAsync(fecha, fecha, ct);
        if (delDia.Count == 0) throw new ConflictoNegocioException("Primero registre la producción de leche para esa fecha.");
        var producidos = delDia.Sum(x => x.Litros);
        var consumidos = (await transformaciones.ListarAsync(fecha, fecha, ct)).Sum(x => x.LitrosUsados);
        if (req.Litros > producidos - consumidos)
            throw new LecheInsuficienteException(req.Litros, producidos - consumidos);
        var p = await productos.ObtenerAsync(req.TipoProducto, ct)
            ?? throw new RecursoNoEncontradoException("Producto sin parámetros configurados.");
        var t = new Transformacion(req.TipoProducto, fecha, req.Litros, p.RendimientoLitrosPorUnidad, p.VidaUtilDias);
        await transformaciones.AgregarAsync(t, ct);
        await uow.SaveChangesAsync(ct);
        return new(t.Id, t.Lote.Id, fecha, t.Tipo, t.LitrosUsados, t.CantidadObtenida, t.Lote.FechaVencimiento);
    }
}
