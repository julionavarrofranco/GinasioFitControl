using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjetoFinal.Models.DTOs;
using ProjetoFinal.Services.Interfaces;

namespace ProjetoFinal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExercisePlanController : ControllerBase
    {
        private readonly IExercisePlanService _exercisePlanService;

        public ExercisePlanController(IExercisePlanService exercisePlanService)
        {
            _exercisePlanService = exercisePlanService;
        }

        [Authorize(Policy = "OnlyPT")]
        [HttpPost("{idPlano}")]
        public async Task<IActionResult> AddExerciseToPlan(int idPlano, [FromBody] ExercisePlanDto dto)
        {
            try
            {
                await _exercisePlanService.AddAsync(idPlano, dto);
                return Ok(new { message = "Exercício adicionado ao plano." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize(Policy = "OnlyPT")]
        [HttpPatch("{idPlano}/{idExercicio}")]
        public async Task<IActionResult> UpdateExerciseInPlan(
            int idPlano,
            int idExercicio,
            [FromBody] UpdateExercisePlanDto dto)
        {
            try
            {
                await _exercisePlanService.UpdateAsync(idPlano, idExercicio, dto);
                return Ok(new { message = "Exercício atualizado no plano." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [Authorize(Policy = "OnlyPT")]
        [HttpDelete("{idPlano}/{idExercicio}")]
        public async Task<IActionResult> RemoveExerciseFromPlan(int idPlano, int idExercicio)
        {
            try
            {
                await _exercisePlanService.DeleteAsync(idPlano, idExercicio);
                return Ok(new { message = "Exercício removido do plano." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
