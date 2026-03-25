namespace SIPAD.Domain.Entities;

public class CentroEducativo
{
    public int IdCentro { get; set; }
    public string CodigoCentro { get; set; } = null!;
    public string NombreCentro { get; set; } = null!;
    public int IdDistrito { get; set; }
    public int? IdMunicipio { get; set; }
    public string? Sector { get; set; }
    public string? Nivel { get; set; }
    public string? Zona { get; set; }
    public string? Direccion { get; set; }
    public decimal? Latitud { get; set; }
    public decimal? Longitud { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Relaciones
    public virtual DistritoEducativo Distrito { get; set; } = null!;
    public virtual Municipio? Municipio { get; set; }
    public virtual ICollection<RendimientoAcademico> RendimientosAcademicos { get; set; } = new List<RendimientoAcademico>();
}