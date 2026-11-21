using AutoMapper;
using Granja.Domain.Entities;
using Granja.Services.Dtos;
namespace Granja.Services.Mapping;
public sealed class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Vaca, VacaResponse>();
        CreateMap<LoteProducto, InventarioLoteResponse>()
            .ForMember(d => d.TipoProducto, o => o.MapFrom(s => s.Tipo))
            .ForMember(d => d.CantidadVendible, o => o.Ignore())
            .ForMember(d => d.Estado, o => o.Ignore());
    }
}
