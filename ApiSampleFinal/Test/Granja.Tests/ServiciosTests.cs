using Domain.Enums;
using Domain.Exceptions;
using Microsoft.EntityFrameworkCore;
using Services.Dtos;
using static Granja.Tests.TestGranja;

namespace Granja.Tests
{
    /// <summary>Pruebas mínimas del punto 9 del taller, sobre los servicios reales y SQLite en memoria.</summary>
    public class ServiciosTests : IDisposable
    {
        private readonly TestGranja _t = new();
        public void Dispose() => _t.Dispose();

        [Fact]
        public async Task Produccion_diaria_es_n_por_l_y_no_se_registra_dos_veces_la_misma_fecha()
        {
            var dia = await _t.ProducirAsync(Dia(1));

            Assert.Equal(10, dia.VacasOrdenadas);
            Assert.Equal(150m, dia.LitrosProducidos); // 10 vacas × 15 L
            await Assert.ThrowsAsync<ProduccionDuplicadaException>(() => _t.ProducirAsync(Dia(1)));
        }

        [Fact]
        public async Task Una_vaca_no_registra_produccion_dos_veces_la_misma_fecha()
        {
            await _t.Produccion.RegistrarVacaAsync(new RegistrarProduccionVacaRequest { VacaId = 1, Fecha = Dia(1), Litros = 12 });

            await Assert.ThrowsAsync<ProduccionDuplicadaException>(() =>
                _t.Produccion.RegistrarVacaAsync(new RegistrarProduccionVacaRequest { VacaId = 1, Fecha = Dia(1), Litros = 10 }));
            await Assert.ThrowsAsync<ProduccionDuplicadaException>(() => _t.ProducirAsync(Dia(1)));
        }

        [Fact]
        public async Task No_se_pueden_transformar_mas_litros_que_los_disponibles_del_dia()
        {
            await _t.ProducirAsync(Dia(1));
            await _t.TransformarAsync(Dia(1), TipoProducto.Queso, 100m);

            var ex = await Assert.ThrowsAsync<LecheInsuficienteException>(() => _t.TransformarAsync(Dia(1), TipoProducto.Yogurt, 60m));
            Assert.Equal(50m, ex.Disponible);
            // Un día sin producción no tiene leche que transformar.
            await Assert.ThrowsAsync<LecheInsuficienteException>(() => _t.TransformarAsync(Dia(2), TipoProducto.Queso, 10m));
        }

        [Fact]
        public async Task Venta_toma_primero_del_lote_mas_antiguo()
        {
            await _t.ProducirAsync(Dia(1));
            await _t.ProducirAsync(Dia(2));
            var q2 = await _t.TransformarAsync(Dia(2), TipoProducto.Queso, 100m); // creado primero, pero más nuevo
            var q1 = await _t.TransformarAsync(Dia(1), TipoProducto.Queso, 100m);

            var venta = await _t.VenderAsync(Dia(3), TipoProducto.Queso, 4m);

            var detalle = Assert.Single(venta.Detalle);
            Assert.Equal(q1.Id, detalle.LoteId);
            Assert.Equal(Dia(1), detalle.FechaProduccion);
            Assert.Equal(10m, (await _t.Inventario.ObtenerLoteAsync(q2.Id, Dia(3))).CantidadDisponible);
        }

        [Fact]
        public async Task Venta_que_excede_un_lote_consume_el_siguiente_y_genera_varios_detalles()
        {
            await _t.ProducirAsync(Dia(1));
            await _t.ProducirAsync(Dia(2));
            var q1 = await _t.TransformarAsync(Dia(1), TipoProducto.Queso, 100m); // 10 kg
            var q2 = await _t.TransformarAsync(Dia(2), TipoProducto.Queso, 150m); // 15 kg

            var venta = await _t.VenderAsync(Dia(3), TipoProducto.Queso, 12m);

            Assert.Equal(2, venta.Detalle.Count);
            Assert.Equal((q1.Id, 10m), (venta.Detalle[0].LoteId, venta.Detalle[0].Cantidad));
            Assert.Equal((q2.Id, 2m), (venta.Detalle[1].LoteId, venta.Detalle[1].Cantidad));
            Assert.Equal(28000m, venta.PrecioUnitario);
            Assert.Equal(336000m, venta.Total);
        }

