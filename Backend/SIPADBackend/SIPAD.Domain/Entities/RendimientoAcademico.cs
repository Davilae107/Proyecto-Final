namespace SIPAD.Domain.Entities;

public class RendimientoAcademico
{
    public int IdRendimiento { get; set; }
    public int IdCentro { get; set; }
    public int IdPeriodo { get; set; }
    public string? NivelEducativo { get; set; }
    public decimal? PromedioCalificaciones { get; set; }
    public decimal? TasaAprobacion { get; set; }
    public decimal? TasaRepitencia { get; set; }
    public decimal? TasaSobreedad { get; set; }
    public int? TotalEstudiantes { get; set; }
    public int? EstudiantesAprobados { get; set; }
    public int? EstudiantesReprobados { get; set; }
    public int? EstudiantesSobreedad { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Relaciones
    public virtual CentroEducativo Centro { get; set; } = null!;
    public virtual PeriodoAcademico Periodo { get; set; } = null!;
}