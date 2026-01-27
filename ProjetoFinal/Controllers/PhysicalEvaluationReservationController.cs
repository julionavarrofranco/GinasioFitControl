using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProjetoFinal.Models.DTOs;
using ProjetoFinal.Services.Interfaces;

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
        [HttpPost("{idMembro}")]
        public async Task<IActionResult> CreateReservation(int idMembro, [FromQuery] DateTime dataReserva)
        {
            try
            {
                var reserva = await _reservationService.CreateReservationAsync(idMembro, dataReserva);
                return Ok(reserva);
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
        [HttpPatch("cancel/{idMembro}/{idAvaliacao}")]
        public async Task<IActionResult> CancelReservation(int idMembro, int idAvaliacao)
        {
            try
            {
                var success = await _reservationService.CancelReservationAsync(idMembro, idAvaliacao);
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

    }
}
