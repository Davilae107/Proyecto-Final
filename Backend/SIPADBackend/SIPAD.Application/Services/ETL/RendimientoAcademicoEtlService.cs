using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SIPAD.Application.DTOs.Csv;
using SIPAD.Application.Helpers;
using SIPAD.Domain.Entities;
using SIPAD.Infrastructure.Data;

namespace SIPAD.Application.Services.ETL;

public class RendimientoAcademicoEtlService
{
    private readonly SipadDbContext _context;
    private readonly CsvReaderHelper _csvReader;
    private readonly ILogger<RendimientoAcademicoEtlService> _logger;

    public RendimientoAcademicoEtlService(
        SipadDbContext context,
        CsvReaderHelper csvReader,
        ILogger<RendimientoAcademicoEtlService> logger)
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
            _logger.LogInformation("Iniciando procesamiento de rendimiento académico...");

            var records = await _csvReader.ReadCsvAsync<RendimientoAcademicoCsvDto>(csvPath);

            foreach (var record in records)
            {
                try
                {
                    // Obtener ID del centro
                    var centro = await _context.CentrosEducativos
                        .FirstOrDefaultAsync(c => c.CodigoCentro == record.CodigoCentro);

                    if (centro == null)
                    {
                        _logger.LogWarning($"Centro no encontrado: {record.CodigoCentro}");
                        errors++;
                        continue;
                    }

                    // Obtener o crear período
                    var periodo = await GetOrCreatePeriodoAsync(record.AnioEscolar);

                    // Calcular tasas
                    var tasaAprobacion = record.TotalEstudiantes > 0
                        ? (decimal)record.EstudiantesAprobados / record.TotalEstudiantes * 100
                        : 0;

                    var tasaRepitencia = record.TotalEstudiantes > 0
                        ? (decimal)record.EstudiantesReprobados / record.TotalEstudiantes * 100
                        : 0;

                    var tasaSobreedad = record.TotalEstudiantes > 0
                        ? (decimal)record.EstudiantesSobreedad / record.TotalEstudiantes * 100
                        : 0;

                    var existing = await _context.RendimientosAcademicos
                        .FirstOrDefaultAsync(r =>
                            r.IdCentro == centro.IdCentro &&
                            r.IdPeriodo == periodo.IdPeriodo &&
                            r.NivelEducativo == record.NivelEducativo);

                    if (existing == null)
                    {
                        var rendimiento = new RendimientoAcademico
                        {
                            IdCentro = centro.IdCentro,
                            IdPeriodo = periodo.IdPeriodo,
                            NivelEducativo = record.NivelEducativo,
                            TotalEstudiantes = record.TotalEstudiantes,
                            EstudiantesAprobados = record.EstudiantesAprobados,
                            EstudiantesReprobados = record.EstudiantesReprobados,
                            EstudiantesSobreedad = record.EstudiantesSobreedad,
                            PromedioCalificaciones = record.PromedioCalificaciones,
                            TasaAprobacion = tasaAprobacion,
                            TasaRepitencia = tasaRepitencia,
                            TasaSobreedad = tasaSobreedad,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };

                        _context.RendimientosAcademicos.Add(rendimiento);
                        inserted++;
                    }
                    else
                    {
                        existing.TotalEstudiantes = record.TotalEstudiantes;
                        existing.EstudiantesAprobados = record.EstudiantesAprobados;
                        existing.EstudiantesReprobados = record.EstudiantesReprobados;
                        existing.EstudiantesSobreedad = record.EstudiantesSobreedad;
                        existing.PromedioCalificaciones = record.PromedioCalificaciones;
                        existing.TasaAprobacion = tasaAprobacion;
                        existing.TasaRepitencia = tasaRepitencia;
                        existing.TasaSobreedad = tasaSobreedad;
                        existing.UpdatedAt = DateTime.UtcNow;
                        updated++;
                    }

                    if ((inserted + updated) % 100 == 0)
                    {
                        await _context.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error procesando rendimiento del centro {record.CodigoCentro}");
                    errors++;
                }
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Rendimiento - Insertados: {inserted}, Actualizados: {updated}, Errores: {errors}");

            return (inserted, updated, errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error procesando rendimiento académico");
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