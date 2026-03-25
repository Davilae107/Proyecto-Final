using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SIPAD.Application.DTOs.Csv;
using SIPAD.Application.Helpers;
using SIPAD.Domain.Entities;
using SIPAD.Infrastructure.Data;

namespace SIPAD.Application.Services.ETL;

public class RecursosCentrosEtlService
{
    private readonly SipadDbContext _context;
    private readonly CsvReaderHelper _csvReader;
    private readonly ILogger<RecursosCentrosEtlService> _logger;

    public RecursosCentrosEtlService(
        SipadDbContext context,
        CsvReaderHelper csvReader,
        ILogger<RecursosCentrosEtlService> logger)
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
            _logger.LogInformation("Iniciando procesamiento de recursos de centros...");

            var records = await _csvReader.ReadCsvAsync<RecursosCentrosCsvDto>(csvPath);

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

                    // Calcular ratio estudiante-docente
                    var ratioEstudianteDocente = record.TotalDocentes > 0
                        ? await CalcularRatioEstudianteDocenteAsync(centro.IdCentro, periodo.IdPeriodo, record.TotalDocentes)
                        : (decimal?)null;

                    var existing = await _context.RecursosEducativos
                        .FirstOrDefaultAsync(r =>
                            r.IdCentro == centro.IdCentro &&
                            r.IdPeriodo == periodo.IdPeriodo);

                    if (existing == null)
                    {
                        var recurso = new RecursoEducativo
                        {
                            IdCentro = centro.IdCentro,
                            IdPeriodo = periodo.IdPeriodo,
                            TotalDocentes = record.TotalDocentes,
                            RatioEstudianteDocente = ratioEstudianteDocente,
                            TieneBiblioteca = record.TieneBiblioteca?.ToLower() == "sí" || record.TieneBiblioteca?.ToLower() == "si",
                            TieneLaboratorio = record.TieneLaboratorio?.ToLower() == "sí" || record.TieneLaboratorio?.ToLower() == "si",
                            TieneTecnologiaEducativa = record.ComputadorasDisponibles > 0,
                            ProgramaAlimentacionEscolar = record.ProgramaAlimentacion?.ToLower() == "sí" || record.ProgramaAlimentacion?.ToLower() == "si",
                            ProgramaTransporteEscolar = record.ProgramaTransporte?.ToLower() == "sí" || record.ProgramaTransporte?.ToLower() == "si",
                            EstadoInfraestructura = record.EstadoInfraestructura,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };

                        _context.RecursosEducativos.Add(recurso);
                        inserted++;
                    }
                    else
                    {
                        existing.TotalDocentes = record.TotalDocentes;
                        existing.RatioEstudianteDocente = ratioEstudianteDocente;
                        existing.TieneBiblioteca = record.TieneBiblioteca?.ToLower() == "sí" || record.TieneBiblioteca?.ToLower() == "si";
                        existing.TieneLaboratorio = record.TieneLaboratorio?.ToLower() == "sí" || record.TieneLaboratorio?.ToLower() == "si";
                        existing.TieneTecnologiaEducativa = record.ComputadorasDisponibles > 0;
                        existing.ProgramaAlimentacionEscolar = record.ProgramaAlimentacion?.ToLower() == "sí" || record.ProgramaAlimentacion?.ToLower() == "si";
                        existing.ProgramaTransporteEscolar = record.ProgramaTransporte?.ToLower() == "sí" || record.ProgramaTransporte?.ToLower() == "si";
                        existing.EstadoInfraestructura = record.EstadoInfraestructura;
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
                    _logger.LogError(ex, $"Error procesando recursos del centro {record.CodigoCentro}");
                    errors++;
                }
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Recursos - Insertados: {inserted}, Actualizados: {updated}, Errores: {errors}");

            return (inserted, updated, errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error procesando recursos de centros");
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

    private async Task<decimal?> CalcularRatioEstudianteDocenteAsync(int idCentro, int idPeriodo, int totalDocentes)
    {
        var rendimiento = await _context.RendimientosAcademicos
            .Where(r => r.IdCentro == idCentro && r.IdPeriodo == idPeriodo)
            .Select(r => r.TotalEstudiantes)
            .FirstOrDefaultAsync();

        if (rendimiento.HasValue && totalDocentes > 0)
        {
            return (decimal)rendimiento.Value / totalDocentes;
        }

        return null;
    }
}