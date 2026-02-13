using TTFWebsite.Models;
using TTFWebsite.Models.DTOs;

namespace TTFWebsite.Services
{
    public interface IApiService
    {
        // =========================
        // AUTENTICAÇÃO
        // =========================
        Task<TokenResponse?> LoginAsync(string email, string password);
        Task<bool> LogoutAsync(string token);
        Task<TokenResponse?> RefreshTokenAsync(string refreshToken);

        // =========================
        // PASSWORD
        // =========================
        Task<ApiResult> ChangePasswordAsync(string currentPassword, string newPassword);
        Task<bool> ResetPasswordAsync(string email);

        // =========================
        // USER / CONTEXTO
        // =========================
        Task<CurrentUserDto?> GetCurrentUserAsync();

        // =========================
        // PERFIL MEMBRO
        // =========================
        Task<MemberProfileViewModel?> GetMemberProfileAsync(int idMembro);

        Task<List<SubscriptionDto>> GetActiveSubscriptionsAsync();

        // =========================
        // RESERVAS / AULAS
        // =========================
        Task<List<ClassReservationDto>> GetUserReservationsAsync(int idMembro);
        Task<List<ScheduleClassDto>> GetAvailableClassesAsync();
        Task<int?> BookClassAsync(int idMembro, int classId);
        Task CancelReservationAsync(int reservationId, int classId);

        // =========================
        // AVALIAÇÕES FÍSICAS
        // =========================
        Task<PhysicalAssessment?> GetLatestPhysicalAssessmentAsync(int idMembro);
        Task<Reservation?> BookPhysicalAssessmentAsync(int idMembro, DateTime dataReserva);
        Task<Reservation?> GetActivePhysicalAssessmentAsync(int memberId);
        Task CancelPhysicalAssessmentAsync(int idAvaliacao);

        // =========================
        // TREINO
        // =========================
        Task<TrainingPlanViewModel?> GetCurrentTrainingPlanAsync();
    }

    // =========================
    // MODELOS AUXILIARES
    // =========================
    public class TokenResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public bool NeedsPasswordChange { get; set; }
        public string? Message { get; set; }
    }
}
