using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SIPAD.Application.DTOs.Csv;
using SIPAD.Application.Helpers;
using SIPAD.Domain.Entities;
using SIPAD.Infrastructure.Data;

namespace SIPAD.Application.Services.ETL;

public class ViolenciaDelincuenciaEtlService
{
    private readonly SipadDbContext _context;
    private readonly CsvReaderHelper _csvReader;
    private readonly ILogger<ViolenciaDelincuenciaEtlService> _logger;

    public ViolenciaDelincuenciaEtlService(
        SipadDbContext context,
        CsvReaderHelper csvReader,
        ILogger<ViolenciaDelincuenciaEtlService> logger)
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
            _logger.LogInformation("Iniciando procesamiento de violencia y delincuencia...");

            var records = await _csvReader.ReadCsvAsync<ViolenciaDelincuenciaCsvDto>(csvPath);

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

                    // Calcular índice de delincuencia juvenil por cada 1000 habitantes
                    decimal indiceDelincuencia = record.PoblacionMenor18 > 0
                        ? (decimal)record.DelitosCometidosMenores / record.PoblacionMenor18 * 1000
                        : 0;

                    var existing = await _context.FactoresRiesgoSocial
                        .FirstOrDefaultAsync(f =>
                            f.IdMunicipio == municipio.IdMunicipio &&
                            f.Anio == record.Anio);

                    if (existing == null)
                    {
                        var factor = new FactorRiesgoSocial
                        {
                            IdMunicipio = municipio.IdMunicipio,
                            Anio = record.Anio,
                            IndiceDelincuenciaJuvenil = indiceDelincuencia,
                            CasosViolenciaIntrafamiliar = record.CasosViolenciaIntrafamiliar,
                            FuenteDatos = "CONANI/PN",
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };

                        _context.FactoresRiesgoSocial.Add(factor);
                        inserted++;
                    }
                    else
                    {
                        existing.IndiceDelincuenciaJuvenil = indiceDelincuencia;
                        existing.CasosViolenciaIntrafamiliar = record.CasosViolenciaIntrafamiliar;
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
                    _logger.LogError(ex, $"Error procesando violencia del municipio {record.CodigoMunicipio}");
                    errors++;
                }
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Violencia - Insertados: {inserted}, Actualizados: {updated}, Errores: {errors}");

            return (inserted, updated, errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error procesando violencia y delincuencia");
            throw;
        }
    }
}