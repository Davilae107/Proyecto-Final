using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SIPAD.Application.DTOs.Csv;
using SIPAD.Application.Helpers;
using SIPAD.Domain.Entities;
using SIPAD.Infrastructure.Data;

namespace SIPAD.Application.Services.ETL;

public class EmbarazoAdolescenteEtlService
{
    private readonly SipadDbContext _context;
    private readonly CsvReaderHelper _csvReader;
    private readonly ILogger<EmbarazoAdolescenteEtlService> _logger;

    public EmbarazoAdolescenteEtlService(
        SipadDbContext context,
        CsvReaderHelper csvReader,
        ILogger<EmbarazoAdolescenteEtlService> logger)
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
            _logger.LogInformation("Iniciando procesamiento de embarazo adolescente...");

            var records = await _csvReader.ReadCsvAsync<EmbarazoAdolescenteCsvDto>(csvPath);

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

                    // Calcular tasa por cada 1000 habitantes
                    int totalEmbarazos = record.Embarazos1014Anios + record.Embarazos1519Anios;
                    int poblacionTotal = record.PoblacionFemenina1014 + record.PoblacionFemenina1519;

                    decimal tasaEmbarazo = poblacionTotal > 0
                        ? (decimal)totalEmbarazos / poblacionTotal * 1000
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
                            TasaEmbarazoAdolescente = tasaEmbarazo,
                            FuenteDatos = "MISPAS",
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };

                        _context.FactoresRiesgoSocial.Add(factor);
                        inserted++;
                    }
                    else
                    {
                        existing.TasaEmbarazoAdolescente = tasaEmbarazo;
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
                    _logger.LogError(ex, $"Error procesando embarazo del municipio {record.CodigoMunicipio}");
                    errors++;
                }
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Embarazo - Insertados: {inserted}, Actualizados: {updated}, Errores: {errors}");

            return (inserted, updated, errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error procesando embarazo adolescente");
            throw;
        }
    }
}