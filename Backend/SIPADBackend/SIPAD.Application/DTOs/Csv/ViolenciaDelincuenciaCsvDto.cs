using CsvHelper.Configuration.Attributes;

namespace SIPAD.Application.DTOs.Csv;

public class ViolenciaDelincuenciaCsvDto
{
    [Name("codigo_municipio")]
    public string CodigoMunicipio { get; set; } = null!;

    [Name("anio")]
    public int Anio { get; set; }

    [Name("casos_violencia_intrafamiliar")]
    public int CasosViolenciaIntrafamiliar { get; set; }

    [Name("casos_abuso_menores")]
    public int CasosAbusoMenores { get; set; }

    [Name("delitos_cometidos_menores")]
    public int DelitosCometidosMenores { get; set; }

    [Name("homicidios_menores")]
    public int HomicidiosMenores { get; set; }

    [Name("casos_drogas_menores")]
    public int CasosDrogasMenores { get; set; }

    [Name("pandillas_activas")]
    public int? PandillasActivas { get; set; }

    [Name("menores_institucionalizados")]
    public int? MenoresInstitucionalizados { get; set; }

    [Name("poblacion_menor_18")]
    public int PoblacionMenor18 { get; set; }
}