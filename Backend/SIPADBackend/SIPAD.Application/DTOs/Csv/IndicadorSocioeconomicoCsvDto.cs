using CsvHelper.Configuration.Attributes;

namespace SIPAD.Application.DTOs.Csv;

public class IndicadorSocioeconomicoCsvDto
{
    [Name("codigo_municipio")]
    public string CodigoMunicipio { get; set; } = null!;

    [Name("anio")]
    public int Anio { get; set; }

    [Name("poblacion_total")]
    public int? PoblacionTotal { get; set; }

    [Name("ingreso_promedio_mensual")]
    public decimal? IngresoPromedioMensual { get; set; }

    [Name("tasa_pobreza_monetaria")]
    public decimal? TasaPobrezaMonetaria { get; set; }

    [Name("tasa_pobreza_extrema")]
    public decimal? TasaPobrezaExtrema { get; set; }

    [Name("tasa_desempleo")]
    public decimal? TasaDesempleo { get; set; }

    [Name("tasa_informalidad")]
    public decimal? TasaInformalidad { get; set; }

    [Name("indice_gini")]
    public decimal? IndiceGini { get; set; }

    [Name("pib_per_capita")]
    public decimal? PibPerCapita { get; set; }
}