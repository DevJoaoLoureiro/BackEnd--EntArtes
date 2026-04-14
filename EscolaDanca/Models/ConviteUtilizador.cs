public class ConviteUtilizador
{
    public int Id { get; set; }
    public string Email { get; set; } = null!;
    public string Perfil { get; set; } = null!;
    public string Token { get; set; } = null!;
    public DateTime ExpiraEm { get; set; }
    public bool Usado { get; set; } = false;
    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    public int CriadoPorUtilizadorId { get; set; }
}