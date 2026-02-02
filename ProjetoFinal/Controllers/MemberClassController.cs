using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjetoFinal.Models.DTOs;
using ProjetoFinal.Services.Interfaces;

namespace ProjetoFinal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MemberClassController : ControllerBase
    {
        private readonly IMemberClassService _memberClassService;

        public MemberClassController(IMemberClassService memberClassService)
        {
            _memberClassService = memberClassService;
        }

        [Authorize(Policy = "OnlyMembers")]
        [HttpPost("reserve")]
        public async Task<IActionResult> ReserveClass([FromQuery] int idMembro, [FromQuery] int idAulaMarcada)
        {
            try
            {
                var reserva = await _memberClassService.ReservarAsync(idMembro, idAulaMarcada);
                var response = new ReserveClassResponseDto
                {
                    IdMembro = reserva.IdMembro,
                    IdAulaMarcada = reserva.IdAulaMarcada,
                    DataReserva = reserva.DataReserva,
                    Presenca = reserva.Presenca.ToString()
                };
                return Ok(response);
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

        [Authorize(Policy = "OnlyMembers")]
        [HttpPatch("cancel")]
        public async Task<IActionResult> CancelReservation([FromQuery] int idMembro, [FromQuery] int idAulaMarcada)
        {
            try
            {
                var result = await _memberClassService.CancelarReservaAsync(idMembro, idAulaMarcada);
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

        [Authorize(Policy = "OnlyMembers")]
        [HttpGet("member-reservations/{idMembro}")]
        public async Task<IActionResult> ListMemberReservations(int idMembro)
        {
            try
            {
                var reservas = await _memberClassService.ListarReservasDoMembroAsync(idMembro);
                return Ok(reservas);
            }
            catch
            {
                return StatusCode(500, new { message = "Erro interno do servidor." });
            }
        }

        [Authorize(Policy = "OnlyPT")]
        [HttpPatch("mark-attendance/{idAulaMarcada}")]
        public async Task<IActionResult> MarkAttendance(int idAulaMarcada, [FromBody] List<int> idsPresentes)
        {
            try
            {
                var result = await _memberClassService.MarcarPresencasAsync(idAulaMarcada, idsPresentes);
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
        [HttpGet("attendance/{idAulaMarcada}")]
        public async Task<IActionResult> GetClassForAttendance(int idAulaMarcada)
        {
            try
            {
                var dto = await _memberClassService.ObterAulaParaPresencaAsync(idAulaMarcada);
                return Ok(dto);
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

        [Authorize(Policy = "OnlyPT")]
        [HttpGet("by-pt/{idFuncionario}")]
        public async Task<IActionResult> ListReservationsByPt(int idFuncionario)
        {
            try
            {
                var list = await _memberClassService.ListarReservasPorPtAsync(idFuncionario);
                return Ok(list);
            }
            catch
            {
                return StatusCode(500, new { message = "Erro interno do servidor." });
            }
        }
    }
}
