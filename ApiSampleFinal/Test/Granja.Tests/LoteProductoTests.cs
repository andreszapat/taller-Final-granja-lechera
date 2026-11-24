using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;

namespace Granja.Tests
{
    public class LoteProductoTests
    {
        private static readonly ProductoParametro Queso = new(TipoProducto.Queso, "kg", 10m, 30, 28000m);
        private static readonly ProductoParametro Yogurt = new(TipoProducto.Yogurt, "L", 1m, 15, 6000m);
        private static readonly DateOnly Dia1 = new(2026, 12, 1);

        [Fact]
        public void Crear_lote_calcula_cantidad_con_rendimiento_y_vencimiento_con_vida_util()
        {
            var lote = new LoteProducto(Queso, Dia1, 100m);

            Assert.Equal(10m, lote.CantidadInicial);
            Assert.Equal(10m, lote.CantidadDisponible);
            Assert.Equal(Dia1.AddDays(30), lote.FechaVencimiento);
        }

        [Fact]
        public void Lote_vence_cuando_la_fecha_es_posterior_al_vencimiento()
        {
            var lote = new LoteProducto(Yogurt, Dia1, 50m); // vence el día 16

            Assert.False(lote.EstaVencido(Dia1.AddDays(15)));
            Assert.True(lote.EstaVencido(Dia1.AddDays(16)));
            Assert.Equal(EstadoLote.Vencido, lote.ObtenerEstado(Dia1.AddDays(16)));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(51)]
        public void Descontar_cantidad_invalida_lanza_excepcion(decimal cantidad)
        {
            var lote = new LoteProducto(Yogurt, Dia1, 50m);
            Assert.Throws<InvalidOperationException>(() => lote.Descontar(cantidad));
            Assert.Equal(50m, lote.CantidadDisponible);
        }

        [Fact]
        public void Procesar_vencimiento_pasa_disponible_a_danado_solo_si_esta_vencido()
        {
            var lote = new LoteProducto(Yogurt, Dia1, 50m);
            lote.Descontar(20m);

            Assert.Equal(0m, lote.ProcesarVencimiento(Dia1.AddDays(10)));
            Assert.Equal(30m, lote.ProcesarVencimiento(Dia1.AddDays(16)));
            Assert.Equal(0m, lote.CantidadDisponible);
            Assert.Equal(30m, lote.CantidadDanada);
            Assert.Equal(20m, lote.CantidadVendida);
            Assert.Equal(EstadoLote.Danado, lote.ObtenerEstado(Dia1.AddDays(16)));
        }

        [Fact]
        public void Transformar_cero_litros_no_es_valido()
        {
            Assert.Throws<DomainException>(() => new LoteProducto(Queso, Dia1, 0m));
        }

        [Fact]
        public void Granja_produccion_esperada_es_n_por_l()
        {
            var granja = new Domain.Entities.Granja("Prueba", 10, 15m);
            Assert.Equal(150m, granja.ProduccionDiariaEsperada);
            Assert.Throws<DomainException>(() => granja.Actualizar(0, 15m));
        }
    }
}
