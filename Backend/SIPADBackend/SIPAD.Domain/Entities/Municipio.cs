namespace SIPAD.Domain.Entities;

public class Municipio
{
    public int IdMunicipio { get; set; }
    public string CodigoMunicipio { get; set; } = null!;
    public string NombreMunicipio { get; set; } = null!;
    public int IdProvincia { get; set; }
    public int? PoblacionEstimada { get; set; }
    public decimal? AreaKm2 { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Relaciones
    public virtual Provincia Provincia { get; set; } = null!;
    public virtual ICollection<CentroEducativo> CentrosEducativos { get; set; } = new List<CentroEducativo>();
    public virtual ICollection<IndicadorSocioeconomico> IndicadoresSocioeconomicos { get; set; } = new List<IndicadorSocioeconomico>();
    public virtual ICollection<ServicioBasico> ServiciosBasicos { get; set; } = new List<ServicioBasico>();
}