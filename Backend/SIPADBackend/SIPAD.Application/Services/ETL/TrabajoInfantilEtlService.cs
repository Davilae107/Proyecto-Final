using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SIPAD.Application.DTOs.Csv;
using SIPAD.Application.Helpers;
using SIPAD.Infrastructure.Data;

namespace SIPAD.Application.Services.ETL;

public class TrabajoInfantilEtlService
{
    private readonly SipadDbContext _context;
    private readonly CsvReaderHelper _csvReader;
    private readonly ILogger<TrabajoInfantilEtlService> _logger;

    public TrabajoInfantilEtlService(
        SipadDbContext context,
        CsvReaderHelper csvReader,
        ILogger<TrabajoInfantilEtlService> logger)
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
            _logger.LogInformation("Iniciando procesamiento de trabajo infantil...");

            var records = await _csvReader.ReadCsvAsync<TrabajoInfantilCsvDto>(csvPath);

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

                    // Calcular índice de trabajo infantil
                    int totalTrabajando = record.MenoresTrabajando514 + record.MenoresTrabajando1517;
                    int poblacionTotal = record.Poblacion514 + record.Poblacion1517;

                    decimal indiceTrabajoInfantil = poblacionTotal > 0
                        ? (decimal)totalTrabajando / poblacionTotal * 100
                        : 0;

                    var existing = await _context.IndicadoresSocioeconomicos
                        .FirstOrDefaultAsync(i =>
                            i.IdMunicipio == municipio.IdMunicipio &&
                            i.Anio == record.Anio);

                    if (existing == null)
                    {
                        // Si no existe, crear uno nuevo con solo este campo
                        var indicador = new Domain.Entities.IndicadorSocioeconomico
                        {
                            IdMunicipio = municipio.IdMunicipio,
                            Anio = record.Anio,
                            IndiceTrabajoInfantil = indiceTrabajoInfantil,
                            FuenteDatos = "ONE/CONANI",
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };

                        _context.IndicadoresSocioeconomicos.Add(indicador);
                        inserted++;
                    }
                    else
                    {
                        existing.IndiceTrabajoInfantil = indiceTrabajoInfantil;
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
                    _logger.LogError(ex, $"Error procesando trabajo infantil del municipio {record.CodigoMunicipio}");
                    errors++;
                }
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Trabajo Infantil - Insertados: {inserted}, Actualizados: {updated}, Errores: {errors}");

            return (inserted, updated, errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error procesando trabajo infantil");
            throw;
        }
    }
}