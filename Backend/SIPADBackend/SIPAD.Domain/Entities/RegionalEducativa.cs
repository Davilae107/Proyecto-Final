namespace SIPAD.Domain.Entities;

public class RegionalEducativa
{
    public int IdRegional { get; set; }
    public string CodigoRegional { get; set; } = null!;
    public string NombreRegional { get; set; } = null!;
    public string? SedeRegional { get; set; }
    public string? Telefono { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Relaciones
    public virtual ICollection<DistritoEducativo> DistritosEducativos { get; set; } = new List<DistritoEducativo>();
}