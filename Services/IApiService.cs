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

        // =========================
        // RESERVAS / AULAS
        // =========================
        Task<List<Reservation>> GetUserReservationsAsync(int idMembro);
        Task<List<Class>> GetAvailableClassesAsync();
        Task<bool> BookClassAsync(int idMembro, int classId);
        Task<bool> CancelReservationAsync(int reservationId);

        // =========================
        // AVALIAÇÕES FÍSICAS
        // =========================
        Task<PhysicalAssessment?> GetLatestPhysicalAssessmentAsync(int idMembro);
        Task<bool> BookPhysicalAssessmentAsync(int idMembro, DateTime dataReserva);
        Task<Reservation?> GetActivePhysicalAssessmentAsync(int memberId);
        Task<bool> CancelPhysicalAssessmentAsync(int memberId, int assessmentId);

        // =========================
        // TREINO
        // =========================
        Task<TrainingPlanViewModel?> GetTrainingPlanAsync(int idMembro);
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
