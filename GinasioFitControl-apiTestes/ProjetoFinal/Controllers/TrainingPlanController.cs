using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjetoFinal.Models;
using ProjetoFinal.Models.DTOs;
using ProjetoFinal.Services;
using ProjetoFinal.Services.Interfaces;

namespace ProjetoFinal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TrainingPlanController : ControllerBase
    {
        private readonly ITrainingPlanService _trainingPlanService;

        public TrainingPlanController(ITrainingPlanService trainingPlanService)
        {
            _trainingPlanService = trainingPlanService;
        }

        [Authorize(Policy = "OnlyPT")]
        [HttpPost]
        public async Task<IActionResult> Create([FromQuery] int idFuncionario, [FromBody] TrainingPlanDto dto)
        {
            try
            {
                var plano = await _trainingPlanService.CreateAsync(idFuncionario, dto);
                return Ok(plano);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Erro interno do servidor." });
            }
        }

        [Authorize(Policy = "OnlyPT")]
        [HttpPatch("{idPlano}")]
        public async Task<IActionResult> Update(int idPlano, [FromBody] UpdateTrainingPlanDto request)
        {
            try
            {
                if (request == null)
                    return BadRequest(new { message = "Dados de atualização inválidos." });

                var result = await _trainingPlanService.UpdateAsync(idPlano, request);
                return Ok(new { message = result });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [Authorize(Policy = "OnlyPT")]
        [HttpPatch("change-active-state/{idPlano}")]
        public async Task<IActionResult> ChangeActiveState(int idPlano, [FromQuery] bool ativo)
        {
            try
            {
                await _trainingPlanService.ChangeActiveStateAsync(idPlano, ativo);
                return Ok(new { message = "Estado do plano atualizado com sucesso." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [Authorize(Policy = "OnlyPT")]
        [HttpPost("assign-to-member")]
        public async Task<IActionResult> AssignToMember([FromQuery] int idMembro, [FromQuery] int idPlano)
        {
            try
            {
                await _trainingPlanService.AtribuirPlanoAoMembroAsync(idMembro, idPlano);
                return Ok(new { message = "Plano atribuído ao membro com sucesso." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Policy = "OnlyPT")]
        [HttpDelete("remove-from-member/{idMembro}")]
        public async Task<IActionResult> RemoveFromMember(int idMembro)
        {
            try
            {
                await _trainingPlanService.RemoverPlanoDoMembroAsync(idMembro);
                return Ok(new { message = "Plano removido do membro." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpGet("current/{idMembro}")]
        public async Task<IActionResult> GetCurrentPlan(int idMembro)
        {
            var plano = await _trainingPlanService.GetPlanoAtualDoMembroAsync(idMembro);
            return Ok(plano);
        }

        [Authorize(Policy = "OnlyPT")]
        [HttpGet("history/{idMembro}")]
        public async Task<IActionResult> GetHistory(int idMembro)
        {
            try
            {
                var planos = await _trainingPlanService.GetHistoricoPlanosDoMembroAsync(idMembro);
                return Ok(planos);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Erro interno do servidor." });
            }
        }

        [Authorize(Policy = "OnlyPT")]
        [HttpGet("by-state")]
        public async Task<IActionResult> GetByState([FromQuery] bool ativo)
        {
            var planos = await _trainingPlanService.GetPlanosByEstadoAsync(ativo);
            return Ok(planos);
        }

        [Authorize(Policy = "OnlyPT")]
        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary([FromQuery] bool? ativo)
        {
            var resumo = await _trainingPlanService.GetPlanosResumoAsync(ativo);
            return Ok(resumo);
        }

        [Authorize(Policy = "OnlyPT")]
        [HttpGet("{idPlano}")]
        public async Task<IActionResult> GetDetail(int idPlano)
        {
            var detalhe = await _trainingPlanService.GetPlanoDetalheAsync(idPlano);
            if (detalhe == null)
                return NotFound(new { message = "Plano de treino não encontrado." });
            return Ok(detalhe);
        }
    }
}
