using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIPAD.Infrastructure.Data;

namespace SIPAD.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EstadisticasController : ControllerBase
{
    private readonly SipadDbContext _context;
    private readonly ILogger<EstadisticasController> _logger;

    public EstadisticasController(SipadDbContext context, ILogger<EstadisticasController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // ═══════════════════════════════════════════════════════════════
    // DESERCIÓN ESCOLAR - Probabilidad de abandono por sector
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Obtiene estadísticas de deserción agrupadas por sector (Público, Privado)
    /// con cálculo de probabilidad de abandono
    /// </summary>
    [HttpGet("desercion/por-sector")]
    public async Task<IActionResult> GetDesercionPorSector()
    {
        try
        {
            var data = await _context.EstadisticasAbandono
                .Include(e => e.Centro)
                .Where(e => e.Centro != null && e.Centro.Sector != null)
                .GroupBy(e => e.Centro!.Sector!)
                .Select(g => new
                {
                    sector = g.Key,
                    totalMatriculados = g.Sum(e => e.TotalMatriculados ?? 0),
                    totalAbandonos = g.Sum(e => e.TotalAbandonos ?? 0),
                    tasaPromedioAbandono = g.Average(e => e.TasaAbandonoInteranual ?? 0),
                    cantidadCentros = g.Select(e => e.IdCentro).Distinct().Count(),
                    registros = g.Count()
                })
                .ToListAsync();

            // Calcular probabilidad
            var result = data.Select(d => new
            {
                d.sector,
                d.totalMatriculados,
                d.totalAbandonos,
                tasaAbandono = d.totalMatriculados > 0
                    ? Math.Round((double)d.totalAbandonos / d.totalMatriculados * 100, 2)
                    : 0,
                probabilidadAbandono = d.totalMatriculados > 0
                    ? Math.Round((double)d.totalAbandonos / d.totalMatriculados, 4)
                    : 0,
                d.cantidadCentros,
                d.registros
            }).OrderByDescending(d => d.tasaAbandono).ToList();

            return Ok(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo deserción por sector");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene deserción agrupada por nivel educativo
    /// </summary>
    [HttpGet("desercion/por-nivel")]
    public async Task<IActionResult> GetDesercionPorNivel()
    {
        try
        {
            var data = await _context.EstadisticasAbandono
                .Where(e => e.NivelEducativo != null)
                .GroupBy(e => e.NivelEducativo!)
                .Select(g => new
                {
                    nivel = g.Key,
                    totalMatriculados = g.Sum(e => e.TotalMatriculados ?? 0),
                    totalAbandonos = g.Sum(e => e.TotalAbandonos ?? 0),
                    tasaPromedioAbandono = g.Average(e => e.TasaAbandonoInteranual ?? 0),
                    registros = g.Count()
                })
                .ToListAsync();

            var result = data.Select(d => new
            {
                d.nivel,
                d.totalMatriculados,
                d.totalAbandonos,
                tasaAbandono = d.totalMatriculados > 0
                    ? Math.Round((double)d.totalAbandonos / d.totalMatriculados * 100, 2)
                    : 0,
                probabilidadAbandono = d.totalMatriculados > 0
                    ? Math.Round((double)d.totalAbandonos / d.totalMatriculados, 4)
                    : 0,
                d.registros
            }).ToList();

            return Ok(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo deserción por nivel");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene la deserción por zona (Urbana, Rural) cruzando con centros educativos
    /// </summary>
    [HttpGet("desercion/por-zona")]
    public async Task<IActionResult> GetDesercionPorZona()
    {
        try
        {
            var data = await _context.EstadisticasAbandono
                .Include(e => e.Centro)
                .Where(e => e.Centro != null && e.Centro.Zona != null)
                .GroupBy(e => e.Centro!.Zona!)
                .Select(g => new
                {
                    zona = g.Key,
                    totalMatriculados = g.Sum(e => e.TotalMatriculados ?? 0),
                    totalAbandonos = g.Sum(e => e.TotalAbandonos ?? 0),
                    tasaPromedioAbandono = g.Average(e => e.TasaAbandonoInteranual ?? 0),
                    cantidadCentros = g.Select(e => e.IdCentro).Distinct().Count()
                })
                .ToListAsync();

            var result = data.Select(d => new
            {
                d.zona,
                d.totalMatriculados,
                d.totalAbandonos,
                tasaAbandono = d.totalMatriculados > 0
                    ? Math.Round((double)d.totalAbandonos / d.totalMatriculados * 100, 2)
                    : 0,
                probabilidadAbandono = d.totalMatriculados > 0
                    ? Math.Round((double)d.totalAbandonos / d.totalMatriculados, 4)
                    : 0,
                d.cantidadCentros
            }).ToList();

            return Ok(new { success = true, data = result });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo deserción por zona");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene las causantes principales de abandono escolar
    /// </summary>
    [HttpGet("desercion/causantes")]
    public async Task<IActionResult> GetCausantesAbandono()
    {
        try
        {
            var data = await _context.AbandonoCausantesZona
                .Include(a => a.Causante)
                .GroupBy(a => a.Causante.NombreCausante)
                .Select(g => new
                {
                    causante = g.Key,
                    totalCasos = g.Sum(a => a.CasosReportados ?? 0),
                    pesoPromedio = g.Average(a => (double)(a.PesoCausal ?? 0)),
                    municipiosAfectados = g.Select(a => a.IdMunicipio).Distinct().Count()
                })
                .OrderByDescending(d => d.totalCasos)
                .ToListAsync();

            return Ok(new { success = true, data });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo causantes de abandono");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene deserción detallada por centro educativo
    /// </summary>
    [HttpGet("desercion/por-centro")]
    public async Task<IActionResult> GetDesercionPorCentro()
    {
        try
        {
            var data = await _context.EstadisticasAbandono
                .Include(e => e.Centro)
                .Include(e => e.Periodo)
                .Where(e => e.Centro != null)
                .Select(e => new
                {
                    centro = e.Centro!.NombreCentro,
                    codigoCentro = e.Centro.CodigoCentro,
                    sector = e.Centro.Sector,
                    zona = e.Centro.Zona,
                    nivel = e.NivelEducativo,
                    matriculados = e.TotalMatriculados ?? 0,
                    abandonos = e.TotalAbandonos ?? 0,
                    tasaAbandono = e.TasaAbandonoInteranual ?? 0,
                    periodo = e.Periodo != null ? e.Periodo.AnioEscolar : "N/A"
                })
                .OrderByDescending(e => e.tasaAbandono)
                .ToListAsync();

            return Ok(new { success = true, data });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo deserción por centro");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene un resumen general de deserción
    /// </summary>
    [HttpGet("desercion/resumen")]
    public async Task<IActionResult> GetResumenDesercion()
    {
        try
        {
            var totalMatriculados = await _context.EstadisticasAbandono
                .SumAsync(e => e.TotalMatriculados ?? 0);
            var totalAbandonos = await _context.EstadisticasAbandono
                .SumAsync(e => e.TotalAbandonos ?? 0);
            var tasaGeneral = totalMatriculados > 0
                ? Math.Round((double)totalAbandonos / totalMatriculados * 100, 2)
                : 0;
            var totalCentros = await _context.EstadisticasAbandono
                .Select(e => e.IdCentro).Distinct().CountAsync();
            var totalRegistros = await _context.EstadisticasAbandono.CountAsync();

            return Ok(new
            {
                success = true,
                data = new
                {
                    totalMatriculados,
                    totalAbandonos,
                    tasaGeneralAbandono = tasaGeneral,
                    probabilidadGeneral = totalMatriculados > 0
                        ? Math.Round((double)totalAbandonos / totalMatriculados, 4)
                        : 0,
                    totalCentros,
                    totalRegistros
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo resumen de deserción");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // RENDIMIENTO ACADÉMICO
    // ═══════════════════════════════════════════════════════════════

    [HttpGet("rendimiento/por-nivel")]
    public async Task<IActionResult> GetRendimientoPorNivel()
    {
        try
        {
            var data = await _context.RendimientosAcademicos
                .Where(r => r.NivelEducativo != null)
                .GroupBy(r => r.NivelEducativo!)
                .Select(g => new
                {
                    nivel = g.Key,
                    promedioCalificaciones = Math.Round(g.Average(r => (double)(r.PromedioCalificaciones ?? 0)), 2),
                    tasaAprobacion = Math.Round(g.Average(r => (double)(r.TasaAprobacion ?? 0)), 2),
                    tasaRepitencia = Math.Round(g.Average(r => (double)(r.TasaRepitencia ?? 0)), 2),
                    totalEstudiantes = g.Sum(r => r.TotalEstudiantes ?? 0),
                    estudiantesAprobados = g.Sum(r => r.EstudiantesAprobados ?? 0),
                    estudiantesReprobados = g.Sum(r => r.EstudiantesReprobados ?? 0)
                })
                .ToListAsync();

            return Ok(new { success = true, data });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo rendimiento por nivel");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    [HttpGet("rendimiento/por-centro")]
    public async Task<IActionResult> GetRendimientoPorCentro()
    {
        try
        {
            var data = await _context.RendimientosAcademicos
                .Include(r => r.Centro)
                .Where(r => r.Centro != null)
                .Select(r => new
                {
                    centro = r.Centro!.NombreCentro,
                    sector = r.Centro.Sector,
                    zona = r.Centro.Zona,
                    nivel = r.NivelEducativo,
                    promedio = r.PromedioCalificaciones ?? 0,
                    tasaAprobacion = r.TasaAprobacion ?? 0,
                    totalEstudiantes = r.TotalEstudiantes ?? 0,
                    aprobados = r.EstudiantesAprobados ?? 0,
                    reprobados = r.EstudiantesReprobados ?? 0
                })
                .OrderByDescending(r => r.promedio)
                .ToListAsync();

            return Ok(new { success = true, data });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo rendimiento por centro");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // INDICADORES SOCIOECONÓMICOS
    // ═══════════════════════════════════════════════════════════════

    [HttpGet("socioeconomicos/por-municipio")]
    public async Task<IActionResult> GetSocioeconomicosPorMunicipio()
    {
        try
        {
            var data = await _context.IndicadoresSocioeconomicos
                .Include(i => i.Municipio)
                .Where(i => i.Municipio != null)
                .Select(i => new
                {
                    municipio = i.Municipio!.NombreMunicipio,
                    ingresoPromedio = i.IngresoPromedioMensual ?? 0,
                    tasaPobreza = i.TasaPobrezaMonetaria ?? 0,
                    tasaPobrezaExtrema = i.TasaPobrezaExtrema ?? 0,
                    trabajoInfantil = i.IndiceTrabajoInfantil ?? 0,
                    tasaDesempleo = i.TasaDesempleo ?? 0,
                    informalidad = i.NivelInformalidadLaboral ?? 0
                })
                .ToListAsync();

            return Ok(new { success = true, data });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo indicadores socioeconómicos");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // SERVICIOS BÁSICOS
    // ═══════════════════════════════════════════════════════════════

    [HttpGet("servicios/por-municipio")]
    public async Task<IActionResult> GetServiciosPorMunicipio()
    {
        try
        {
            var data = await _context.ServiciosBasicos
                .Include(s => s.Municipio)
                .Where(s => s.Municipio != null)
                .Select(s => new
                {
                    municipio = s.Municipio!.NombreMunicipio,
                    aguaPotable = s.CoberturaAguaPotable ?? 0,
                    energiaElectrica = s.CoberturaEnergiaElectrica ?? 0,
                    saneamiento = s.CoberturaSaneamientoBasico ?? 0,
                    internet = s.AccesoInternet ?? 0
                })
                .ToListAsync();

            return Ok(new { success = true, data });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo servicios básicos");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }

    // ═══════════════════════════════════════════════════════════════
    // COBERTURA EDUCATIVA
    // ═══════════════════════════════════════════════════════════════

    [HttpGet("cobertura/por-municipio")]
    public async Task<IActionResult> GetCoberturaPorMunicipio()
    {
        try
        {
            var data = await _context.DemocratizacionesEducativas
                .Include(d => d.Municipio)
                .Where(d => d.Municipio != null)
                .Select(d => new
                {
                    municipio = d.Municipio!.NombreMunicipio,
                    nivel = d.NivelEducativo,
                    tasaCobertura = d.TasaCoberturaNeta ?? 0,
                    brechaUrbanoRural = d.BrechaUrbanoRural ?? 0,
                    inequidad = d.IndiceInequidadEducativa ?? 0,
                    centrosPorMil = d.CentrosPorMilEstudiantes ?? 0,
                    distanciaPromedio = d.DistanciaPromedioCentroKm ?? 0
                })
                .ToListAsync();

            return Ok(new { success = true, data });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo cobertura educativa");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }
}
