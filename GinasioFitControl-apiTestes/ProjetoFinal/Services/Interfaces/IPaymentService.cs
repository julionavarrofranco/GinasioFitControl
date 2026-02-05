using ProjetoFinal.Models;
using ProjetoFinal.Models.DTOs;

namespace ProjetoFinal.Services.Interfaces
{
    public interface IPaymentService
    {
        Task CreatePaymentAsync(PaymentDto request);
        Task<string> UpdatePaymentAsync(int idPagamento, UpdatePaymentDto request);
        Task ChangePaymentActiveStateAsync(int idPagamento, bool ativo);
        Task<List<PaymentResponseDto>> GetPaymentsByActiveStateAsync(bool ativo);
        Task<List<PaymentResponseDto>> GetPaymentsByDateAsync(DateTime inicio, DateTime fim);
        Task<List<PaymentResponseDto>> GetPaymentsByPaymentStateAsync(EstadoPagamento estado);
        Task<decimal> GetMonthlyRevenueAsync(int ano, int mes);
    }
}
