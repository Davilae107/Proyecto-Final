using CsvHelper.Configuration.Attributes;

namespace SIPAD.Application.DTOs.Csv;

public class DivisionTerritorialCsvDto
{
    [Name("tipo_division")]
    public string TipoDivision { get; set; } = null!;

    [Name("codigo")]
    public string Codigo { get; set; } = null!;

    [Name("nombre")]
    public string Nombre { get; set; } = null!;

    [Name("codigo_padre")]
    public string? CodigoPadre { get; set; }

    [Name("region_geografica")]
    public string? RegionGeografica { get; set; }

    [Name("poblacion_estimada")]
    public int? PoblacionEstimada { get; set; }

    [Name("area_km2")]
    public decimal? AreaKm2 { get; set; }
}