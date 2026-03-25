namespace SIPAD.Domain.Entities;

public class EstadisticaAbandono
{
    public int IdEstadistica { get; set; }
    public int IdCentro { get; set; }
    public int IdPeriodo { get; set; }
    public string? NivelEducativo { get; set; }
    public decimal? TasaDesercionHistorica { get; set; }
    public decimal? TasaAbandonoInteranual { get; set; }
    public int? TotalAbandonos { get; set; }
    public int? TotalMatriculados { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Relaciones
    public virtual CentroEducativo Centro { get; set; } = null!;
    public virtual PeriodoAcademico Periodo { get; set; } = null!;
}