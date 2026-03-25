using CsvHelper.Configuration.Attributes;

namespace SIPAD.Application.DTOs.Csv;

public class RendimientoAcademicoCsvDto
{
    [Name("codigo_centro")]
    public string CodigoCentro { get; set; } = null!;

    [Name("anio_escolar")]
    public string AnioEscolar { get; set; } = null!;

    [Name("nivel_educativo")]
    public string NivelEducativo { get; set; } = null!;

    [Name("total_estudiantes")]
    public int TotalEstudiantes { get; set; }

    [Name("estudiantes_aprobados")]
    public int EstudiantesAprobados { get; set; }

    [Name("estudiantes_reprobados")]
    public int EstudiantesReprobados { get; set; }

    [Name("estudiantes_retirados")]
    public int EstudiantesRetirados { get; set; }

    [Name("estudiantes_sobreedad")]
    public int EstudiantesSobreedad { get; set; }

    [Name("promedio_calificaciones")]
    public decimal? PromedioCalificaciones { get; set; }

    [Name("promedio_asistencia")]
    public decimal? PromedioAsistencia { get; set; }
}