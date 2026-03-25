namespace SIPAD.Domain.Entities;

public class RecursoEducativo
{
    public int IdRecurso { get; set; }
    public int IdCentro { get; set; }
    public int IdPeriodo { get; set; }
    public int? TotalEstudiantes { get; set; }
    public int? TotalDocentes { get; set; }
    public decimal? RatioEstudianteDocente { get; set; }
    public string? DisponibilidadMaterialesDidacticos { get; set; }
    public bool TieneBiblioteca { get; set; }
    public bool TieneLaboratorio { get; set; }
    public bool TieneTecnologiaEducativa { get; set; }
    public bool ProgramaAlimentacionEscolar { get; set; }
    public bool ProgramaTransporteEscolar { get; set; }
    public string? EstadoInfraestructura { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Relaciones
    public virtual CentroEducativo Centro { get; set; } = null!;
    public virtual PeriodoAcademico Periodo { get; set; } = null!;
}