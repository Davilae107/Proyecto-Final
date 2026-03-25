using CsvHelper.Configuration.Attributes;

namespace SIPAD.Application.DTOs.Csv;

public class EmbarazoAdolescenteCsvDto
{
    [Name("codigo_municipio")]
    public string CodigoMunicipio { get; set; } = null!;

    [Name("anio")]
    public int Anio { get; set; }

    [Name("embarazos_10_14_anios")]
    public int Embarazos1014Anios { get; set; }

    [Name("embarazos_15_19_anios")]
    public int Embarazos1519Anios { get; set; }

    [Name("poblacion_femenina_10_14")]
    public int PoblacionFemenina1014 { get; set; }

    [Name("poblacion_femenina_15_19")]
    public int PoblacionFemenina1519 { get; set; }

    [Name("embarazos_estudiantes_secundaria")]
    public int? EmbarazosEstudiantesSecundaria { get; set; }

    [Name("madres_retornaron_escuela")]
    public int? MadresRetornaronEscuela { get; set; }

    [Name("centros_salud_reproductiva")]
    public int? CentrosSaludReproductiva { get; set; }
}