using CsvHelper.Configuration.Attributes;

namespace SIPAD.Application.DTOs.Csv;

public class PruebasNacionalesCsvDto
{
    [Name("codigo_centro")]
    public string CodigoCentro { get; set; } = null!;

    [Name("anio_escolar")]
    public string AnioEscolar { get; set; } = null!;

    [Name("nivel_evaluado")]
    public string NivelEvaluado { get; set; } = null!;

    [Name("total_evaluados")]
    public int TotalEvaluados { get; set; }

    [Name("promedio_lengua_espanola")]
    public decimal PromedioLenguaEspanola { get; set; }

    [Name("promedio_matematicas")]
    public decimal PromedioMatematicas { get; set; }

    [Name("promedio_ciencias_sociales")]
    public decimal? PromedioSocialesCiencias { get; set; }

    [Name("promedio_ciencias_naturales")]
    public decimal? PromedioNaturalesCiencias { get; set; }

    [Name("promedio_general")]
    public decimal PromedioGeneral { get; set; }

    [Name("porcentaje_aprobados")]
    public decimal PorcentajeAprobados { get; set; }
}