        [Fact]
        public async Task Un_lote_vencido_nunca_se_vende_aunque_sea_el_mas_antiguo()
        {
            await _t.ProducirAsync(Dia(1));
            await _t.ProducirAsync(Dia(10));
            var viejo = await _t.TransformarAsync(Dia(1), TipoProducto.Yogurt, 50m);  // vence día 16
            var nuevo = await _t.TransformarAsync(Dia(10), TipoProducto.Yogurt, 40m); // vence día 25

            // Día 17: el lote viejo está vencido pero sus vencimientos aún no se han procesado.
            var venta = await _t.VenderAsync(Dia(17), TipoProducto.Yogurt, 5m);

            Assert.All(venta.Detalle, d => Assert.Equal(nuevo.Id, d.LoteId));
            Assert.Equal(50m, (await _t.Inventario.ObtenerLoteAsync(viejo.Id, Dia(17))).CantidadDisponible);
            await Assert.ThrowsAsync<InventarioInsuficienteException>(() => _t.VenderAsync(Dia(17), TipoProducto.Yogurt, 40m));
        }

        [Fact]
        public async Task Venta_con_existencias_insuficientes_no_modifica_el_inventario()
        {
            await _t.ProducirAsync(Dia(1));
            await _t.ProducirAsync(Dia(2));
            await _t.TransformarAsync(Dia(1), TipoProducto.Queso, 100m);
            await _t.TransformarAsync(Dia(2), TipoProducto.Queso, 150m);

            await Assert.ThrowsAsync<InventarioInsuficienteException>(() => _t.VenderAsync(Dia(3), TipoProducto.Queso, 26m));

            using var db = _t.NuevoContexto();
            var disponibles = await db.Lotes.OrderBy(l => l.Id).Select(l => l.CantidadDisponible).ToListAsync();
            Assert.Equal(new[] { 10m, 15m }, disponibles);
            Assert.Equal(0, await db.Ventas.CountAsync());
            Assert.Equal(0, await db.VentaDetalles.CountAsync());
        }

        [Fact]
        public async Task Procesar_vencimientos_pasa_disponible_a_danado_y_el_reporte_lo_refleja()
        {
            await _t.ProducirAsync(Dia(1));
            var y1 = await _t.TransformarAsync(Dia(1), TipoProducto.Yogurt, 50m);
            await _t.VenderAsync(Dia(3), TipoProducto.Yogurt, 20m);

            var resultado = await _t.Inventario.ProcesarVencimientosAsync(Dia(17));

            var lote = Assert.Single(resultado.Lotes);
            Assert.Equal(y1.Id, lote.Id);
            Assert.Equal(30m, lote.CantidadDanada);
            Assert.Equal(EstadoLote.Danado, lote.Estado);

            var danado = await _t.Reportes.ProductoDanadoAsync(null, null);
            Assert.Equal(30m, Assert.Single(danado.Lotes).CantidadDanada);
            Assert.Equal(Dia(16), danado.Lotes[0].FechaVencimiento);
            Assert.Equal(30m, (await _t.Reportes.ProductoTotalAsync()).Single(p => p.TipoProducto == TipoProducto.Yogurt).Danado);
        }

