using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TTFWebsite.Models;
using TTFWebsite.Models.DTOs;
using TTFWebsite.Services;
using TTFWebsite.ViewModels;

[Authorize]
public class MemberController : Controller
{
    private readonly IApiService _api;

    public MemberController(IApiService api)
    {
        _api = api;
    }

    private async Task<int?> GetIdMembro()
    {
        var user = await _api.GetCurrentUserAsync();
        return user?.IdMembro;
    }

    private async Task<MemberProfileViewModel> GetProfileAsync(int idMembro)
    {
        var profile = await _api.GetMemberProfileAsync(idMembro);
        ViewData["MemberName"] = profile?.Name ?? "Membro";
        return profile ?? new MemberProfileViewModel();
    }

    // pequenos DTOs para binding de AJAX
    public class BookClassRequestDto { public int classId { get; set; } }
    public class CancelReservationRequestDto {
        public int MemberId { get; set; }
        public int ClassId { get; set; }
    }

    // -------------------------
    // DASHBOARD
    // -------------------------
    public async Task<IActionResult> Dashboard()
    {
        var idMembro = await GetIdMembro();
        if (idMembro == null) return Unauthorized();

        var profile = await GetProfileAsync(idMembro.Value);
        var recentAssessment = await _api.GetLatestPhysicalAssessmentAsync(idMembro.Value);
        var trainingPlan = await _api.GetCurrentTrainingPlanAsync() ?? new TrainingPlanViewModel();
        var reservations = await _api.GetUserReservationsAsync(idMembro.Value);
        var classesToday = await _api.GetAvailableClassesAsync();

        var model = new DashboardViewModel
        {
            Profile = profile,
            RecentAssessment = recentAssessment,
            TrainingPlan = trainingPlan,
            UpcomingReservations = reservations.Take(3).ToList(),
            AvailableClassesToday = classesToday
        };

        return View(model);
    }

    // -------------------------
    // PROFILE
    // -------------------------
    public async Task<IActionResult> Profile()
    {
        var idMembro = await GetIdMembro();
        if (idMembro == null) return Unauthorized();

        var profile = await GetProfileAsync(idMembro.Value);
        return View(profile);
    }

    [AllowAnonymous]
    public async Task<IActionResult> GetMemberName()
    {
        var idMembro = await GetIdMembro();
        if (idMembro == null) return Json(null);

        var profile = await _api.GetMemberProfileAsync(idMembro.Value);
        if (profile == null) return Json(null);

        return Json(profile.Name);
    }

    // -------------------------
    // CLASSES
    // -------------------------
    public async Task<IActionResult> Classes()
    {
        var idMembro = await GetIdMembro();
        if (idMembro == null) return Unauthorized();

        var profile = await GetProfileAsync(idMembro.Value);
        var classes = await _api.GetAvailableClassesAsync();
        var reservations = await _api.GetUserReservationsAsync(idMembro.Value);

        var model = new ClassesViewModel
        {
            AvailableClasses = classes,
            UserReservations = reservations
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> BookClass([FromQuery] int id)
    {
        var idMembro = await GetIdMembro();
        if (idMembro == null) return Unauthorized();

        var reservationId = await _api.BookClassAsync(idMembro.Value, id);
        if (reservationId == null) return BadRequest(new { message = "Erro ao reservar." });

        // Busca reservas atualizadas
        var reservations = await _api.GetUserReservationsAsync(idMembro.Value);

        return PartialView("UserReservationsPartial", reservations);
    }

    [HttpPatch]
    public async Task<IActionResult> CancelReservation([FromQuery] CancelReservationRequestDto request)
    {
        if (request.MemberId <= 0 || request.ClassId <= 0)
            return BadRequest(new { message = "Dados inválidos." });

        await _api.CancelReservationAsync(request.MemberId, request.ClassId);

        var reservations = await _api.GetUserReservationsAsync(request.MemberId);
        return PartialView("UserReservationsPartial", reservations);
    }




    // -------------------------
    // TRAINING PLAN
    // -------------------------
    public async Task<IActionResult> TrainingPlan()
    {
        var idMembro = await GetIdMembro();
        if (idMembro == null) return Unauthorized();

        var profile = await GetProfileAsync(idMembro.Value);
        var plan = await _api.GetCurrentTrainingPlanAsync();

        if (plan == null || plan.Exercises.Count == 0)
            return RedirectToAction("Dashboard");

        return View(plan);
    }

    // -------------------------
    // PHYSICAL ASSESSMENT
    // -------------------------
    public async Task<IActionResult> PhysicalAssessment()
    {
        var idMembro = await GetIdMembro();
        if (idMembro == null) return Unauthorized();

        var profile = await GetProfileAsync(idMembro.Value);
        var latestAssessment = await _api.GetLatestPhysicalAssessmentAsync(idMembro.Value);
        var nextReservation = await _api.GetActivePhysicalAssessmentAsync(idMembro.Value);

        var vm = new PhysicalAssessmentViewModel
        {
            LatestAssessment = latestAssessment,
            NextReservation = nextReservation,
            ReservationState = nextReservation != null
                ? (nextReservation.IsCancelled ? Presenca.Cancelado : Presenca.Reservado)
                : null
        };

        return View(vm);
    }

    [HttpPost]
    public async Task<IActionResult> BookPhysicalAssessment([FromBody] BookPhysicalAssessmentDto request)
    {
        var idMembro = await GetIdMembro();
        if (idMembro == null) return Unauthorized();

        if (string.IsNullOrWhiteSpace(request.DataReserva))
            return BadRequest(new { message = "DataReserva não fornecida." });

        if (!DateTime.TryParse(request.DataReserva, null, System.Globalization.DateTimeStyles.RoundtripKind, out var dataReserva))
            return BadRequest(new { message = "DataReserva inválida." });

        // ✅ Verifica se já existe reserva ativa
        var existing = await _api.GetActivePhysicalAssessmentAsync(idMembro.Value);
        if (existing != null)
            return BadRequest(new { message = "O membro já possui uma reserva ativa." });

        Reservation? reservation;
        try
        {
            // ✅ Cria reserva e retorna o objeto criado
            reservation = await _api.BookPhysicalAssessmentAsync(idMembro.Value, dataReserva);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }

        if (reservation == null)
            return BadRequest(new { message = "Erro ao criar a reserva." });

        // ✅ Retorna o ID válido da reserva imediatamente
        return Ok(new
        {
            IdMembroAvaliacao = reservation.Id,
            IdAvaliacaoFisica = reservation.AssessmentId,
            DataReserva = reservation.CreatedAt,
            Estado = 0
        });
    }
        
    [HttpPost]
    public async Task<IActionResult> CancelPhysicalAssessment([FromQuery] int reservationId)
    {
        var idMembro = await GetIdMembro();
        if (idMembro == null) return Unauthorized();

        var reservation = await _api.GetActivePhysicalAssessmentAsync(idMembro.Value);
        if (reservation == null)
            return NotFound(new { success = false, message = "Reserva não encontrada." });

        // ✅ Verifica se a reserva pertence ao membro
        if (reservation.Id != reservationId)
            return BadRequest(new { success = false, message = "Reserva não corresponde ao membro." });

        try
        {
            await _api.CancelPhysicalAssessmentAsync(reservation.Id);
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }

        return Ok(new { success = true });
    }
}
