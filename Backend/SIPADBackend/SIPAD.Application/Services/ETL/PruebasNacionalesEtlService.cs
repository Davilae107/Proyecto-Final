using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SIPAD.Application.DTOs.Csv;
using SIPAD.Application.Helpers;
using SIPAD.Domain.Entities;
using SIPAD.Infrastructure.Data;

namespace SIPAD.Application.Services.ETL;

public class PruebasNacionalesEtlService
{
    private readonly SipadDbContext _context;
    private readonly CsvReaderHelper _csvReader;
    private readonly ILogger<PruebasNacionalesEtlService> _logger;

    public PruebasNacionalesEtlService(
        SipadDbContext context,
        CsvReaderHelper csvReader,
        ILogger<PruebasNacionalesEtlService> logger)
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
            _logger.LogInformation("Iniciando procesamiento de pruebas nacionales...");

            var records = await _csvReader.ReadCsvAsync<PruebasNacionalesCsvDto>(csvPath);

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

                    // Buscar el rendimiento académico correspondiente para actualizar el campo de pruebas nacionales
                    var rendimiento = await _context.RendimientosAcademicos
                        .FirstOrDefaultAsync(r =>
                            r.IdCentro == centro.IdCentro &&
                            r.IdPeriodo == periodo.IdPeriodo &&
                            (r.NivelEducativo == "Secundario" || r.NivelEducativo == "Primario"));

                    if (rendimiento != null)
                    {
                        rendimiento.PromedioCalificaciones = record.PromedioGeneral;
                        rendimiento.UpdatedAt = DateTime.UtcNow;
                        updated++;
                    }
                    else
                    {
                        // Si no existe, crear un nuevo registro
                        var nuevoRendimiento = new RendimientoAcademico
                        {
                            IdCentro = centro.IdCentro,
                            IdPeriodo = periodo.IdPeriodo,
                            NivelEducativo = record.NivelEvaluado.Contains("4to") ? "Secundario" : "Primario",
                            PromedioCalificaciones = record.PromedioGeneral,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };

                        _context.RendimientosAcademicos.Add(nuevoRendimiento);
                        inserted++;
                    }

                    if ((inserted + updated) % 100 == 0)
                    {
                        await _context.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error procesando pruebas del centro {record.CodigoCentro}");
                    errors++;
                }
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Pruebas Nacionales - Insertados: {inserted}, Actualizados: {updated}, Errores: {errors}");

            return (inserted, updated, errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error procesando pruebas nacionales");
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
}