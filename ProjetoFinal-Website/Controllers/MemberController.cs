using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TTFWebsite.Models;
using TTFWebsite.Models.DTOs;
using TTFWebsite.Services;
using TTFWebsite.ViewModels;


[Authorize(Policy = "PasswordChanged")]
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

    private async Task<IActionResult> RequireMemberAsync()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        HttpContext.Session.Clear();
        TempData["ErrorMessage"] = "Apenas membros podem aceder a esta área.";
        return RedirectToAction("Login", "Account");
    }

    private async Task<MemberProfileViewModel> GetProfileAsync(int idMembro)
    {
        var profile = await _api.GetMemberProfileAsync(idMembro);
        ViewData["MemberName"] = profile?.Name ?? "Membro";
        return profile ?? new MemberProfileViewModel();
    }

    public async Task<IActionResult> Dashboard()
    {
        var idMembro = await GetIdMembro();
        if (idMembro == null) return await RequireMemberAsync();

        var profile = await GetProfileAsync(idMembro.Value);
        var recentAssessment = await _api.GetLatestPhysicalAssessmentAsync(idMembro.Value);
        var trainingPlan = await _api.GetCurrentTrainingPlanAsync() ?? new TrainingPlanViewModel();
        var reservations = await _api.GetUserReservationsAsync();
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

    public async Task<IActionResult> Profile()
    {
        var idMembro = await GetIdMembro();
        if (idMembro == null) return await RequireMemberAsync();

        var profile = await GetProfileAsync(idMembro.Value);

        var trainingPlan = await _api.GetCurrentTrainingPlanAsync();
        ViewBag.TrainingPlanName = trainingPlan?.Name;

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

    public async Task<IActionResult> Classes()
    {
        var idMembro = await GetIdMembro();
        if (idMembro == null) return await RequireMemberAsync();

        await GetProfileAsync(idMembro.Value);

        var classes = await _api.GetAvailableClassesAsync();
        var reservations = await _api.GetUserReservationsAsync();

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
        var (result, _) = await _api.BookClassAsync(id);

        if (!result.Success)
            return BadRequest(new { message = result.ErrorMessage });

        var reservations = await _api.GetUserReservationsAsync();
        return PartialView("UserReservationsPartial", reservations);
    }


    [HttpPatch]
    public async Task<IActionResult> CancelReservation(int classId)
    {
        if (classId <= 0)
            return BadRequest(new { message = "Dados inválidos." });

        var result = await _api.CancelReservationAsync(classId);

        if (!result.Success)
            return BadRequest(new { message = result.ErrorMessage });

        await Task.Delay(150);

        var reservations = await _api.GetUserReservationsAsync();
        return PartialView("UserReservationsPartial", reservations);
    }

    public async Task<IActionResult> TrainingPlan()
    {
        var idMembro = await GetIdMembro();
        if (idMembro == null) return await RequireMemberAsync();

        var profile = await GetProfileAsync(idMembro.Value);
        var plan = await _api.GetCurrentTrainingPlanAsync();

        if (plan == null || plan.Exercises.Count == 0)
            return RedirectToAction("Dashboard");

        return View(plan);
    }

    public async Task<IActionResult> PhysicalAssessment()
    {
        var idMembro = await GetIdMembro();
        if (idMembro == null) return await RequireMemberAsync();

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

        var existing = await _api.GetActivePhysicalAssessmentAsync(idMembro.Value);
        if (existing != null)
            return BadRequest(new { message = "O membro já possui uma reserva ativa." });

        Reservation? reservation;
        try
        {
            reservation = await _api.BookPhysicalAssessmentAsync(idMembro.Value, dataReserva);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }

        if (reservation == null)
            return BadRequest(new { message = "Erro ao criar a reserva." });

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
        if (idMembro == null) return await RequireMemberAsync();

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
