using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SIPAD.Application.DTOs.Csv;
using SIPAD.Application.Helpers;
using SIPAD.Domain.Entities;
using SIPAD.Infrastructure.Data;

namespace SIPAD.Application.Services.ETL;

public class CentroEducativoEtlService
{
    private readonly SipadDbContext _context;
    private readonly CsvReaderHelper _csvReader;
    private readonly ILogger<CentroEducativoEtlService> _logger;

    public CentroEducativoEtlService(
        SipadDbContext context,
        CsvReaderHelper csvReader,
        ILogger<CentroEducativoEtlService> logger)
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
            _logger.LogInformation("Iniciando procesamiento de centros educativos...");

            var records = await _csvReader.ReadCsvAsync<CentroEducativoCsvDto>(csvPath);

            foreach (var record in records)
            {
                try
                {
                    // Obtener IDs de distrito y municipio
                    var distrito = await _context.DistritosEducativos
                        .FirstOrDefaultAsync(d => d.CodigoDistrito == record.CodigoDistrito);

                    if (distrito == null)
                    {
                        _logger.LogWarning($"Distrito no encontrado para centro {record.CodigoCentro}: {record.CodigoDistrito}");
                        errors++;
                        continue;
                    }

                    var municipio = await _context.Municipios
                        .FirstOrDefaultAsync(m => m.CodigoMunicipio == record.CodigoMunicipio);

                    var existing = await _context.CentrosEducativos
                        .FirstOrDefaultAsync(c => c.CodigoCentro == record.CodigoCentro);

                    if (existing == null)
                    {
                        var centro = new CentroEducativo
                        {
                            CodigoCentro = record.CodigoCentro,
                            NombreCentro = record.NombreCentro,
                            IdDistrito = distrito.IdDistrito,
                            IdMunicipio = municipio?.IdMunicipio,
                            Sector = record.Sector,
                            Nivel = record.Nivel,
                            Zona = record.Zona,
                            Direccion = record.Direccion,
                            Latitud = record.Latitud,
                            Longitud = record.Longitud,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };

                        _context.CentrosEducativos.Add(centro);
                        inserted++;
                    }
                    else
                    {
                        existing.NombreCentro = record.NombreCentro;
                        existing.IdDistrito = distrito.IdDistrito;
                        existing.IdMunicipio = municipio?.IdMunicipio;
                        existing.Sector = record.Sector;
                        existing.Nivel = record.Nivel;
                        existing.Zona = record.Zona;
                        existing.Direccion = record.Direccion;
                        existing.Latitud = record.Latitud;
                        existing.Longitud = record.Longitud;
                        existing.UpdatedAt = DateTime.UtcNow;
                        updated++;
                    }

                    // Guardar cada 100 registros para evitar memoria excesiva
                    if ((inserted + updated) % 100 == 0)
                    {
                        await _context.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error procesando centro {record.CodigoCentro}");
                    errors++;
                }
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Centros - Insertados: {inserted}, Actualizados: {updated}, Errores: {errors}");

            return (inserted, updated, errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error procesando centros educativos");
            throw;
        }
    }
}