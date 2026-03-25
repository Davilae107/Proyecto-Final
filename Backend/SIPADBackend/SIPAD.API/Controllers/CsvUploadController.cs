using Microsoft.AspNetCore.Mvc;
using SIPAD.API.DTOs;
using SIPAD.Application.Services;
using SIPAD.Application.Services.ETL;

namespace SIPAD.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CsvUploadController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly CsvValidationService _validationService;
    private readonly EtlPipelineService _pipelineService;
    private readonly ILogger<CsvUploadController> _logger;

    public CsvUploadController(
        IConfiguration configuration,
        CsvValidationService validationService,
        EtlPipelineService pipelineService,
        ILogger<CsvUploadController> logger)
    {
        _configuration = configuration;
        _validationService = validationService;
        _pipelineService = pipelineService;
        _logger = logger;
    }

    /// <summary>
    /// Sube un archivo CSV individual
    /// </summary>
    /// <param name="file">Archivo CSV a subir</param>
    /// <param name="autoProcess">Si es true, procesa automáticamente el archivo después de subirlo</param>
    [HttpPost("upload-single")]
    [RequestSizeLimit(52428800)] // 50MB
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<UploadCsvResponse>> UploadSingleCsv([FromForm] CsvUploadRequest request)
    {
        try
        {
            // Se accede al archivo a través de la propiedad del DTO
            if (request.File == null || request.File.Length == 0)
            {
                return BadRequest(new UploadCsvResponse
                {
                    Success = false,
                    Message = "No se recibió ningún archivo"
                });
            }

            _logger.LogInformation($"Recibiendo archivo: {request.File.FileName} ({request.File.Length} bytes)");

            // Validación del archivo empleando las propiedades del objeto request
            var fileExtension = Path.GetExtension(request.File.FileName);
            var (isValid, errorMessage) = _validationService.ValidateFile(
                request.File.FileName,
                request.File.Length,
                fileExtension);

            if (!isValid)
            {
                return BadRequest(new UploadCsvResponse
                {
                    Success = false,
                    Message = errorMessage!
                });
            }

            var basePath = _configuration["CsvSettings:CsvBasePath"];
            if (string.IsNullOrEmpty(basePath))
            {
                return StatusCode(500, new UploadCsvResponse
                {
                    Success = false,
                    Message = "Ruta de almacenamiento no configurada"
                });
            }

            if (!Directory.Exists(basePath))
            {
                Directory.CreateDirectory(basePath);
            }

            var fileName = request.File.FileName;
            var filePath = Path.Combine(basePath, fileName);

            if (System.IO.File.Exists(filePath))
            {
                var fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                fileName = $"{fileNameWithoutExt}_{timestamp}{fileExtension}";
                filePath = Path.Combine(basePath, fileName);
            }

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await request.File.CopyToAsync(stream);
            }

            var response = new UploadCsvResponse
            {
                Success = true,
                Message = "Archivo subido exitosamente",
                FileName = fileName,
                FilePath = filePath,
                FileSizeBytes = request.File.Length,
                AutoProcessed = false
            };

            // Uso de la bandera booleana proveniente del formulario
            if (request.AutoProcess)
            {
                try
                {
                    var (inserted, updated, errors) = await _pipelineService.ExecuteSingleFileAsync(fileName);
                    response.AutoProcessed = true;
                    response.ProcessingResult = new ProcessingResult { Inserted = inserted, Updated = updated, Errors = errors };
                    response.Message = $"Archivo subido y procesado. Insertados: {inserted}, Actualizados: {updated}, Errores: {errors}";
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error procesando archivo automáticamente");
                    response.Message = "Archivo subido pero hubo un error al procesarlo";
                }
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error subiendo archivo CSV");
            return StatusCode(500, new UploadCsvResponse { Success = false, Message = $"Error interno: {ex.Message}" });
        }
    }

    /// <summary>
    /// Sube múltiples archivos CSV de una sola vez
    /// </summary>
    /// <param name="files">Lista de archivos CSV</param>
    /// <param name="autoProcess">Si es true, procesa automáticamente todos los archivos</param>
    [HttpPost("upload-multiple")]
    [RequestSizeLimit(104857600)] // 100MB total
    public async Task<ActionResult<UploadMultipleCsvsResponse>> UploadMultipleCsvs(
        [FromForm] List<IFormFile> files,
        [FromForm] bool autoProcess = false)
    {
        var response = new UploadMultipleCsvsResponse
        {
            TotalFiles = files.Count,
            Results = new List<FileUploadResult>()
        };

        try
        {
            if (files == null || files.Count == 0)
            {
                return BadRequest(new UploadMultipleCsvsResponse
                {
                    Success = false,
                    Message = "No se recibieron archivos"
                });
            }

            _logger.LogInformation($"Recibiendo {files.Count} archivos");

            var basePath = _configuration["CsvSettings:CsvBasePath"];
            if (string.IsNullOrEmpty(basePath))
            {
                return StatusCode(500, new UploadMultipleCsvsResponse
                {
                    Success = false,
                    Message = "Ruta de almacenamiento no configurada"
                });
            }

            if (!Directory.Exists(basePath))
            {
                Directory.CreateDirectory(basePath);
            }

            foreach (var file in files)
            {
                try
                {
                    if (file.Length == 0)
                    {
                        response.Results.Add(new FileUploadResult
                        {
                            FileName = file.FileName,
                            Success = false,
                            Message = "Archivo vacío"
                        });
                        continue;
                    }

                    // Validar
                    var fileExtension = Path.GetExtension(file.FileName);
                    var (isValid, errorMessage) = _validationService.ValidateFile(
                        file.FileName,
                        file.Length,
                        fileExtension);

                    if (!isValid)
                    {
                        response.Results.Add(new FileUploadResult
                        {
                            FileName = file.FileName,
                            Success = false,
                            Message = errorMessage
                        });
                        response.FailedUploads++;
                        continue;
                    }

                    // Guardar
                    var fileName = file.FileName;
                    var filePath = Path.Combine(basePath, fileName);

                    if (System.IO.File.Exists(filePath))
                    {
                        var fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
                        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                        fileName = $"{fileNameWithoutExt}_{timestamp}{fileExtension}";
                        filePath = Path.Combine(basePath, fileName);
                    }

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    response.Results.Add(new FileUploadResult
                    {
                        FileName = fileName,
                        Success = true,
                        Message = "Subido exitosamente",
                        FileSizeBytes = file.Length
                    });
                    response.SuccessfulUploads++;

                    _logger.LogInformation($"Archivo guardado: {filePath}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error procesando archivo {file.FileName}");
                    response.Results.Add(new FileUploadResult
                    {
                        FileName = file.FileName,
                        Success = false,
                        Message = $"Error: {ex.Message}"
                    });
                    response.FailedUploads++;
                }
            }

            response.Success = response.SuccessfulUploads > 0;
            response.Message = $"Subidos: {response.SuccessfulUploads}/{response.TotalFiles}. Fallidos: {response.FailedUploads}";

            // Procesar automáticamente si se solicita
            if (autoProcess && response.SuccessfulUploads > 0)
            {
                try
                {
                    _logger.LogInformation("Ejecutando pipeline ETL completo...");
                    await _pipelineService.ExecuteFullPipelineAsync();
                    response.Message += " - Pipeline ETL ejecutado automáticamente";
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error ejecutando pipeline automáticamente");
                    response.Message += " - Error al ejecutar pipeline ETL";
                }
            }

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en upload múltiple");
            return StatusCode(500, new UploadMultipleCsvsResponse
            {
                Success = false,
                Message = $"Error interno: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Lista todos los archivos CSV disponibles en el servidor
    /// </summary>
    [HttpGet("list-files")]
    public IActionResult ListCsvFiles()
    {
        try
        {
            var basePath = _configuration["CsvSettings:CsvBasePath"];

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
                    filePath = f,
                    sizeBytes = new FileInfo(f).Length,
                    sizeMB = Math.Round(new FileInfo(f).Length / 1024.0 / 1024.0, 2),
                    lastModified = new FileInfo(f).LastWriteTime,
                    fileType = _validationService.DetermineFileType(Path.GetFileName(f))
                })
                .OrderByDescending(f => f.lastModified)
                .ToList();

            return Ok(new
            {
                success = true,
                basePath,
                totalFiles = files.Count,
                totalSizeMB = Math.Round(files.Sum(f => f.sizeMB), 2),
                files
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listando archivos");
            return StatusCode(500, new
            {
                success = false,
                message = "Error listando archivos",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// Elimina un archivo CSV del servidor
    /// </summary>
    [HttpDelete("delete-file")]
    public IActionResult DeleteCsvFile([FromQuery] string fileName)
    {
        try
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return BadRequest(new { success = false, message = "fileName es requerido" });
            }

            var basePath = _configuration["CsvSettings:CsvBasePath"];
            var filePath = Path.Combine(basePath!, fileName);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound(new
                {
                    success = false,
                    message = $"Archivo no encontrado: {fileName}"
                });
            }

            System.IO.File.Delete(filePath);
            _logger.LogInformation($"Archivo eliminado: {filePath}");

            return Ok(new
            {
                success = true,
                message = $"Archivo {fileName} eliminado exitosamente"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error eliminando archivo {fileName}");
            return StatusCode(500, new
            {
                success = false,
                message = "Error eliminando archivo",
                error = ex.Message
            });
        }
    }
}