using CsvHelper.Configuration.Attributes;

namespace SIPAD.Application.DTOs.Csv;

public class CoberturaEducativaCsvDto
{
    [Name("codigo_municipio")]
    public string CodigoMunicipio { get; set; } = null!;

    [Name("anio")]
    public int Anio { get; set; }

    [Name("nivel_educativo")]
    public string NivelEducativo { get; set; } = null!;

    [Name("poblacion_edad_escolar")]
    public int PoblacionEdadEscolar { get; set; }

    [Name("estudiantes_matriculados")]
    public int EstudiantesMatriculados { get; set; }

    [Name("matriculados_sector_publico")]
    public int MatriculadosSectorPublico { get; set; }

    [Name("matriculados_sector_privado")]
    public int MatriculadosSectorPrivado { get; set; }

    [Name("zona_urbana")]
    public int ZonaUrbana { get; set; }

    [Name("zona_rural")]
    public int ZonaRural { get; set; }

    [Name("total_centros_publicos")]
    public int TotalCentrosPublicos { get; set; }

    [Name("total_centros_privados")]
    public int TotalCentrosPrivados { get; set; }

    [Name("distancia_promedio_km")]
    public decimal? DistanciaPromedioKm { get; set; }
}