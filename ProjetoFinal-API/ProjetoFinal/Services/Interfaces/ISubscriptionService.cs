using ProjetoFinal.Models;
using ProjetoFinal.Models.DTOs;

namespace ProjetoFinal.Services.Interfaces
{
    public interface ISubscriptionService
    {
        Task<Subscricao> CreateSubscriptionAsync(SubscriptionDto request);

        Task<string> UpdateSubscriptionAsync(int idSubscricao, UpdateSubscriptionDto request);

        Task ChangeSubscriptionActiveStatusAsync(int idSubscricao, bool ativo);

        Task<List<SubscriptionDto>> GetSubscriptionsByStateAsync(bool ativo, bool ordenarNomeAsc = true, bool? ordenarPrecoAsc = null);

        Task<List<Subscricao>> GetSubscriptionsByTypeAsync(TipoSubscricao tipo, bool ordenarNomeAsc = true, bool? ordenarPrecoAsc = null);

        Task<List<Subscricao>> GetSubscriptionsByNameAsync(string nome, bool ordenarNomeAsc = true, bool? ordenarPrecoAsc = null);
    }
}
