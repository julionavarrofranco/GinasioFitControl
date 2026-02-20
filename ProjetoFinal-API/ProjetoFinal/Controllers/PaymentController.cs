using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjetoFinal.Models;
using ProjetoFinal.Models.DTOs;
using ProjetoFinal.Services.Interfaces;

namespace ProjetoFinal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [Authorize(Policy = "CanManagePayments")]
        [HttpPost]

        public async Task<IActionResult> CreatePayment([FromBody] PaymentDto request)
        {
            try
            {
                if (request == null)
                    return BadRequest(new { message = "Dados de pagamento inválidos." });

                await _paymentService.CreatePaymentAsync(request);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Erro interno do servidor." });
            }
        }

        [Authorize(Policy = "CanManagePayments")]
        [HttpPatch("update-payment/{idPagamento}")]

        public async Task<IActionResult> UpdatePayment(int idPagamento, [FromBody] UpdatePaymentDto request)
        {
            try
            {
                if (request == null)
                    return BadRequest(new { message = "Dados de atualização inválidos." });

                var result = await _paymentService.UpdatePaymentAsync(idPagamento, request);
                return Ok(new { message = result });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Erro interno do servidor." });
            }
        }

        [Authorize(Policy = "CanManagePayments")]
        [HttpPatch("change-active-status/{idPagamento}")]

        public async Task<IActionResult> ChangePaymentState(int idPagamento, [FromQuery] bool ativo)
        {
            try
            {
                await _paymentService.ChangePaymentActiveStateAsync(idPagamento, ativo);
                return Ok(new { message = $"Pagamento {(ativo ? "ativado" : "desativado")} com sucesso." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Erro interno do servidor." });
            }
        }

        [Authorize(Policy = "CanManagePayments")]
        [HttpGet("by-state")]

        public async Task<IActionResult> GetPaymentsByActiveState([FromQuery] bool ativo)
        {
            try
            {
                var pagamentos = await _paymentService.GetPaymentsByActiveStateAsync(ativo);
                return Ok(pagamentos);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Erro interno do servidor." });
            }
        }

        [Authorize(Policy = "CanManagePayments")]
        [HttpGet("by-date")]

        public async Task<IActionResult> GetPaymentsByDate([FromQuery] DateTime inicio, [FromQuery] DateTime fim)
        {
            try
            {
                var pagamentos = await _paymentService.GetPaymentsByDateAsync(inicio, fim);
                return Ok(pagamentos);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Erro interno do servidor." });
            }
        }

        [Authorize(Policy = "CanManagePayments")]
        [HttpGet("by-state-payment")]

        public async Task<IActionResult> GetPaymentsByPaymentState([FromQuery] EstadoPagamento estado)
        {
            try
            {
                var pagamentos = await _paymentService.GetPaymentsByPaymentStateAsync(estado);
                return Ok(pagamentos);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Erro interno do servidor." });
            }
        }

        [Authorize(Policy = "OnlyAdmin")]
        [HttpGet("monthly-revenue")]

        public async Task<IActionResult> GetMonthlyRevenue([FromQuery] int ano, [FromQuery] int mes)
        {
            try
            {
                var receita = await _paymentService.GetMonthlyRevenueAsync(ano, mes);
                return Ok(new { receita });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Erro interno do servidor." });
            }
        }
    }
}
