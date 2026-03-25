using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;

namespace SIPAD.Application.Helpers;

public class CsvReaderHelper
{
    private readonly ILogger<CsvReaderHelper> _logger;

    public CsvReaderHelper(ILogger<CsvReaderHelper> logger)
    {
        _logger = logger;
    }

    public async Task<List<T>> ReadCsvAsync<T>(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                _logger.LogError($"Archivo CSV no encontrado: {filePath}");
                throw new FileNotFoundException($"El archivo {filePath} no existe");
            }

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null,
                HeaderValidated = null,
                Encoding = Encoding.UTF8,
                BadDataFound = context =>
                {
                    _logger.LogWarning($"Datos malformados en línea {context.Context.Parser.Row}: {context.Field}");
                }
            };

            using var reader = new StreamReader(filePath, Encoding.UTF8);
            using var csv = new CsvReader(reader, config);

            var records = csv.GetRecords<T>().ToList();

            _logger.LogInformation($"Se leyeron {records.Count} registros del archivo {Path.GetFileName(filePath)}");

            return records;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error leyendo el archivo CSV: {filePath}");
            throw;
        }
    }
}