public class CurrentUserDto
{
    public int IdUser { get; set; }
    public int? IdMembro { get; set; }
    public string Email { get; set; } = "";
    public string Tipo { get; set; } = "";
    public string Nome { get; set; } = "";
    public string Telemovel { get; set; } = "";
    public DateTime? DataNascimento { get; set; }
    public int? IdSubscricao { get; set; }
}
