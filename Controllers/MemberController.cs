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

    // -------------------------
    // DASHBOARD
    // -------------------------
    public async Task<IActionResult> Dashboard()
    {
        Console.WriteLine("Dashboard chamado");
        var idMembro = await GetIdMembro();
        if (idMembro == null)
        {
            // Apenas retorna Unauthorized, middleware trata o redirect
            Console.WriteLine("ID do membro não encontrado. Provável JWT expirado ou refresh falhou.");
            return Unauthorized();
        }

        var profile = await _api.GetMemberProfileAsync(idMembro.Value) ?? new MemberProfileViewModel();
        var recentAssessment = await _api.GetLatestPhysicalAssessmentAsync(idMembro.Value);
        var trainingPlan = await _api.GetTrainingPlanAsync(idMembro.Value) ?? new TrainingPlanViewModel();
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

        return View(model); // View: Dashboard.cshtml
    }


    // -------------------------
    // PROFILE
    // -------------------------
    public async Task<IActionResult> Profile()
    {
        var idMembro = await GetIdMembro();
        if (idMembro == null) return Unauthorized();

        var profile = await _api.GetMemberProfileAsync(idMembro.Value) ?? new MemberProfileViewModel();
        return View(profile); // View: Profile.cshtml
    }

    // -------------------------
    // CLASSES
    // -------------------------
    public async Task<IActionResult> Classes()
    {
        var idMembro = await GetIdMembro();
        if (idMembro == null) return Unauthorized();

        var classes = await _api.GetAvailableClassesAsync();
        var reservations = await _api.GetUserReservationsAsync(idMembro.Value);

        var model = new ClassesViewModel
        {
            AvailableClasses = classes,
            UserReservations = reservations
        };

        return View(model); // View: Classes.cshtml
    }

    [HttpPost]
    public async Task<IActionResult> BookClass(int idAula)
    {
        var idMembro = await GetIdMembro();
        if (idMembro == null) return Unauthorized();

        await _api.BookClassAsync(idMembro.Value, idAula);
        return RedirectToAction("Classes");
    }

    [HttpPost]
    public async Task<IActionResult> CancelReservation(int reservationId)
    {
        await _api.CancelReservationAsync(reservationId);
        return RedirectToAction("Dashboard");
    }

    // -------------------------
    // TRAINING PLAN
    // -------------------------
    public async Task<IActionResult> TrainingPlan()
    {
        var idMembro = await GetIdMembro();
        if (idMembro == null) return Unauthorized();

        var plan = await _api.GetTrainingPlanAsync(idMembro.Value) ?? new TrainingPlanViewModel();
        return View(plan); // View: TrainingPlan.cshtml
    }

    // -------------------------
    // PHYSICAL ASSESSMENT
    // -------------------------
    public async Task<IActionResult> PhysicalAssessment()
    {
        var memberId = await GetIdMembro();
        if (memberId == null) return Unauthorized();

        var latestAssessment = await _api.GetLatestPhysicalAssessmentAsync(memberId.Value);
        var nextReservation = await _api.GetActivePhysicalAssessmentAsync(memberId.Value);

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

        if (!DateTime.TryParse(request.DataReserva, null, System.Globalization.DateTimeStyles.AssumeLocal, out var dataReserva))
            return BadRequest(new { message = "DataReserva inválida." });

        var errorMessage = await _api.BookPhysicalAssessmentAsync(idMembro.Value, dataReserva);

        if (!string.IsNullOrEmpty(errorMessage))
            return BadRequest(new { message = errorMessage });

        return Ok();
    }





    [HttpPost]
    public async Task<IActionResult> CancelPhysicalAssessment(int reservationId)
    {
        var idMembro = await GetIdMembro();
        if (idMembro == null) return Unauthorized();

        await _api.CancelPhysicalAssessmentAsync(reservationId);
        return RedirectToAction("PhysicalAssessment");
    }

}
