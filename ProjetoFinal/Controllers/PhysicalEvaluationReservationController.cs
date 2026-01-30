using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProjetoFinal.Models;
using ProjetoFinal.Models.DTOs;
using ProjetoFinal.Services.Interfaces;
using System.Security.Claims;

namespace ProjetoFinal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PhysicalEvaluationReservationController : ControllerBase
    {
        private readonly IPhysicalEvaluationReservationService _reservationService;

        public PhysicalEvaluationReservationController(IPhysicalEvaluationReservationService reservationService)
        {
            _reservationService = reservationService;
        }

        [Authorize(Policy = "OnlyMembers")]
        [HttpPost("reservation")]
        public async Task<IActionResult> CreateReservation([FromQuery] DateTime dataReserva)
        {
            try
            {
                var idUser = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

                var reserva = await _reservationService.CreateReservationAsync(idUser, dataReserva);

                return Ok(new
                {
                    reserva.IdMembroAvaliacao,
                    reserva.IdMembro,
                    reserva.DataReserva,
                    reserva.Estado
                });

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
        [HttpPatch("cancel/{idAvaliacao}")]
        public async Task<IActionResult> CancelReservation(int idAvaliacao)
        {
            try
            {
                var idUser = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
                var success = await _reservationService.CancelReservationAsync(idUser, idAvaliacao);
                if (!success)
                    return NotFound(new { message = "Reserva não encontrada ou não está ativa." });

                return Ok(new { message = "Reserva cancelada com sucesso." });
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
        [HttpPatch("confirm-reservation/{idMembro}/{idAvaliacao}")]
        public async Task<IActionResult> ConfirmReservation(int idMembro, int idAvaliacao, [FromBody] ConfirmReservationDto request)
        {
            try
            {
                var success = await _reservationService.ConfirmReservationAsync(idMembro, idAvaliacao, request.IdFuncionario);
                if (!success)
                    return NotFound(new { message = "Reserva não encontrada ou não está em estado reservado." });

                return Ok(new { message = "Reserva confirmada com sucesso." });
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
        [HttpPatch("attendance/{idMembro}/{idAvaliacao}")]
        public async Task<IActionResult> MarkAttendance(int idMembro, int idAvaliacao, [FromBody] MarkAttendanceDto request)
        {
            try
            {
                var success = await _reservationService.MarkAttendanceAsync(idMembro, idAvaliacao, request);
                if (!success)
                    return NotFound(new { message = "Reserva não encontrada ou não está em estado Presente." });

                return Ok(new { message = "Presença registrada com sucesso." });
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
        [HttpGet("active")]
        public async Task<IActionResult> GetActiveReservations()
        {
            try
            {
                var reservas = await _reservationService.GetReservationsAsync();
                return Ok(reservas);
            }
            catch
            {
                return StatusCode(500, new { message = "Erro interno do servidor." });
            }
        }

        [Authorize(Policy = "OnlyPT")]
        [HttpGet("completed")]
        public async Task<IActionResult> GetCompletedReservations()
        {
            try
            {
                var reservas = await _reservationService.GetCompletedReservationsAsync();
                return Ok(reservas);
            }
            catch
            {
                return StatusCode(500, new { message = "Erro interno do servidor." });
            }
        }

        [Authorize(Policy = "OnlyMembers")]
        [HttpGet("active/{idMembro}")]
        public async Task<IActionResult> GetActiveReservationByMember(int idMembro)
        {
            var reserva = await _reservationService.GetActiveReservationByMemberAsync(idMembro);
            if (reserva == null) return NoContent();
            return Ok(reserva);
        }
    }
}
