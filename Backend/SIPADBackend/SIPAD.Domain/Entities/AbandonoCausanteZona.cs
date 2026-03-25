namespace SIPAD.Domain.Entities;

public class AbandonoCausanteZona
{
    public int IdRegistro { get; set; }
    public int IdMunicipio { get; set; }
    public int IdCausante { get; set; }
    public int Anio { get; set; }
    public decimal? PesoCausal { get; set; }
    public int? CasosReportados { get; set; }
    public DateTime CreatedAt { get; set; }

    // Relaciones
    public virtual Municipio Municipio { get; set; } = null!;
    public virtual CausanteAbandono Causante { get; set; } = null!;
}