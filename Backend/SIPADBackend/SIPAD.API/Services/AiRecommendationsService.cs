using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SIPAD.API.DTOs;
using SIPAD.API.Options;
using SIPAD.Infrastructure.Data;

namespace SIPAD.API.Services;

public class AiRecommendationsService
{
    private readonly SipadDbContext _context;
    private readonly HttpClient _httpClient;
    private readonly GeminiAiOptions _options;
    private readonly ILogger<AiRecommendationsService> _logger;

    public AiRecommendationsService(
        SipadDbContext context,
        HttpClient httpClient,
        IOptions<GeminiAiOptions> options,
        ILogger<AiRecommendationsService> logger)
    {
        _context = context;
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<AiProvinceRecommendationsResultDto> GenerateProvinceRecommendationsAsync(int top)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            throw new InvalidOperationException("La API key de Gemini no está configurada. Usa GeminiAi:ApiKey en appsettings o variables de entorno.");
        }

        var model = string.IsNullOrWhiteSpace(_options.Model) ? "gemini-1.5-flash" : _options.Model;
        var metrics = await BuildProvinceMetricsAsync();

        if (metrics.Count == 0)
        {
            return new AiProvinceRecommendationsResultDto
            {
                GeneratedAt = DateTime.UtcNow,
                Model = model,
                ProvinciasAnalizadas = 0,
                Recomendaciones = new List<AiProvinceRecommendationDto>()
            };
        }

        var selected = metrics
            .OrderByDescending(m => m.RiskScore)
            .Take(top)
            .ToList();

        var aiResult = await RequestGeminiSuggestionsAsync(selected, model);

