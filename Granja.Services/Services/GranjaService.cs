using AutoMapper;
using Granja.Domain.Entities;
using Granja.Domain.Enums;
using Granja.Domain.Exceptions;
using Granja.Domain.Interfaces;
using Granja.Services.Dtos;
namespace Granja.Services.Services;
public sealed class GranjaService(IGranjaRepository granjas, IVacaRepository vacas,
    IConfiguracionProductoRepository productos, IUnitOfWork uow)
{
    public async Task<GranjaConfigResponse> ObtenerAsync(CancellationToken ct = default)
    {
        var granja = await granjas.ObtenerAsync(ct) ?? throw new RecursoNoEncontradoException("La granja no está configurada.");
        var configs = await productos.ListarAsync(ct);
        var q = configs.Single(x => x.Tipo == TipoProducto.Queso);
        var y = configs.Single(x => x.Tipo == TipoProducto.Yogurt);
        return new(granja.NumeroVacas, granja.LitrosPorVaca, granja.NumeroVacas * granja.LitrosPorVaca,
            new(q.RendimientoLitrosPorUnidad, q.VidaUtilDias, q.PrecioUnitario),
            new(y.RendimientoLitrosPorUnidad, y.VidaUtilDias, y.PrecioUnitario));
    }
    public async Task<GranjaConfigResponse> ActualizarAsync(GranjaActualizarRequest req, CancellationToken ct = default)
    {
        if (req is null || req.Queso is null || req.Yogurt is null)
            throw new ValidacionNegocioException("Debe indicar la granja y ambos productos.");
        var g = await granjas.ObtenerAsync(ct) ?? throw new RecursoNoEncontradoException("No existe configuración de granja.");
        // Validar todos los valores antes de persistir cambios.
        if (req.NumeroVacas <= 0 || req.NumeroVacas > 10000 || req.LitrosPorVaca <= 0 ||
            req.Queso.RendimientoLitrosPorUnidad <= 0 || req.Yogurt.RendimientoLitrosPorUnidad <= 0 ||
            req.Queso.VidaUtilDias <= 0 || req.Yogurt.VidaUtilDias <= 0 ||
            req.Queso.PrecioUnitario < 0 || req.Yogurt.PrecioUnitario < 0)
            throw new ValidacionNegocioException("Número de vacas, litros, rendimientos y vida útil deben ser positivos; los precios no negativos.");
        var q = await productos.ObtenerAsync(TipoProducto.Queso, ct) ?? throw new RecursoNoEncontradoException("Falta el queso.");
        var y = await productos.ObtenerAsync(TipoProducto.Yogurt, ct) ?? throw new RecursoNoEncontradoException("Falta el yogurt.");
        var todas = await vacas.ListarAsync(ct);
        var activas = todas.Where(v => v.Activa).ToList();
        if (req.NumeroVacas < activas.Count)
            foreach (var v in activas.OrderByDescending(x => x.Id).Take(activas.Count - req.NumeroVacas)) v.Desactivar();
        else if (req.NumeroVacas > activas.Count)
        {
            var faltantes = req.NumeroVacas - activas.Count;
            foreach (var v in todas.Where(x => !x.Activa).Take(faltantes)) { v.Activar(); faltantes--; }
            var siguienteCodigo = todas.Select(x => x.Codigo).Where(x => x.StartsWith("V") && int.TryParse(x[1..], out _))
                .Select(x => int.Parse(x[1..])).DefaultIfEmpty(0).Max() + 1;
            for (var i = 0; i < faltantes; i++) await vacas.AgregarAsync(new Vaca($"V{siguienteCodigo + i:0000}", $"Vaca {siguienteCodigo + i}"), ct);
        }
        g.Configurar(req.NumeroVacas, req.LitrosPorVaca);
        q.Configurar(req.Queso.RendimientoLitrosPorUnidad, req.Queso.VidaUtilDias, req.Queso.PrecioUnitario);
        y.Configurar(req.Yogurt.RendimientoLitrosPorUnidad, req.Yogurt.VidaUtilDias, req.Yogurt.PrecioUnitario);
        await uow.SaveChangesAsync(ct);
        return await ObtenerAsync(ct);
    }
}
public sealed class VacaService(IVacaRepository vacas, IUnitOfWork uow, IMapper mapper)
{
    public async Task<List<VacaResponse>> ListarAsync(CancellationToken ct = default)
        => mapper.Map<List<VacaResponse>>(await vacas.ListarAsync(ct));
    public async Task<VacaResponse> RenombrarAsync(int id, RenombrarVacaRequest req, CancellationToken ct = default)
    {
        var vaca = await vacas.ObtenerAsync(id, ct) ?? throw new RecursoNoEncontradoException($"No existe la vaca {id}.");
        vaca.Renombrar(req?.Nombre ?? "");
        await uow.SaveChangesAsync(ct);
        return mapper.Map<VacaResponse>(vaca);
    }
}
