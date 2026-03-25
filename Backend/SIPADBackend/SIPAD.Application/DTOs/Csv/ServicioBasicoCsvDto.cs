using CsvHelper.Configuration.Attributes;

namespace SIPAD.Application.DTOs.Csv;

public class ServicioBasicoCsvDto
{
    [Name("codigo_municipio")]
    public string CodigoMunicipio { get; set; } = null!;

    [Name("anio")]
    public int Anio { get; set; }

    [Name("cobertura_agua_potable")]
    public decimal? CoberturaAguaPotable { get; set; }

    [Name("cobertura_energia_electrica")]
    public decimal? CoberturaEnergiaElectrica { get; set; }

    [Name("cobertura_alcantarillado")]
    public decimal? CoberturaAlcantarillado { get; set; }

    [Name("cobertura_recogida_basura")]
    public decimal? CoberturaRecogidaBasura { get; set; }

    [Name("acceso_internet_hogar")]
    public decimal? AccesoInternetHogar { get; set; }

    [Name("acceso_telefonia_movil")]
    public decimal? AccesoTelefoniaMovil { get; set; }

    [Name("centros_salud_por_1000")]
    public decimal? CentrosSaludPor1000 { get; set; }

    [Name("camas_hospital_por_1000")]
    public decimal? CamasHospitalPor1000 { get; set; }

    [Name("calidad_carreteras")]
    public string? CalidadCarreteras { get; set; }
}