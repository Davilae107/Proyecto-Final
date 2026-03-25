namespace SIPAD.Domain.Entities;

public class ServicioBasico
{
    public int IdServicio { get; set; }
    public int IdMunicipio { get; set; }
    public int Anio { get; set; }
    public decimal? CoberturaAguaPotable { get; set; }
    public decimal? CoberturaEnergiaElectrica { get; set; }
    public decimal? CoberturaSaneamientoBasico { get; set; }
    public decimal? AccesoInternet { get; set; }
    public string? FuenteDatos { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Relaciones
    public virtual Municipio Municipio { get; set; } = null!;
}