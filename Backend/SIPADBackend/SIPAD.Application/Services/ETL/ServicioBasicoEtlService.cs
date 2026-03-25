using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SIPAD.Application.DTOs.Csv;
using SIPAD.Application.Helpers;
using SIPAD.Domain.Entities;
using SIPAD.Infrastructure.Data;

namespace SIPAD.Application.Services.ETL;

public class ServicioBasicoEtlService
{
    private readonly SipadDbContext _context;
    private readonly CsvReaderHelper _csvReader;
    private readonly ILogger<ServicioBasicoEtlService> _logger;

    public ServicioBasicoEtlService(
        SipadDbContext context,
        CsvReaderHelper csvReader,
        ILogger<ServicioBasicoEtlService> logger)
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
            _logger.LogInformation("Iniciando procesamiento de servicios básicos...");

            var records = await _csvReader.ReadCsvAsync<ServicioBasicoCsvDto>(csvPath);

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

                    var existing = await _context.ServiciosBasicos
                        .FirstOrDefaultAsync(s =>
                            s.IdMunicipio == municipio.IdMunicipio &&
                            s.Anio == record.Anio);

                    if (existing == null)
                    {
                        var servicio = new ServicioBasico
                        {
                            IdMunicipio = municipio.IdMunicipio,
                            Anio = record.Anio,
                            CoberturaAguaPotable = record.CoberturaAguaPotable,
                            CoberturaEnergiaElectrica = record.CoberturaEnergiaElectrica,
                            CoberturaSaneamientoBasico = record.CoberturaAlcantarillado,
                            AccesoInternet = record.AccesoInternetHogar,
                            FuenteDatos = "ONE",
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };

                        _context.ServiciosBasicos.Add(servicio);
                        inserted++;
                    }
                    else
                    {
                        existing.CoberturaAguaPotable = record.CoberturaAguaPotable;
                        existing.CoberturaEnergiaElectrica = record.CoberturaEnergiaElectrica;
                        existing.CoberturaSaneamientoBasico = record.CoberturaAlcantarillado;
                        existing.AccesoInternet = record.AccesoInternetHogar;
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
                    _logger.LogError(ex, $"Error procesando servicio del municipio {record.CodigoMunicipio}");
                    errors++;
                }
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Servicios - Insertados: {inserted}, Actualizados: {updated}, Errores: {errors}");

            return (inserted, updated, errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error procesando servicios básicos");
            throw;
        }
    }
}