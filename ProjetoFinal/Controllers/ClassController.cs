using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjetoFinal.Models;
using ProjetoFinal.Models.DTOs;
using ProjetoFinal.Services.Interfaces;

namespace ProjetoFinal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClassController : ControllerBase
    {
        private readonly IClassService _classService;

        public ClassController(IClassService classService)
        {
            _classService = classService;
        }

        [Authorize(Policy = "OnlyAdmin")]
        [HttpPost]
        public async Task<IActionResult> CreateClass([FromBody] ClassDto request)
        {
            try
            {
                if (request == null)
                    return BadRequest(new { message = "Dados da aula inválidos." });

                var aula = await _classService.CreateAsync(request);
                return Ok(aula);
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
        [HttpPatch("update/{idAula}")]
        public async Task<IActionResult> UpdateClass(int idAula, [FromBody] UpdateClassDto request)
        {
            try
            {
                if (request == null)
                    return BadRequest(new { message = "Dados de atualização inválidos." });

                var result = await _classService.UpdateAsync(idAula, request);
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
        [HttpPatch("swap/{idAulaA}/{idAulaB}")]
        public async Task<IActionResult> SwapClasses(int idAulaA, int idAulaB)
        {
            try
            {
                await _classService.SwapClassSlotsAsync(idAulaA, idAulaB);
                return Ok(new { message = "Swap realizado com sucesso." });
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
        [HttpPatch("assign-pt/{idAula}")]
        public async Task<IActionResult> AssignPt(int idAula, [FromQuery] int idPt)
        {
            try
            {
                var aula = await _classService.AssignPtAsync(idAula, idPt);
                return Ok(aula);
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
        [HttpPatch("change-active-status/{idAula}")]
        public async Task<IActionResult> ChangeActiveStatus(int idAula, [FromQuery] bool ativo)
        {
            try
            {
                await _classService.ChangeActiveStateAsync(idAula, ativo);
                return Ok(new { message = "Estado da aula atualizado com sucesso." });
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

        [Authorize(Policy = "CanViewClasses")]
        [HttpGet("by-state")]
        public async Task<IActionResult> ListByState([FromQuery] bool ativo)
        {
            try
            {
                var aulas = await _classService.ListByStateAsync(ativo);
                return Ok(aulas);
            }
            catch
            {
                return StatusCode(500, new { message = "Erro interno do servidor." });
            }
        }

        [Authorize(Policy = "CanViewClasses")]
        [HttpGet("by-day/{dia}")]
        public async Task<IActionResult> ListByDay(DiaSemana dia)
        {
            try
            {
                var aulas = await _classService.ListByDayAsync(dia);
                return Ok(aulas);
            }
            catch
            {
                return StatusCode(500, new { message = "Erro interno do servidor." });
            }
        }

        [Authorize(Policy = "CanViewClasses")]
        [HttpGet("by-pt/{idFuncionario}")]
        public async Task<IActionResult> ListByPt(int idFuncionario)
        {
            try
            {
                var aulas = await _classService.ListByPtAsync(idFuncionario);
                return Ok(aulas);
            }
            catch
            {
                return StatusCode(500, new { message = "Erro interno do servidor." });
            }
        }
    }
}
