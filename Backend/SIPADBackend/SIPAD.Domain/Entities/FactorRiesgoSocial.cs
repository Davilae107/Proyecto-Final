namespace SIPAD.Domain.Entities;

public class FactorRiesgoSocial
{
    public int IdRiesgoSocial { get; set; }
    public int IdMunicipio { get; set; }
    public int Anio { get; set; }
    public decimal? TasaEmbarazoAdolescente { get; set; }
    public decimal? IndiceDelincuenciaJuvenil { get; set; }
    public int? CasosViolenciaIntrafamiliar { get; set; }
    public decimal? TasaConsumoSustancias { get; set; }
    public decimal? IndiceDesintegracionFamiliar { get; set; }
    public string? FuenteDatos { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Relaciones
    public virtual Municipio Municipio { get; set; } = null!;
}