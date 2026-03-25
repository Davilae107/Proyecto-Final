using CsvHelper.Configuration.Attributes;

namespace SIPAD.Application.DTOs.Csv;

public class MatriculaDesercionCsvDto
{
    [Name("codigo_centro")]
    public string CodigoCentro { get; set; } = null!;

    [Name("anio_escolar")]
    public string AnioEscolar { get; set; } = null!;

    [Name("nivel_educativo")]
    public string NivelEducativo { get; set; } = null!;

    [Name("grado")]
    public string? Grado { get; set; }

    [Name("matricula_inicial")]
    public int MatriculaInicial { get; set; }

    [Name("matricula_final")]
    public int MatriculaFinal { get; set; }

    [Name("abandonos_temporales")]
    public int AbandonosTemporales { get; set; }

    [Name("abandonos_definitivos")]
    public int AbandonosDefinitivos { get; set; }

    [Name("causante_principal")]
    public string? CausantePrincipal { get; set; }

    [Name("mes_mayor_desercion")]
    public string? MesMayorDesercion { get; set; }
}