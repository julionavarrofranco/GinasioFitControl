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
    public class ExerciseController : ControllerBase
    {
        private readonly IExerciseService _exerciseService;

        public ExerciseController(IExerciseService exerciseService)
        {
            _exerciseService = exerciseService;
        }


        [Authorize(Policy = "OnlyAdmin")]
        [HttpPost]

        public async Task<IActionResult> CreateExercise([FromBody] ExerciseDto request)
        {
            try
            {
                if (request == null)
                    return BadRequest(new { message = "Dados do exercício inválidos." });

                var exercicio = await _exerciseService.CreateExerciseAsync(request);
                return Ok(exercicio);
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

        [Authorize(Policy = "OnlyAdmin")]
        [HttpPatch("update-exercise/{idExercicio}")]

        public async Task<IActionResult> UpdateExercise(int idExercicio, [FromBody] UpdateExerciseDto request)
        {
            try
            {
                if (request == null)
                    return BadRequest(new { message = "Dados de atualização inválidos." });

                var result = await _exerciseService.UpdateExerciseAsync(idExercicio, request);
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

        [Authorize(Policy = "OnlyAdmin")]
        [HttpPatch("change-active-status/{idExercicio}")]

        public async Task<IActionResult> ChangeExerciseStatus(int idExercicio, [FromQuery] bool ativo)
        {
            try
            {
                await _exerciseService.ChangeExerciseActiveStatusAsync(idExercicio, ativo);

                return Ok(new { message = "Estado do exercício atualizado com sucesso." });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Erro interno do servidor." });
            }
        }

        [Authorize(Policy = "CanViewExercises")]
        [HttpGet("by-state")]

        public async Task<IActionResult> GetExercisesByState([FromQuery] bool ativo, [FromQuery] bool ordenarAsc = true)
        {
            try
            {
                var exercicios = await _exerciseService.GetExercisesByStateAsync(ativo, ordenarAsc);
                return Ok(exercicios);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Erro interno do servidor." });
            }
        }

        [Authorize(Policy = "CanViewExercises")]
        [HttpGet("by-muscle-group/{grupo}")]

        public async Task<IActionResult> GetExercisesByMuscleGroup(GrupoMuscular grupo, [FromQuery] bool ordenarAsc = true)
        {
            try
            {
                var exercicios = await _exerciseService.GetExercisesByMuscleGroupAsync(grupo, ordenarAsc);
                return Ok(exercicios);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Erro interno do servidor." });
            }
        }

        [Authorize(Policy = "CanViewExercises")]
        [HttpGet("by-name")]

        public async Task<IActionResult> GetExercisesByName([FromQuery] string nome, [FromQuery] bool ordenarAsc = true)
        {
            try
            {
                var exercicios = await _exerciseService.GetExercisesByNameAsync(nome, ordenarAsc);
                return Ok(exercicios);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Erro interno do servidor." });
            }
        }
    }
}
