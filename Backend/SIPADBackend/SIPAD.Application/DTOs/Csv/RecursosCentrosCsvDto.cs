using CsvHelper.Configuration.Attributes;

namespace SIPAD.Application.DTOs.Csv;

public class RecursosCentrosCsvDto
{
    [Name("codigo_centro")]
    public string CodigoCentro { get; set; } = null!;

    [Name("anio_escolar")]
    public string AnioEscolar { get; set; } = null!;

    [Name("total_docentes")]
    public int TotalDocentes { get; set; }

    [Name("docentes_titulados")]
    public int? DocentesTitulados { get; set; }

    [Name("total_aulas")]
    public int TotalAulas { get; set; }

    [Name("aulas_buen_estado")]
    public int? AulasBuenEstado { get; set; }

    [Name("tiene_biblioteca")]
    public string TieneBiblioteca { get; set; } = null!;

    [Name("tiene_laboratorio")]
    public string TieneLaboratorio { get; set; } = null!;

    [Name("tiene_cancha_deportiva")]
    public string? TieneCanchaDeportiva { get; set; }

    [Name("computadoras_disponibles")]
    public int? ComputadorasDisponibles { get; set; }

    [Name("acceso_internet")]
    public string AccesoInternet { get; set; } = null!;

    [Name("programa_alimentacion")]
    public string ProgramaAlimentacion { get; set; } = null!;

    [Name("programa_transporte")]
    public string ProgramaTransporte { get; set; } = null!;

    [Name("estado_infraestructura")]
    public string EstadoInfraestructura { get; set; } = null!;
}