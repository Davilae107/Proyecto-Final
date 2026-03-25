using Microsoft.AspNetCore.Mvc;
using SIPAD.Application.Services.ETL;

namespace SIPAD.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EtlController : ControllerBase
{
    private readonly EtlPipelineService _pipelineService;
    private readonly ILogger<EtlController> _logger;

    public EtlController(
        EtlPipelineService pipelineService,
        ILogger<EtlController> logger)
    {
        _pipelineService = pipelineService;
        _logger = logger;
    }

    /// <summary>
    /// Ejecuta el pipeline ETL completo procesando todos los CSV
    /// </summary>
    [HttpPost("execute-full-pipeline")]
    public async Task<IActionResult> ExecuteFullPipeline()
    {
        try
        {
            _logger.LogInformation("Iniciando pipeline ETL completo desde API...");

            var results = await _pipelineService.ExecuteFullPipelineAsync();

            return Ok(new
            {
                success = true,
                message = "Pipeline ETL ejecutado exitosamente",
                results = results,
                totalInserted = results.Values.Sum(r => r.inserted),
                totalUpdated = results.Values.Sum(r => r.updated),
                totalErrors = results.Values.Sum(r => r.errors)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ejecutando pipeline ETL");
            return StatusCode(500, new
            {
                success = false,
                message = "Error ejecutando pipeline ETL",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// Ejecuta el procesamiento de un solo archivo CSV
    /// </summary>
    [HttpPost("execute-single-file")]
    public async Task<IActionResult> ExecuteSingleFile([FromQuery] string fileName)
    {
        try
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return BadRequest(new { success = false, message = "fileName es requerido" });
            }

            _logger.LogInformation($"Procesando archivo individual: {fileName}");

            var (inserted, updated, errors) = await _pipelineService.ExecuteSingleFileAsync(fileName);

            return Ok(new
            {
                success = true,
                message = $"Archivo {fileName} procesado exitosamente",
                inserted,
                updated,
                errors
            });
        }
        catch (FileNotFoundException ex)
        {
            return NotFound(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error procesando archivo {fileName}");
            return StatusCode(500, new
            {
                success = false,
                message = "Error procesando archivo",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// Verifica el estado de los archivos CSV
    /// </summary>
    [HttpGet("check-csv-files")]
    public IActionResult CheckCsvFiles()
    {
        try
        {
            var basePath = HttpContext.RequestServices
                .GetRequiredService<IConfiguration>()["CsvSettings:CsvBasePath"];

            if (string.IsNullOrEmpty(basePath) || !Directory.Exists(basePath))
            {
                return NotFound(new
                {
                    success = false,
                    message = "Directorio de CSV no encontrado",
                    path = basePath
                });
            }

            var files = Directory.GetFiles(basePath, "*.csv")
                .Select(f => new
                {
                    fileName = Path.GetFileName(f),
                    size = new FileInfo(f).Length,
                    lastModified = new FileInfo(f).LastWriteTime
                })
                .ToList();

            return Ok(new
            {
                success = true,
                basePath,
                totalFiles = files.Count,
                files
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                message = "Error verificando archivos CSV",
                error = ex.Message
            });
        }
    }
}