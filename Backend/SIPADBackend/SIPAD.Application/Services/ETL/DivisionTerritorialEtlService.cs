using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SIPAD.Application.DTOs.Csv;
using SIPAD.Application.Helpers;
using SIPAD.Domain.Entities;
using SIPAD.Infrastructure.Data;

namespace SIPAD.Application.Services.ETL;

public class DivisionTerritorialEtlService
{
    private readonly SipadDbContext _context;
    private readonly CsvReaderHelper _csvReader;
    private readonly ILogger<DivisionTerritorialEtlService> _logger;

    public DivisionTerritorialEtlService(
        SipadDbContext context,
        CsvReaderHelper csvReader,
        ILogger<DivisionTerritorialEtlService> logger)
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
            _logger.LogInformation("Iniciando procesamiento de división territorial...");

            var records = await _csvReader.ReadCsvAsync<DivisionTerritorialCsvDto>(csvPath);

            // Separar por tipo
            var provincias = records.Where(r => r.TipoDivision == "Provincia").ToList();
            var municipios = records.Where(r => r.TipoDivision == "Municipio").ToList();
            var regionales = records.Where(r => r.TipoDivision == "Regional").ToList();
            var distritos = records.Where(r => r.TipoDivision == "Distrito").ToList();

            // Procesar en orden de dependencias
            var (pInserted, pUpdated, pErrors) = await ProcessProvinciasAsync(provincias);
            inserted += pInserted; updated += pUpdated; errors += pErrors;

            var (mInserted, mUpdated, mErrors) = await ProcessMunicipiosAsync(municipios);
            inserted += mInserted; updated += mUpdated; errors += mErrors;

            var (rInserted, rUpdated, rErrors) = await ProcessRegionalesAsync(regionales);
            inserted += rInserted; updated += rUpdated; errors += rErrors;

            var (dInserted, dUpdated, dErrors) = await ProcessDistritosAsync(distritos);
            inserted += dInserted; updated += dUpdated; errors += dErrors;

            _logger.LogInformation($"Procesamiento completado. Insertados: {inserted}, Actualizados: {updated}, Errores: {errors}");

            return (inserted, updated, errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error procesando división territorial");
            throw;
        }
    }

    private async Task<(int inserted, int updated, int errors)> ProcessProvinciasAsync(List<DivisionTerritorialCsvDto> records)
    {
        int inserted = 0, updated = 0, errors = 0;

        foreach (var record in records)
        {
            try
            {
                var existing = await _context.Provincias
                    .FirstOrDefaultAsync(p => p.CodigoProvincia == record.Codigo);

                if (existing == null)
                {
                    var provincia = new Provincia
                    {
                        CodigoProvincia = record.Codigo,
                        NombreProvincia = record.Nombre,
                        RegionGeografica = record.RegionGeografica,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.Provincias.Add(provincia);
                    inserted++;
                }
                else
                {
                    existing.NombreProvincia = record.Nombre;
                    existing.RegionGeografica = record.RegionGeografica;
                    existing.UpdatedAt = DateTime.UtcNow;
                    updated++;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error procesando provincia {record.Codigo}");
                errors++;
            }
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation($"Provincias - Insertadas: {inserted}, Actualizadas: {updated}, Errores: {errors}");

        return (inserted, updated, errors);
    }

    private async Task<(int inserted, int updated, int errors)> ProcessMunicipiosAsync(List<DivisionTerritorialCsvDto> records)
    {
        int inserted = 0, updated = 0, errors = 0;

        foreach (var record in records)
        {
            try
            {
                // Obtener ID de provincia
                var provincia = await _context.Provincias
                    .FirstOrDefaultAsync(p => p.CodigoProvincia == record.CodigoPadre);

                if (provincia == null)
                {
                    _logger.LogWarning($"Provincia no encontrada para municipio {record.Codigo}: {record.CodigoPadre}");
                    errors++;
                    continue;
                }

                var existing = await _context.Municipios
                    .FirstOrDefaultAsync(m => m.CodigoMunicipio == record.Codigo);

                if (existing == null)
                {
                    var municipio = new Municipio
                    {
                        CodigoMunicipio = record.Codigo,
                        NombreMunicipio = record.Nombre,
                        IdProvincia = provincia.IdProvincia,
                        PoblacionEstimada = record.PoblacionEstimada,
                        AreaKm2 = record.AreaKm2,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.Municipios.Add(municipio);
                    inserted++;
                }
                else
                {
                    existing.NombreMunicipio = record.Nombre;
                    existing.IdProvincia = provincia.IdProvincia;
                    existing.PoblacionEstimada = record.PoblacionEstimada;
                    existing.AreaKm2 = record.AreaKm2;
                    existing.UpdatedAt = DateTime.UtcNow;
                    updated++;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error procesando municipio {record.Codigo}");
                errors++;
            }
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation($"Municipios - Insertados: {inserted}, Actualizados: {updated}, Errores: {errors}");

        return (inserted, updated, errors);
    }

    private async Task<(int inserted, int updated, int errors)> ProcessRegionalesAsync(List<DivisionTerritorialCsvDto> records)
    {
        int inserted = 0, updated = 0, errors = 0;

        foreach (var record in records)
        {
            try
            {
                var existing = await _context.RegionalesEducativas
                    .FirstOrDefaultAsync(r => r.CodigoRegional == record.Codigo);

                if (existing == null)
                {
                    var regional = new RegionalEducativa
                    {
                        CodigoRegional = record.Codigo,
                        NombreRegional = record.Nombre,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.RegionalesEducativas.Add(regional);
                    inserted++;
                }
                else
                {
                    existing.NombreRegional = record.Nombre;
                    existing.UpdatedAt = DateTime.UtcNow;
                    updated++;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error procesando regional {record.Codigo}");
                errors++;
            }
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation($"Regionales - Insertadas: {inserted}, Actualizadas: {updated}, Errores: {errors}");

        return (inserted, updated, errors);
    }

    private async Task<(int inserted, int updated, int errors)> ProcessDistritosAsync(List<DivisionTerritorialCsvDto> records)
    {
        int inserted = 0, updated = 0, errors = 0;

        foreach (var record in records)
        {
            try
            {
                // Obtener ID de regional
                var regional = await _context.RegionalesEducativas
                    .FirstOrDefaultAsync(r => r.CodigoRegional == record.CodigoPadre);

                if (regional == null)
                {
                    _logger.LogWarning($"Regional no encontrada para distrito {record.Codigo}: {record.CodigoPadre}");
                    errors++;
                    continue;
                }

                var existing = await _context.DistritosEducativos
                    .FirstOrDefaultAsync(d => d.CodigoDistrito == record.Codigo);

                if (existing == null)
                {
                    var distrito = new DistritoEducativo
                    {
                        CodigoDistrito = record.Codigo,
                        NombreDistrito = record.Nombre,
                        IdRegional = regional.IdRegional,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    _context.DistritosEducativos.Add(distrito);
                    inserted++;
                }
                else
                {
                    existing.NombreDistrito = record.Nombre;
                    existing.IdRegional = regional.IdRegional;
                    existing.UpdatedAt = DateTime.UtcNow;
                    updated++;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error procesando distrito {record.Codigo}");
                errors++;
            }
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation($"Distritos - Insertados: {inserted}, Actualizados: {updated}, Errores: {errors}");

        return (inserted, updated, errors);
    }
}