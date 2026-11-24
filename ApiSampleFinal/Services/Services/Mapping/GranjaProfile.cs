using AutoMapper;
using Domain.Entities;
using Services.Dtos;

namespace Services.Mapping
{
    /// <summary>Mapeos de entidades de dominio a DTOs. Las entidades nunca salen de la capa de servicios.</summary>
    public class GranjaProfile : Profile
    {
        public GranjaProfile()
        {
            CreateMap<ProductoParametro, ProductoParametroDto>()
                .ForMember(d => d.TipoProducto, o => o.MapFrom(s => s.Tipo));

            CreateMap<Granja, GranjaResponse>()
                .ForMember(d => d.Productos, o => o.Ignore());

            CreateMap<Vaca, VacaResponse>();

            CreateMap<ProduccionLeche, ProduccionVacaResponse>()
                .ForMember(d => d.CodigoVaca, o => o.MapFrom(s => s.Vaca != null ? s.Vaca.Codigo : string.Empty));

            // El estado depende de la fecha de consulta; lo asigna el servicio.
            CreateMap<LoteProducto, LoteResponse>()
                .ForMember(d => d.TipoProducto, o => o.MapFrom(s => s.Tipo))
                .ForMember(d => d.Estado, o => o.Ignore());

            CreateMap<VentaDetalle, VentaDetalleResponse>()
                .ForMember(d => d.FechaProduccion, o => o.MapFrom(s => s.Lote!.FechaProduccion));

            CreateMap<Venta, VentaResponse>()
                .ForMember(d => d.Detalle, o => o.MapFrom(s => s.Detalles));

            CreateMap<Venta, ReporteVentaItem>()
                .ForMember(d => d.VentaId, o => o.MapFrom(s => s.Id));
        }
    }
}
