namespace SIPAD.API.DTOs;

public class AiProvinceRecommendationDto
{
    public string Provincia { get; set; } = string.Empty;
    public string Region { get; set; } = "N/D";
    public string Prioridad { get; set; } = "Media";
    public List<string> Hallazgos { get; set; } = new();
    public List<string> Sugerencias { get; set; } = new();
}

public class AiProvinceRecommendationsResultDto
{
    public DateTime GeneratedAt { get; set; }
    public string Model { get; set; } = string.Empty;
    public int ProvinciasAnalizadas { get; set; }
    public List<AiProvinceRecommendationDto> Recomendaciones { get; set; } = new();
}
