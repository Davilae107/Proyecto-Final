using CsvHelper.Configuration.Attributes;

namespace SIPAD.Application.DTOs.Csv;

public class CentroEducativoCsvDto
{
    [Name("codigo_centro")]
    public string CodigoCentro { get; set; } = null!;

    [Name("nombre_centro")]
    public string NombreCentro { get; set; } = null!;

    [Name("codigo_distrito")]
    public string CodigoDistrito { get; set; } = null!;

    [Name("codigo_municipio")]
    public string CodigoMunicipio { get; set; } = null!;

    [Name("sector")]
    public string? Sector { get; set; }

    [Name("nivel")]
    public string? Nivel { get; set; }

    [Name("zona")]
    public string? Zona { get; set; }

    [Name("direccion")]
    public string? Direccion { get; set; }

    [Name("latitud")]
    public decimal? Latitud { get; set; }

    [Name("longitud")]
    public decimal? Longitud { get; set; }

    [Name("telefono")]
    public string? Telefono { get; set; }

    [Name("email")]
    public string? Email { get; set; }

    [Name("director")]
    public string? Director { get; set; }
}