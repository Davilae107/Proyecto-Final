using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SIPAD.Application.DTOs.Csv;
using SIPAD.Application.Helpers;
using SIPAD.Domain.Entities;
using SIPAD.Infrastructure.Data;

namespace SIPAD.Application.Services.ETL;

public class MatriculaDesercionEtlService
{
    private readonly SipadDbContext _context;
    private readonly CsvReaderHelper _csvReader;
    private readonly ILogger<MatriculaDesercionEtlService> _logger;

    public MatriculaDesercionEtlService(
        SipadDbContext context,
        CsvReaderHelper csvReader,
        ILogger<MatriculaDesercionEtlService> logger)
    {
        _context = context;
        _csvReader = csvReader;
        _logger = logger;
    }

    public async Task<(int inserted, int updated, int errors)> ProcessAsync(string csvPath)
    {
        int inserted = 0, updated = 0, errors = 0;

        try
        {
            _logger.LogInformation("Iniciando procesamiento de matrícula y deserción...");

            var records = await _csvReader.ReadCsvAsync<MatriculaDesercionCsvDto>(csvPath);

            // Diccionario para acumular causantes por municipio
            var causantesPorMunicipio = new Dictionary<int, Dictionary<int, int>>();

            foreach (var record in records)
            {
                try
                {
                    var centro = await _context.CentrosEducativos
                        .FirstOrDefaultAsync(c => c.CodigoCentro == record.CodigoCentro);

                    if (centro == null)
                    {
                        _logger.LogWarning($"Centro no encontrado: {record.CodigoCentro}");
                        errors++;
                        continue;
                    }

                    var periodo = await GetOrCreatePeriodoAsync(record.AnioEscolar);

                    // Calcular tasa de abandono
                    var tasaAbandono = record.MatriculaInicial > 0
                        ? (decimal)record.AbandonosDefinitivos / record.MatriculaInicial * 100
                        : 0;

                    var existing = await _context.EstadisticasAbandono
                        .FirstOrDefaultAsync(e =>
                            e.IdCentro == centro.IdCentro &&
                            e.IdPeriodo == periodo.IdPeriodo &&
                            e.NivelEducativo == record.NivelEducativo);

                    if (existing == null)
                    {
                        var estadistica = new EstadisticaAbandono
                        {
                            IdCentro = centro.IdCentro,
                            IdPeriodo = periodo.IdPeriodo,
                            NivelEducativo = record.NivelEducativo,
                            TotalMatriculados = record.MatriculaInicial,
                            TotalAbandonos = record.AbandonosDefinitivos,
                            TasaAbandonoInteranual = tasaAbandono,
                            TasaDesercionHistorica = tasaAbandono,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };

                        _context.EstadisticasAbandono.Add(estadistica);
                        inserted++;
                    }
                    else
                    {
                        existing.TotalMatriculados = record.MatriculaInicial;
                        existing.TotalAbandonos = record.AbandonosDefinitivos;
                        existing.TasaAbandonoInteranual = tasaAbandono;
                        existing.TasaDesercionHistorica = tasaAbandono;
                        existing.UpdatedAt = DateTime.UtcNow;
                        updated++;
                    }

                    // Acumular causantes por municipio
                    if (centro.IdMunicipio.HasValue && !string.IsNullOrEmpty(record.CausantePrincipal))
                    {
                        var idCausante = await GetOrCreateCausanteAsync(record.CausantePrincipal);

                        if (!causantesPorMunicipio.ContainsKey(centro.IdMunicipio.Value))
                            causantesPorMunicipio[centro.IdMunicipio.Value] = new Dictionary<int, int>();

                        if (!causantesPorMunicipio[centro.IdMunicipio.Value].ContainsKey(idCausante))
                            causantesPorMunicipio[centro.IdMunicipio.Value][idCausante] = 0;

                        causantesPorMunicipio[centro.IdMunicipio.Value][idCausante] += record.AbandonosDefinitivos;
                    }

                    if ((inserted + updated) % 100 == 0)
                    {
                        await _context.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error procesando deserción del centro {record.CodigoCentro}");
                    errors++;
                }
            }

            await _context.SaveChangesAsync();

            // Procesar causantes por zona
            await ProcessCausantesPorZonaAsync(causantesPorMunicipio);

            _logger.LogInformation($"Matrícula/Deserción - Insertados: {inserted}, Actualizados: {updated}, Errores: {errors}");

            return (inserted, updated, errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error procesando matrícula y deserción");
            throw;
        }
    }

    private async Task<PeriodoAcademico> GetOrCreatePeriodoAsync(string anioEscolar)
    {
        var periodo = await _context.PeriodosAcademicos
            .FirstOrDefaultAsync(p => p.AnioEscolar == anioEscolar);

        if (periodo == null)
        {
            periodo = new PeriodoAcademico
            {
                AnioEscolar = anioEscolar,
                Periodo = "Anual",
                Activo = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.PeriodosAcademicos.Add(periodo);
            await _context.SaveChangesAsync();
        }

        return periodo;
    }

    private async Task<int> GetOrCreateCausanteAsync(string nombreCausante)
    {
        var causante = await _context.CausantesAbandono
            .FirstOrDefaultAsync(c => c.NombreCausante == nombreCausante);

        if (causante == null)
        {
            var categoria = nombreCausante switch
            {
                "Embarazo Juvenil" => "Exógeno",
                "Precariedad Económica" => "Exógeno",
                "Delincuencia" => "Exógeno",
                "Inequidad Educativa" => "Exógeno",
                "Bajo Rendimiento Académico" => "Endógeno",
                "Infraestructura Deficiente" => "Endógeno",
                _ => null
            };

            causante = new CausanteAbandono
            {
                NombreCausante = nombreCausante,
                Categoria = categoria,
                CreatedAt = DateTime.UtcNow
            };

            _context.CausantesAbandono.Add(causante);
            await _context.SaveChangesAsync();
        }

        return causante.IdCausante;
    }

    private async Task ProcessCausantesPorZonaAsync(Dictionary<int, Dictionary<int, int>> causantesPorMunicipio)
    {
        foreach (var municipio in causantesPorMunicipio)
        {
            int idMunicipio = municipio.Key;
            int totalAbandonos = municipio.Value.Values.Sum();

            foreach (var causante in municipio.Value)
            {
                int idCausante = causante.Key;
                int casosReportados = causante.Value;
                decimal pesoCausal = totalAbandonos > 0 ? (decimal)casosReportados / totalAbandonos * 100 : 0;

                var existing = await _context.AbandonoCausantesZona
                    .FirstOrDefaultAsync(a =>
                        a.IdMunicipio == idMunicipio &&
                        a.IdCausante == idCausante &&
                        a.Anio == DateTime.UtcNow.Year);

                if (existing == null)
                {
                    var registro = new AbandonoCausanteZona
                    {
                        IdMunicipio = idMunicipio,
                        IdCausante = idCausante,
                        Anio = DateTime.UtcNow.Year,
                        PesoCausal = pesoCausal,
                        CasosReportados = casosReportados,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.AbandonoCausantesZona.Add(registro);
                }
                else
                {
                    existing.PesoCausal = pesoCausal;
                    existing.CasosReportados = casosReportados;
                }
            }
        }

        await _context.SaveChangesAsync();
    }
}