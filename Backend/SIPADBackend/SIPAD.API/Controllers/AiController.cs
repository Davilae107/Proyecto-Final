using Microsoft.AspNetCore.Mvc;
using SIPAD.API.Services;

namespace SIPAD.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AiController : ControllerBase
{
    private readonly AiRecommendationsService _aiService;
    private readonly ILogger<AiController> _logger;

    public AiController(AiRecommendationsService aiService, ILogger<AiController> logger)
    {
        _aiService = aiService;
        _logger = logger;
    }

    [HttpGet("recomendaciones/provincias")]
    public async Task<IActionResult> GetRecomendacionesPorProvincia([FromQuery] int top = 10)
    {
        if (top < 1 || top > 32)
        {
            return BadRequest(new
            {
                success = false,
                message = "El parámetro 'top' debe estar entre 1 y 32."
            });
        }

        try
        {
            var result = await _aiService.GenerateProvinceRecommendationsAsync(top);
            return Ok(new { success = true, data = result });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Configuración de IA inválida");
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generando recomendaciones de IA");
            return StatusCode(500, new
            {
                success = false,
                message = "No se pudieron generar las recomendaciones de IA."
            });
        }
    }
}