        return new AiProvinceRecommendationsResultDto
        {
            GeneratedAt = DateTime.UtcNow,
            Model = model,
            ProvinciasAnalizadas = selected.Count,
            Recomendaciones = aiResult
        };
    }

    private async Task<List<ProvinceMetrics>> BuildProvinceMetricsAsync()
    {
        var provincias = await _context.Provincias
            .Select(p => new
            {
                p.IdProvincia,
                p.NombreProvincia,
                Region = p.RegionGeografica ?? "N/D"
            })
            .ToListAsync();

        var metricsByProvince = provincias.ToDictionary(
            p => p.IdProvincia,
            p => new ProvinceMetrics
            {
                IdProvincia = p.IdProvincia,
                Provincia = p.NombreProvincia,
                Region = p.Region
            });

        var desercion = await (
            from ea in _context.EstadisticasAbandono
            join c in _context.CentrosEducativos on ea.IdCentro equals c.IdCentro
            where c.IdMunicipio != null
            join m in _context.Municipios on c.IdMunicipio equals (int?)m.IdMunicipio
            group ea by m.IdProvincia
            into g
            select new
            {
                IdProvincia = g.Key,
                TasaAbandono = g.Average(x => (double)(x.TasaAbandonoInteranual ?? 0))
            }
        ).ToListAsync();

        var rendimiento = await (
            from r in _context.RendimientosAcademicos
            join c in _context.CentrosEducativos on r.IdCentro equals c.IdCentro
            where c.IdMunicipio != null
            join m in _context.Municipios on c.IdMunicipio equals (int?)m.IdMunicipio
            group r by m.IdProvincia
            into g
            select new
            {
                IdProvincia = g.Key,
                Promedio = g.Average(x => (double)(x.PromedioCalificaciones ?? 0))
            }
        ).ToListAsync();

        var socioeconomicos = await (
            from i in _context.IndicadoresSocioeconomicos
            join m in _context.Municipios on i.IdMunicipio equals m.IdMunicipio
            group i by m.IdProvincia
            into g
            select new
            {
                IdProvincia = g.Key,
                Pobreza = g.Average(x => (double)(x.TasaPobrezaMonetaria ?? 0)),
                Informalidad = g.Average(x => (double)(x.NivelInformalidadLaboral ?? 0))
            }
        ).ToListAsync();

        var servicios = await (
            from s in _context.ServiciosBasicos
            join m in _context.Municipios on s.IdMunicipio equals m.IdMunicipio
            group s by m.IdProvincia
            into g
            select new
            {
                IdProvincia = g.Key,
                Internet = g.Average(x => (double)(x.AccesoInternet ?? 0))
            }
        ).ToListAsync();

        var cobertura = await (
            from c in _context.DemocratizacionesEducativas
            join m in _context.Municipios on c.IdMunicipio equals m.IdMunicipio
            group c by m.IdProvincia
            into g
            select new
            {
                IdProvincia = g.Key,
                Cobertura = g.Average(x => (double)(x.TasaCoberturaNeta ?? 0))
            }
        ).ToListAsync();

        foreach (var item in desercion)
        {
            if (metricsByProvince.TryGetValue(item.IdProvincia, out var metric))
            {
                metric.TasaAbandono = Math.Round(item.TasaAbandono, 2);
            }
        }

        foreach (var item in rendimiento)
        {
            if (metricsByProvince.TryGetValue(item.IdProvincia, out var metric))
            {
                metric.PromedioCalificaciones = Math.Round(item.Promedio, 2);
            }
        }

        foreach (var item in socioeconomicos)
        {
            if (metricsByProvince.TryGetValue(item.IdProvincia, out var metric))
            {
                metric.Pobreza = Math.Round(item.Pobreza, 2);
                metric.Informalidad = Math.Round(item.Informalidad, 2);
            }
        }

        foreach (var item in servicios)
        {
            if (metricsByProvince.TryGetValue(item.IdProvincia, out var metric))
            {
                metric.AccesoInternet = Math.Round(item.Internet, 2);
            }
        }

        foreach (var item in cobertura)
        {
            if (metricsByProvince.TryGetValue(item.IdProvincia, out var metric))
            {
                metric.CoberturaEducativa = Math.Round(item.Cobertura, 2);
            }
        }

        foreach (var metric in metricsByProvince.Values)
        {
            metric.RiskScore = Math.Round(CalculateRiskScore(metric), 2);
        }

        return metricsByProvince.Values.ToList();
    }

    private static double CalculateRiskScore(ProvinceMetrics metric)
    {
        var abandono = Clamp(metric.TasaAbandono, 0, 100);
        var pobreza = Clamp(metric.Pobreza, 0, 100);
        var informalidad = Clamp(metric.Informalidad, 0, 100);
        var rendimientoInverso = 100 - Clamp(metric.PromedioCalificaciones, 0, 100);
        var coberturaInversa = 100 - Clamp(metric.CoberturaEducativa, 0, 100);
        var internetInverso = 100 - Clamp(metric.AccesoInternet, 0, 100);

        return
            (abandono * 0.30) +
            (pobreza * 0.25) +
            (informalidad * 0.10) +
            (rendimientoInverso * 0.20) +
            (coberturaInversa * 0.10) +
            (internetInverso * 0.05);
    }

    private async Task<List<AiProvinceRecommendationDto>> RequestGeminiSuggestionsAsync(
        List<ProvinceMetrics> selected,
        string model)
    {
        var prompt = BuildPrompt(selected);
        var endpoint =
            $"https://generativelanguage.googleapis.com/v1beta/models/{Uri.EscapeDataString(model)}:generateContent?key={Uri.EscapeDataString(_options.ApiKey)}";

        var payload = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = prompt }
                    }
                }
            },
            generationConfig = new
            {
                temperature = 0.35,
                topP = 0.9,
                maxOutputTokens = 1800
            }
        };

        var requestContent = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json");

        try
        {
            using var response = await _httpClient.PostAsync(endpoint, requestContent);
            var responseText = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Gemini API retornó {StatusCode}. Body: {Response}", response.StatusCode, responseText);
                return BuildFallback(selected);
            }

            using var jsonDoc = JsonDocument.Parse(responseText);
            var generatedText = jsonDoc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            if (string.IsNullOrWhiteSpace(generatedText))
            {
                return BuildFallback(selected);
            }

            var cleanJson = ExtractJson(generatedText);
            var parsed = JsonSerializer.Deserialize<AiRecommendationsWrapper>(
                cleanJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (parsed?.Recomendaciones == null || parsed.Recomendaciones.Count == 0)
            {
                return BuildFallback(selected);
            }

            return parsed.Recomendaciones;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error consumiendo Gemini API. Se usará fallback local.");
            return BuildFallback(selected);
        }
    }

    private static string BuildPrompt(List<ProvinceMetrics> selected)
    {
        var statsJson = JsonSerializer.Serialize(selected, new JsonSerializerOptions
        {
            WriteIndented = false
        });

                var sb = new StringBuilder();
                sb.AppendLine("Eres un analista de políticas educativas en República Dominicana.");
                sb.AppendLine("Recibirás estadísticas agregadas por provincia.");
                sb.AppendLine();
                sb.AppendLine("Devuelve SOLO JSON válido con este esquema exacto:");
                sb.AppendLine("{");
                sb.AppendLine("  \"recomendaciones\": [");
                sb.AppendLine("    {");
                sb.AppendLine("      \"provincia\": \"string\",");
                sb.AppendLine("      \"region\": \"string\",");
                sb.AppendLine("      \"prioridad\": \"Alta|Media|Baja\",");
                sb.AppendLine("      \"hallazgos\": [\"string\", \"string\", \"string\"],");
                sb.AppendLine("      \"sugerencias\": [\"string\", \"string\", \"string\"]");
                sb.AppendLine("    }");
                sb.AppendLine("  ]");
                sb.AppendLine("}");
                sb.AppendLine();
                sb.AppendLine("Reglas:");
                sb.AppendLine("- Basar cada recomendación en los datos numéricos entregados.");
                sb.AppendLine("- Máximo 3 hallazgos y máximo 3 sugerencias por provincia.");
                sb.AppendLine("- Sugerencias concretas, accionables y medibles.");
                sb.AppendLine("- No incluyas markdown ni texto adicional fuera del JSON.");
                sb.AppendLine();
                sb.AppendLine("Datos de entrada:");
                sb.Append(statsJson);

                return sb.ToString();
    }

    private static string ExtractJson(string raw)
    {
        var trimmed = raw.Trim();

        if (trimmed.StartsWith("```") && trimmed.Contains('{'))
        {
            var start = trimmed.IndexOf('{');
            var end = trimmed.LastIndexOf('}');
            if (start >= 0 && end > start)
            {
                return trimmed[start..(end + 1)];
            }
        }

        return trimmed;
    }

    private static List<AiProvinceRecommendationDto> BuildFallback(List<ProvinceMetrics> selected)
    {
        return selected.Select(item =>
        {
            var prioridad = item.RiskScore >= 65
                ? "Alta"
                : item.RiskScore >= 45
                    ? "Media"
                    : "Baja";

            var hallazgos = new List<string>
            {
                $"Riesgo educativo compuesto: {item.RiskScore}%.",
                $"Tasa de abandono estimada: {item.TasaAbandono}%.",
                $"Promedio académico estimado: {item.PromedioCalificaciones}%."
            };

            var sugerencias = new List<string>
            {
                "Implementar tutorías focalizadas para estudiantes en sobreedad y riesgo de abandono.",
                "Priorizar inversiones en conectividad y acceso a recursos digitales en centros de baja cobertura.",
                "Coordinar con programas sociales para reducir barreras económicas de permanencia escolar."
            };

            return new AiProvinceRecommendationDto
            {
                Provincia = item.Provincia,
                Region = item.Region,
                Prioridad = prioridad,
                Hallazgos = hallazgos,
                Sugerencias = sugerencias
            };
        }).ToList();
    }

    private static double Clamp(double value, double min, double max)
    {
        if (value < min) return min;
        if (value > max) return max;
        return value;
    }

    private sealed class ProvinceMetrics
    {
        public int IdProvincia { get; set; }
        public string Provincia { get; set; } = string.Empty;
        public string Region { get; set; } = "N/D";
        public double TasaAbandono { get; set; }
        public double PromedioCalificaciones { get; set; }
        public double Pobreza { get; set; }
        public double Informalidad { get; set; }
        public double CoberturaEducativa { get; set; }
        public double AccesoInternet { get; set; }
        public double RiskScore { get; set; }
    }

    private sealed class AiRecommendationsWrapper
    {
        public List<AiProvinceRecommendationDto> Recomendaciones { get; set; } = new();
    }
}
