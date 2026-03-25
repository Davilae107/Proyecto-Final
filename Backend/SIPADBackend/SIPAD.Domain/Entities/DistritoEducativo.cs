namespace SIPAD.Domain.Entities;

public class DistritoEducativo
{
    public int IdDistrito { get; set; }
    public string CodigoDistrito { get; set; } = null!;
    public string NombreDistrito { get; set; } = null!;
    public int IdRegional { get; set; }
    public int? IdProvincia { get; set; }
    public int? IdMunicipio { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Relaciones
    public virtual RegionalEducativa Regional { get; set; } = null!;
    public virtual Provincia? Provincia { get; set; }
    public virtual Municipio? Municipio { get; set; }
    public virtual ICollection<CentroEducativo> CentrosEducativos { get; set; } = new List<CentroEducativo>();
}