using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SIPAD.Application.Services.ETL;

public class EtlPipelineService
{
    private readonly DivisionTerritorialEtlService _divisionService;
    private readonly CentroEducativoEtlService _centroService;
    private readonly RendimientoAcademicoEtlService _rendimientoService;
    private readonly IndicadorSocioeconomicoEtlService _indicadorService;
    private readonly ServicioBasicoEtlService _servicioService;
    private readonly MatriculaDesercionEtlService _matriculaService;
    private readonly RecursosCentrosEtlService _recursosService;
    private readonly EmbarazoAdolescenteEtlService _embarazoService;
    private readonly ViolenciaDelincuenciaEtlService _violenciaService;
    private readonly TrabajoInfantilEtlService _trabajoInfantilService;
    private readonly CoberturaEducativaEtlService _coberturaService;
    private readonly PruebasNacionalesEtlService _pruebasService;

    private readonly IConfiguration _configuration;
    private readonly ILogger<EtlPipelineService> _logger;

    public EtlPipelineService(
        DivisionTerritorialEtlService divisionService,
        CentroEducativoEtlService centroService,
        RendimientoAcademicoEtlService rendimientoService,
        IndicadorSocioeconomicoEtlService indicadorService,
        ServicioBasicoEtlService servicioService,
        MatriculaDesercionEtlService matriculaService,
        RecursosCentrosEtlService recursosService,
        EmbarazoAdolescenteEtlService embarazoService,
        ViolenciaDelincuenciaEtlService violenciaService,
        TrabajoInfantilEtlService trabajoInfantilService,
        CoberturaEducativaEtlService coberturaService,
        PruebasNacionalesEtlService pruebasService,
        IConfiguration configuration,
        ILogger<EtlPipelineService> logger)
    {
        _divisionService = divisionService;
        _centroService = centroService;
        _rendimientoService = rendimientoService;
        _indicadorService = indicadorService;
        _servicioService = servicioService;
        _matriculaService = matriculaService;
        _recursosService = recursosService;
        _embarazoService = embarazoService;
        _violenciaService = violenciaService;
        _trabajoInfantilService = trabajoInfantilService;
        _coberturaService = coberturaService;
        _pruebasService = pruebasService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<Dictionary<string, (int inserted, int updated, int errors)>> ExecuteFullPipelineAsync()
    {
        var results = new Dictionary<string, (int inserted, int updated, int errors)>();
        var basePath = _configuration["CsvSettings:CsvBasePath"];

        if (string.IsNullOrEmpty(basePath))
        {
            throw new InvalidOperationException("CsvBasePath no configurado en appsettings.json");
        }

        _logger.LogInformation("=== INICIANDO PIPELINE ETL COMPLETO ===");
        var startTime = DateTime.UtcNow;

        try
        {
            // FASE 1: Datos Maestros (crítico - debe ejecutarse primero)
            _logger.LogInformation("FASE 1: Procesando datos maestros...");

            var divisionPath = Path.Combine(basePath, "division_territorial.csv");
            results["division_territorial"] = await _divisionService.ProcessAsync(divisionPath);

            var centrosPath = Path.Combine(basePath, "centros_educativos.csv");
            results["centros_educativos"] = await _centroService.ProcessAsync(centrosPath);

            // FASE 2: Datos Educativos
            _logger.LogInformation("FASE 2: Procesando datos educativos...");

            var rendimientoPath = Path.Combine(basePath, "rendimiento_academico_2023-2024.csv");
            if (File.Exists(rendimientoPath))
            {
                results["rendimiento_academico"] = await _rendimientoService.ProcessAsync(rendimientoPath);
            }

            var matriculaPath = Path.Combine(basePath, "matricula_desercion_2023-2024.csv");
            if (File.Exists(matriculaPath))
            {
                results["matricula_desercion"] = await _matriculaService.ProcessAsync(matriculaPath);
            }

            var recursosPath = Path.Combine(basePath, "recursos_centros_2023-2024.csv");
            if (File.Exists(recursosPath))
            {
                results["recursos_centros"] = await _recursosService.ProcessAsync(recursosPath);
            }

            var pruebasPath = Path.Combine(basePath, "pruebas_nacionales_2023-2024.csv");
            if (File.Exists(pruebasPath))
            {
                results["pruebas_nacionales"] = await _pruebasService.ProcessAsync(pruebasPath);
            }

            var coberturaPath = Path.Combine(basePath, "cobertura_educativa_2024.csv");
            if (File.Exists(coberturaPath))
            {
                results["cobertura_educativa"] = await _coberturaService.ProcessAsync(coberturaPath);
            }

            // FASE 3: Datos Socioeconómicos
            _logger.LogInformation("FASE 3: Procesando datos socioeconómicos...");

            var indicadorPath = Path.Combine(basePath, "indicadores_socioeconomicos_2024.csv");
            if (File.Exists(indicadorPath))
            {
                results["indicadores_socioeconomicos"] = await _indicadorService.ProcessAsync(indicadorPath);
            }

            var servicioPath = Path.Combine(basePath, "servicios_basicos_2024.csv");
            if (File.Exists(servicioPath))
            {
                results["servicios_basicos"] = await _servicioService.ProcessAsync(servicioPath);
            }

            var trabajoPath = Path.Combine(basePath, "trabajo_infantil_2024.csv");
            if (File.Exists(trabajoPath))
            {
                results["trabajo_infantil"] = await _trabajoInfantilService.ProcessAsync(trabajoPath);
            }

            // FASE 4: Datos de Riesgo Social
            _logger.LogInformation("FASE 4: Procesando datos de riesgo social...");

            var embarazoPath = Path.Combine(basePath, "embarazo_adolescente_2024.csv");
            if (File.Exists(embarazoPath))
            {
                results["embarazo_adolescente"] = await _embarazoService.ProcessAsync(embarazoPath);
            }

            var violenciaPath = Path.Combine(basePath, "violencia_delincuencia_2024.csv");
            if (File.Exists(violenciaPath))
            {
                results["violencia_delincuencia"] = await _violenciaService.ProcessAsync(violenciaPath);
            }

            var endTime = DateTime.UtcNow;
            var duration = endTime - startTime;

            _logger.LogInformation($"=== PIPELINE ETL COMPLETADO EN {duration.TotalSeconds:F2} SEGUNDOS ===");

            // Resumen
            var totalInserted = results.Values.Sum(r => r.inserted);
            var totalUpdated = results.Values.Sum(r => r.updated);
            var totalErrors = results.Values.Sum(r => r.errors);

            _logger.LogInformation($"RESUMEN TOTAL - Insertados: {totalInserted}, Actualizados: {totalUpdated}, Errores: {totalErrors}");

            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ejecutando pipeline ETL");
            throw;
        }
    }

    public async Task<(int inserted, int updated, int errors)> ExecuteSingleFileAsync(string fileName)
    {
        var basePath = _configuration["CsvSettings:CsvBasePath"];
        var filePath = Path.Combine(basePath!, fileName);

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Archivo no encontrado: {filePath}");
        }

        _logger.LogInformation($"Procesando archivo individual: {fileName}");

        if (fileName.Contains("division_territorial"))
            return await _divisionService.ProcessAsync(filePath);

        if (fileName.Contains("centros_educativos"))
            return await _centroService.ProcessAsync(filePath);

        if (fileName.Contains("rendimiento_academico"))
            return await _rendimientoService.ProcessAsync(filePath);

        if (fileName.Contains("matricula_desercion"))
            return await _matriculaService.ProcessAsync(filePath);

        if (fileName.Contains("recursos_centros"))
            return await _recursosService.ProcessAsync(filePath);

        if (fileName.Contains("pruebas_nacionales"))
            return await _pruebasService.ProcessAsync(filePath);

        if (fileName.Contains("indicadores_socioeconomicos"))
            return await _indicadorService.ProcessAsync(filePath);

        if (fileName.Contains("servicios_basicos"))
            return await _servicioService.ProcessAsync(filePath);

        if (fileName.Contains("embarazo_adolescente"))
            return await _embarazoService.ProcessAsync(filePath);

        if (fileName.Contains("violencia_delincuencia"))
            return await _violenciaService.ProcessAsync(filePath);

        if (fileName.Contains("trabajo_infantil"))
            return await _trabajoInfantilService.ProcessAsync(filePath);

        if (fileName.Contains("cobertura_educativa"))
            return await _coberturaService.ProcessAsync(filePath);

        throw new ArgumentException($"Tipo de archivo no reconocido: {fileName}");
    }
}