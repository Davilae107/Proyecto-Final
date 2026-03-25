namespace SIPAD.Domain.Entities;

public class IndicadorSocioeconomico
{
    public int IdIndicadorSocio { get; set; }
    public int IdMunicipio { get; set; }
    public int Anio { get; set; }
    public decimal? IngresoPromedioMensual { get; set; }
    public decimal? TasaPobrezaMonetaria { get; set; }
    public decimal? TasaPobrezaExtrema { get; set; }
    public decimal? IndiceTrabajoInfantil { get; set; }
    public decimal? TasaDesempleo { get; set; }
    public decimal? NivelInformalidadLaboral { get; set; }
    public string? FuenteDatos { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Relaciones
    public virtual Municipio Municipio { get; set; } = null!;
}