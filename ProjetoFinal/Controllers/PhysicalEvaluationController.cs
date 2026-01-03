using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjetoFinal.Models.DTOs;
using ProjetoFinal.Services.Interfaces;

namespace ProjetoFinal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PhysicalEvaluationController : ControllerBase
    {
        private readonly IPhysicalEvaluationService _physicalEvaluationService;

        public PhysicalEvaluationController(IPhysicalEvaluationService physicalEvaluationService)
        {
            _physicalEvaluationService = physicalEvaluationService;
        }

        [Authorize(Policy = "OnlyPT")]
        [HttpPost]
        public async Task<IActionResult> CreateEvaluation([FromBody] PhysicalEvaluationDto request)
        {
            try
            {
                if (request == null)
                    return BadRequest(new { message = "Dados inválidos." });

                var avaliacao = await _physicalEvaluationService.CreatePhysicalEvaluationAsync(request);
                return Ok(avaliacao);
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

        [Authorize(Policy = "OnlyPT")]
        [HttpPatch("update/{idAvaliacao}")]
        public async Task<IActionResult> UpdateEvaluation(int idAvaliacao, [FromBody] UpdatePhysicalEvaluationDto request)
        {
            try
            {
                if (request == null)
                    return BadRequest(new { message = "Dados inválidos." });

                var result = await _physicalEvaluationService.UpdatePhysicalEvaluationAsync(idAvaliacao, request);
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

        [Authorize(Policy = "OnlyPT")]
        [HttpPatch("change-active-status/{idAvaliacao}")]
        public async Task<IActionResult> ChangePhysicalEvaluationStatus(int idAvaliacao, [FromQuery] bool ativo)
        {
            try
            {
                await _physicalEvaluationService.ChangePhysicalEvaluationActiveStatusAsync(idAvaliacao, ativo);

                return Ok(new { message = "Estado atualizado com sucesso." });
            }
            catch
            {
                return StatusCode(500, new { message = "Erro interno do servidor." });
            }
        }

        [Authorize(Policy = "OnlyPT")]
        [HttpGet("member/evaluations/{idMembro}")]
        public async Task<IActionResult> GetAllEvaluationsForMember(int idMembro)
        {
            try
            {
                var avaliacoes = await _physicalEvaluationService.GetAllEvaluationsFromMemberAsync(idMembro);
                return Ok(avaliacoes);
            }
            catch
            {
                return StatusCode(500, new { message = "Erro interno do servidor." });
            }
        }


        [Authorize(Policy = "OnlyMembers")]
        [HttpGet("member/latest-evaluation/{idMembro}")]
        public async Task<IActionResult> GetLatestEvaluationForMember(int idMembro)
        {
            try
            {
                var avaliacao = await _physicalEvaluationService.GetLatestEvaluationFromMemberAsync(idMembro);
                if (avaliacao == null)
                    return NotFound(new { message = "Nenhuma avaliação encontrada." });

                return Ok(avaliacao);
            }
            catch
            {
                return StatusCode(500, new { message = "Erro interno do servidor." });
            }
        }
    }
}
