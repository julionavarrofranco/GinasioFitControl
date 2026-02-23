namespace ProjetoFinal.Models
{
    public enum TipoSubscricao
    {
        Mensal,
        Trimestral,
        Anual
    }

    public class Subscricao
    {
        public int IdSubscricao { get; set; }

        public string Nome { get; set; } = null!;

        public TipoSubscricao Tipo { get; set; }

        public decimal Preco { get; set; }

        public string? Descricao { get; set; }

        public bool Ativo { get; set; } = true;

        public ICollection<Membro> Membros { get; set; } = new List<Membro>();

        public ICollection<Pagamento> Pagamentos { get; set; } = new List<Pagamento>();
    }
}
