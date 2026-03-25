using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SIPAD.Application.DTOs.Csv;
using SIPAD.Application.Helpers;
using SIPAD.Domain.Entities;
using SIPAD.Infrastructure.Data;

namespace SIPAD.Application.Services.ETL;

public class CoberturaEducativaEtlService
{
    private readonly SipadDbContext _context;
    private readonly CsvReaderHelper _csvReader;
    private readonly ILogger<CoberturaEducativaEtlService> _logger;

    public CoberturaEducativaEtlService(
        SipadDbContext context,
        CsvReaderHelper csvReader,
        ILogger<CoberturaEducativaEtlService> logger)
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
            _logger.LogInformation("Iniciando procesamiento de cobertura educativa...");

            var records = await _csvReader.ReadCsvAsync<CoberturaEducativaCsvDto>(csvPath);

            foreach (var record in records)
            {
                try
                {
                    var municipio = await _context.Municipios
                        .FirstOrDefaultAsync(m => m.CodigoMunicipio == record.CodigoMunicipio);

                    if (municipio == null)
                    {
                        _logger.LogWarning($"Municipio no encontrado: {record.CodigoMunicipio}");
                        errors++;
                        continue;
                    }

                    // Calcular indicadores
                    decimal tasaCoberturaNeta = record.PoblacionEdadEscolar > 0
                        ? (decimal)record.EstudiantesMatriculados / record.PoblacionEdadEscolar * 100
                        : 0;

                    decimal porcentajeUrbano = record.EstudiantesMatriculados > 0
                        ? (decimal)record.ZonaUrbana / record.EstudiantesMatriculados * 100
                        : 0;

                    decimal porcentajeRural = record.EstudiantesMatriculados > 0
                        ? (decimal)record.ZonaRural / record.EstudiantesMatriculados * 100
                        : 0;

                    decimal brechaUrbanoRural = Math.Abs(porcentajeUrbano - porcentajeRural);

                    decimal centrosPorMilEstudiantes = record.EstudiantesMatriculados > 0
                        ? (decimal)(record.TotalCentrosPublicos + record.TotalCentrosPrivados) / record.EstudiantesMatriculados * 1000
                        : 0;

                    var existing = await _context.DemocratizacionesEducativas
                        .FirstOrDefaultAsync(d =>
                            d.IdMunicipio == municipio.IdMunicipio &&
                            d.Anio == record.Anio &&
                            d.NivelEducativo == record.NivelEducativo);

                    if (existing == null)
                    {
                        var democratizacion = new DemocratizacionEducativa
                        {
                            IdMunicipio = municipio.IdMunicipio,
                            Anio = record.Anio,
                            NivelEducativo = record.NivelEducativo,
                            TasaCoberturaNeta = tasaCoberturaNeta,
                            BrechaUrbanoRural = brechaUrbanoRural,
                            CentrosPorMilEstudiantes = centrosPorMilEstudiantes,
                            DistanciaPromedioCentroKm = record.DistanciaPromedioKm,
                            FuenteDatos = "MINERD",
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };

                        _context.DemocratizacionesEducativas.Add(democratizacion);
                        inserted++;
                    }
                    else
                    {
                        existing.TasaCoberturaNeta = tasaCoberturaNeta;
                        existing.BrechaUrbanoRural = brechaUrbanoRural;
                        existing.CentrosPorMilEstudiantes = centrosPorMilEstudiantes;
                        existing.DistanciaPromedioCentroKm = record.DistanciaPromedioKm;
                        existing.UpdatedAt = DateTime.UtcNow;
                        updated++;
                    }

                    if ((inserted + updated) % 50 == 0)
                    {
                        await _context.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error procesando cobertura del municipio {record.CodigoMunicipio}");
                    errors++;
                }
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Cobertura - Insertados: {inserted}, Actualizados: {updated}, Errores: {errors}");

            return (inserted, updated, errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error procesando cobertura educativa");
            throw;
        }
    }
}