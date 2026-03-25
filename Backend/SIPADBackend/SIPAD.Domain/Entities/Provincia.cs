namespace SIPAD.Domain.Entities;

public class Provincia
{
    public int IdProvincia { get; set; }
    public string CodigoProvincia { get; set; } = null!;
    public string NombreProvincia { get; set; } = null!;
    public string? RegionGeografica { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Relaciones
    public virtual ICollection<Municipio> Municipios { get; set; } = new List<Municipio>();
}