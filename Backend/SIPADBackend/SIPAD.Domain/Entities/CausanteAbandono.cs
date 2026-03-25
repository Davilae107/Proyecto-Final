namespace SIPAD.Domain.Entities;

public class CausanteAbandono
{
    public int IdCausante { get; set; }
    public string NombreCausante { get; set; } = null!;
    public string? Categoria { get; set; }
    public string? Descripcion { get; set; }
    public DateTime CreatedAt { get; set; }
}