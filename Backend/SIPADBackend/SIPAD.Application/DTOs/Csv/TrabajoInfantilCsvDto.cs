using CsvHelper.Configuration.Attributes;

namespace SIPAD.Application.DTOs.Csv;

public class TrabajoInfantilCsvDto
{
    [Name("codigo_municipio")]
    public string CodigoMunicipio { get; set; } = null!;

    [Name("anio")]
    public int Anio { get; set; }

    [Name("menores_trabajando_5_14")]
    public int MenoresTrabajando514 { get; set; }

    [Name("menores_trabajando_15_17")]
    public int MenoresTrabajando1517 { get; set; }

    [Name("poblacion_5_14")]
    public int Poblacion514 { get; set; }

    [Name("poblacion_15_17")]
    public int Poblacion1517 { get; set; }

    [Name("sector_agricultura")]
    public int? SectorAgricultura { get; set; }

    [Name("sector_comercio")]
    public int? SectorComercio { get; set; }

    [Name("sector_servicios")]
    public int? SectorServicios { get; set; }

    [Name("sector_domestico")]
    public int? SectorDomestico { get; set; }

    [Name("trabajan_estudian")]
    public int? TrabajanEstudian { get; set; }

    [Name("trabajan_no_estudian")]
    public int TrabajanNoEstudian { get; set; }
}