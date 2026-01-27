using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjetoFinal.Models.DTOs;
using ProjetoFinal.Services.Interfaces;

namespace ProjetoFinal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScheduleClassController : ControllerBase
    {
        private readonly IScheduleClassService _scheduleClassService;

        public ScheduleClassController(IScheduleClassService scheduleClassService)
        {
            _scheduleClassService = scheduleClassService;
        }

        // PT cria aula marcada individualmente
        [Authorize(Policy = "OnlyPT")]
        [HttpPost("create")]
        public async Task<IActionResult> CreateScheduledClass([FromBody] ScheduleClassDto request)
        {
            try
            {
                if (request == null)
                    return BadRequest(new { message = "Dados da aula marcada inválidos." });

                var aulaMarcada = await _scheduleClassService.CreateAsync(request);
                return Ok(aulaMarcada);
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

        // PT gera todas as aulas agendadas para si nos próximos X dias
        [Authorize(Policy = "OnlyPT")]
        [HttpPost("generate-for-pt/{idPt}")]
        public async Task<IActionResult> GenerateScheduledClassesForPt(int idPt)
        {
            try
            {
                var result = await _scheduleClassService.GenerateScheduledClassesForPtAsync(idPt);
                return Ok(new { message = $"{result} aulas geradas com sucesso." });
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

        // PT cancela aula manualmente
        [Authorize(Policy = "OnlyPT")]
        [HttpPatch("cancel/{idAulaMarcada}")]
        public async Task<IActionResult> CancelScheduledClass(int idAulaMarcada)
        {
            try
            {
                var result = await _scheduleClassService.CancelByPtAsync(idAulaMarcada);
                return Ok(new { message = result });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch
            {
                return StatusCode(500, new { message = "Erro interno do servidor." });
            }
        }
    }
}
