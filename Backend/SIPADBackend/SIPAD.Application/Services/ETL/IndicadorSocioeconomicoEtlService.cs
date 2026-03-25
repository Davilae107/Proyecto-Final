using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SIPAD.Application.DTOs.Csv;
using SIPAD.Application.Helpers;
using SIPAD.Domain.Entities;
using SIPAD.Infrastructure.Data;

namespace SIPAD.Application.Services.ETL;

public class IndicadorSocioeconomicoEtlService
{
    private readonly SipadDbContext _context;
    private readonly CsvReaderHelper _csvReader;
    private readonly ILogger<IndicadorSocioeconomicoEtlService> _logger;

    public IndicadorSocioeconomicoEtlService(
        SipadDbContext context,
        CsvReaderHelper csvReader,
        ILogger<IndicadorSocioeconomicoEtlService> logger)
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
            _logger.LogInformation("Iniciando procesamiento de indicadores socioeconómicos...");

            var records = await _csvReader.ReadCsvAsync<IndicadorSocioeconomicoCsvDto>(csvPath);

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

                    var existing = await _context.IndicadoresSocioeconomicos
                        .FirstOrDefaultAsync(i =>
                            i.IdMunicipio == municipio.IdMunicipio &&
                            i.Anio == record.Anio);

                    if (existing == null)
                    {
                        var indicador = new IndicadorSocioeconomico
                        {
                            IdMunicipio = municipio.IdMunicipio,
                            Anio = record.Anio,
                            IngresoPromedioMensual = record.IngresoPromedioMensual,
                            TasaPobrezaMonetaria = record.TasaPobrezaMonetaria,
                            TasaPobrezaExtrema = record.TasaPobrezaExtrema,
                            TasaDesempleo = record.TasaDesempleo,
                            NivelInformalidadLaboral = record.TasaInformalidad,
                            FuenteDatos = "ONE",
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };

                        _context.IndicadoresSocioeconomicos.Add(indicador);
                        inserted++;
                    }
                    else
                    {
                        existing.IngresoPromedioMensual = record.IngresoPromedioMensual;
                        existing.TasaPobrezaMonetaria = record.TasaPobrezaMonetaria;
                        existing.TasaPobrezaExtrema = record.TasaPobrezaExtrema;
                        existing.TasaDesempleo = record.TasaDesempleo;
                        existing.NivelInformalidadLaboral = record.TasaInformalidad;
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
                    _logger.LogError(ex, $"Error procesando indicador del municipio {record.CodigoMunicipio}");
                    errors++;
                }
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Indicadores - Insertados: {inserted}, Actualizados: {updated}, Errores: {errors}");

            return (inserted, updated, errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error procesando indicadores socioeconómicos");
            throw;
        }
    }
}