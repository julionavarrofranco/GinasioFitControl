using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProjetoFinal.Models;
using ProjetoFinal.Models.DTOs;
using ProjetoFinal.Services.Interfaces;

namespace ProjetoFinal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubscriptionController : ControllerBase
    {
        private readonly ISubscriptionService _subscriptionService;

        public SubscriptionController(ISubscriptionService subscriptionService)
        {
            _subscriptionService = subscriptionService;
        }

        [Authorize(Policy = "OnlyAdmin")]
        [HttpPost]
        public async Task<IActionResult> CreateSubscription([FromBody] SubscriptionDto request)
        {
            try
            {
                if (request == null)
                    return BadRequest(new { message = "Dados da subscrição inválidos." });

                var subscricao = await _subscriptionService.CreateSubscriptionAsync(request);
                return Ok(subscricao);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch
            {
                return StatusCode(500, new { message = "Erro interno do servidor." });
            }
        }

        [Authorize(Policy = "OnlyAdmin")]
        [HttpPatch("update-subscription/{idSubscricao}")]
        public async Task<IActionResult> UpdateSubscription(int idSubscricao, [FromBody] UpdateSubscriptionDto request)
        {
            try
            {
                if (request == null)
                    return BadRequest(new { message = "Dados de atualização inválidos." });

                var result = await _subscriptionService.UpdateSubscriptionAsync(idSubscricao, request);
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
            catch
            {
                return StatusCode(500, new { message = "Erro interno do servidor." });
            }
        }

        [Authorize(Policy = "OnlyAdmin")]
        [HttpPatch("change-active-status/{idSubscricao}")]
        public async Task<IActionResult> ChangeSubscriptionStatus(int idSubscricao, [FromQuery] bool ativo)
        {
            try
            {
                await _subscriptionService.ChangeSubscriptionActiveStatusAsync(idSubscricao, ativo);

                return Ok(new { message = "Estado da subscrição atualizado com sucesso." });
            }
            catch
            {
                return StatusCode(500, new { message = "Erro interno do servidor." });
            }
        }

        [HttpGet("by-state")]
        public async Task<IActionResult> GetSubscriptionsByState([FromQuery] bool ativo, [FromQuery] bool ordenarNomeAsc = true, [FromQuery] bool? ordenarPrecoAsc = null)
        {
            try
            {
                var subscricoes = await _subscriptionService.GetSubscriptionsByStateAsync(ativo, ordenarNomeAsc, ordenarPrecoAsc);
                return Ok(subscricoes);
            }
            catch
            {
                return StatusCode(500, new { message = "Erro interno do servidor." });
            }
        }

        [Authorize(Policy = "CanViewSubscriptions")]
        [HttpGet("by-type/{tipo}")]
        public async Task<IActionResult> GetSubscriptionsByType(TipoSubscricao tipo, [FromQuery] bool ordenarNomeAsc = true, [FromQuery] bool? ordenarPrecoAsc = null)
        {
            try
            {
                var subscricoes = await _subscriptionService.GetSubscriptionsByTypeAsync(tipo, ordenarNomeAsc, ordenarPrecoAsc);
                return Ok(subscricoes);
            }
            catch
            {
                return StatusCode(500, new { message = "Erro interno do servidor." });
            }
        }

        [Authorize(Policy = "CanViewSubscriptions")]
        [HttpGet("by-name")]
        public async Task<IActionResult> GetSubscriptionsByName([FromQuery] string nome, [FromQuery] bool ordenarNomeAsc = true, [FromQuery] bool? ordenarPrecoAsc = null)
        {
            try
            {
                var subscricoes = await _subscriptionService.GetSubscriptionsByNameAsync(nome, ordenarNomeAsc, ordenarPrecoAsc);
                return Ok(subscricoes);
            }
            catch
            {
                return StatusCode(500, new { message = "Erro interno do servidor." });
            }
        }    
    }
}
