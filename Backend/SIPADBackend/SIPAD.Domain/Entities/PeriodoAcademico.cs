namespace SIPAD.Domain.Entities;

public class PeriodoAcademico
{
    public int IdPeriodo { get; set; }
    public string AnioEscolar { get; set; } = null!;
    public string? Periodo { get; set; }
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }
    public bool Activo { get; set; }
    public DateTime CreatedAt { get; set; }

    // Relaciones
    public virtual ICollection<RendimientoAcademico> RendimientosAcademicos { get; set; } = new List<RendimientoAcademico>();
}