        /// <summary>Escenario de referencia del punto 8 del taller.</summary>
        [Fact]
        public async Task Escenario_de_referencia_produce_los_reportes_esperados()
        {
            // Día 1: 150 L → 100 L queso (Q1 = 10 kg) y 50 L yogurt (Y1 = 50 L, vence día 16)
            await _t.ProducirAsync(Dia(1));
            var q1 = await _t.TransformarAsync(Dia(1), TipoProducto.Queso, 100m);
            var y1 = await _t.TransformarAsync(Dia(1), TipoProducto.Yogurt, 50m);
            Assert.Equal(10m, q1.CantidadInicial);
            Assert.Equal(Dia(16), y1.FechaVencimiento);

            // Día 2: 150 L → queso (Q2 = 15 kg)
            await _t.ProducirAsync(Dia(2));
            var q2 = await _t.TransformarAsync(Dia(2), TipoProducto.Queso, 150m);
            Assert.Equal(15m, q2.CantidadInicial);

            // Día 3: venta 12 kg queso (10 de Q1 + 2 de Q2) y 20 L de yogurt
            var vq = await _t.VenderAsync(Dia(3), TipoProducto.Queso, 12m);
            Assert.Equal(new[] { (q1.Id, 10m), (q2.Id, 2m) }, vq.Detalle.Select(d => (d.LoteId, d.Cantidad)));
            await _t.VenderAsync(Dia(3), TipoProducto.Yogurt, 20m);

            // Día 17: procesar vencimientos (Y1 → 30 L dañados) y venta de 5 L rechazada
            await _t.Inventario.ProcesarVencimientosAsync(Dia(17));
            await Assert.ThrowsAsync<InventarioInsuficienteException>(() => _t.VenderAsync(Dia(17), TipoProducto.Yogurt, 5m));

            var total = await _t.Reportes.ProductoTotalAsync();
            var queso = total.Single(p => p.TipoProducto == TipoProducto.Queso);
            var yogurt = total.Single(p => p.TipoProducto == TipoProducto.Yogurt);
            Assert.Equal((25m, 12m, 13m, 0m), (queso.Producido, queso.Vendido, queso.Disponible, queso.Danado));
            Assert.Equal((50m, 20m, 0m, 30m), (yogurt.Producido, yogurt.Vendido, yogurt.Disponible, yogurt.Danado));

            var leche = await _t.Reportes.LecheDiariaAsync(null, null);
            Assert.Equal(new[] { 150m, 150m }, leche.Dias.Select(d => d.LitrosProducidos));
            Assert.All(leche.Dias, d => Assert.Equal(0m, d.LitrosNoAprovechados));

            var ventas = await _t.Reportes.VentasAsync(Dia(1), Dia(30), null);
            Assert.Equal(2, ventas.Ventas.Count);
            Assert.Equal(12m * 28000m + 20m * 6000m, ventas.TotalGeneral);

            var diario = await _t.Reportes.ProductoDiarioAsync(null, null);
            Assert.Equal(3, diario.Count);
        }

        [Fact]
        public async Task Reporte_de_vacas_identifica_mayor_menor_y_debajo_del_promedio()
        {
            await _t.Produccion.RegistrarDiaAsync(new RegistrarProduccionRequest
            {
                Fecha = Dia(1),
                Ajustes = new() { new() { VacaId = 1, Litros = 20 }, new() { VacaId = 2, Litros = 10 } }
            });
            await _t.ProducirAsync(Dia(2));

            var reporte = await _t.Reportes.VacasAsync(null, null);

            Assert.Equal(10, reporte.Vacas.Count);
            Assert.Equal(15m, reporte.ProduccionPromedio);   // (17.5 + 12.5 + 8 × 15) / 10
            Assert.Equal(1, reporte.MayorProductora!.VacaId);
            Assert.Equal(17.5m, reporte.MayorProductora.PromedioDiario);
            Assert.Equal(2, reporte.MenorProductora!.VacaId);
            Assert.Equal(2, Assert.Single(reporte.DebajoDelPromedio).VacaId);
        }

        [Fact]
        public async Task Actualizar_granja_cambia_parametros_y_ajusta_las_vacas_activas_a_n()
        {
            var granja = await _t.Granja.ActualizarAsync(new ActualizarGranjaRequest
            {
                NumeroVacas = 12,
                LitrosPorVaca = 20m,
                Productos = new() { new() { TipoProducto = TipoProducto.Yogurt, Unidad = "L", LitrosPorUnidad = 1m, VidaUtilDias = 14, Precio = 7000m } }
            });

            Assert.Equal(240m, granja.ProduccionDiariaEsperada);
            Assert.Equal(14, granja.Productos.Single(p => p.TipoProducto == TipoProducto.Yogurt).VidaUtilDias);
            Assert.Equal(240m, (await _t.ProducirAsync(Dia(1))).LitrosProducidos);

            await _t.Granja.ActualizarAsync(new ActualizarGranjaRequest { NumeroVacas = 5, LitrosPorVaca = 15m });
            Assert.Equal(75m, (await _t.ProducirAsync(Dia(2))).LitrosProducidos);
        }
    }
}
