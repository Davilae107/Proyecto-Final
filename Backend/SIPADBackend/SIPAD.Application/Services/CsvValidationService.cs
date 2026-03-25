using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
namespace SIPAD.Application.Services;

public class CsvValidationService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<CsvValidationService> _logger;

    public CsvValidationService(
        IConfiguration configuration,
        ILogger<CsvValidationService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public (bool isValid, string? errorMessage) ValidateFile(string fileName, long fileSizeBytes, string fileExtension)
    {
        // Validar extensión
        var allowedExtensions = _configuration.GetSection("CsvSettings:AllowedExtensions").Get<string[]>();
        if (allowedExtensions == null || !allowedExtensions.Contains(fileExtension.ToLower()))
        {
            return (false, $"Extensión de archivo no permitida. Solo se permiten: {string.Join(", ", allowedExtensions ?? new[] { ".csv" })}");
        }

        // Validar tamaño
        var maxSizeMB = _configuration.GetValue<int>("CsvSettings:MaxFileSizeMB", 50);
        var maxSizeBytes = maxSizeMB * 1024 * 1024;

        if (fileSizeBytes > maxSizeBytes)
        {
            return (false, $"El archivo excede el tamaño máximo permitido de {maxSizeMB}MB");
        }

        // Validar que sea uno de los tipos permitidos
        var allowedFileTypes = _configuration.GetSection("CsvSettings:AllowedFileTypes").Get<string[]>();
        if (allowedFileTypes != null)
        {
            var isRecognizedType = allowedFileTypes.Any(type =>
                fileName.ToLower().Contains(type.ToLower()));

            if (!isRecognizedType)
            {
                return (false, $"Tipo de archivo no reconocido. Debe contener alguno de: {string.Join(", ", allowedFileTypes)}");
            }
        }

        return (true, null);
    }

    public string DetermineFileType(string fileName)
    {
        var lowerFileName = fileName.ToLower();

        if (lowerFileName.Contains("division_territorial"))
            return "division_territorial";

        if (lowerFileName.Contains("centros_educativos"))
            return "centros_educativos";

        if (lowerFileName.Contains("rendimiento_academico"))
            return "rendimiento_academico";

        if (lowerFileName.Contains("matricula_desercion"))
            return "matricula_desercion";

        if (lowerFileName.Contains("recursos_centros"))
            return "recursos_centros";

        if (lowerFileName.Contains("pruebas_nacionales"))
            return "pruebas_nacionales";

        if (lowerFileName.Contains("indicadores_socioeconomicos"))
            return "indicadores_socioeconomicos";

        if (lowerFileName.Contains("servicios_basicos"))
            return "servicios_basicos";

        if (lowerFileName.Contains("embarazo_adolescente"))
            return "embarazo_adolescente";

        if (lowerFileName.Contains("violencia_delincuencia"))
            return "violencia_delincuencia";

        if (lowerFileName.Contains("trabajo_infantil"))
            return "trabajo_infantil";

        if (lowerFileName.Contains("cobertura_educativa"))
            return "cobertura_educativa";

        return "desconocido";
    }
}