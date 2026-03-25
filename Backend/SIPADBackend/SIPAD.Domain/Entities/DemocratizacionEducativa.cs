namespace SIPAD.Domain.Entities;

public class DemocratizacionEducativa
{
    public int IdDemocratizacion { get; set; }
    public int IdMunicipio { get; set; }
    public int Anio { get; set; }
    public string? NivelEducativo { get; set; }
    public decimal? TasaCoberturaNeta { get; set; }
    public decimal? BrechaUrbanoRural { get; set; }
    public decimal? IndiceInequidadEducativa { get; set; }
    public decimal? CentrosPorMilEstudiantes { get; set; }
    public decimal? DistanciaPromedioCentroKm { get; set; }
    public string? FuenteDatos { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Relaciones
    public virtual Municipio Municipio { get; set; } = null!;
}