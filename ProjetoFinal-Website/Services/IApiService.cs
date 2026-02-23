using TTFWebsite.Models;
using TTFWebsite.Models.DTOs;

namespace TTFWebsite.Services
{
    /// <summary>
    /// Contrato da camada de integração com a FitControl API.
    /// Implementação principal: <see cref="ApiService"/>.
    /// </summary>
    public interface IApiService
    {
        Task<TokenResponse?> LoginAsync(string email, string password);
        Task<bool> LogoutAsync(string token);
        Task<TokenResponse?> RefreshTokenAsync(string refreshToken);
        Task<ApiResult> ChangePasswordAsync(string currentPassword, string newPassword);
        Task<bool> ResetPasswordAsync(string email);
        Task<CurrentUserDto?> GetCurrentUserAsync();
        Task<MemberProfileViewModel?> GetMemberProfileAsync(int idMembro);
        Task<List<SubscriptionDto>> GetActiveSubscriptionsAsync();
        Task<List<ClassReservationDto>> GetUserReservationsAsync();
        Task<List<ScheduleClassDto>> GetAvailableClassesAsync();
        Task<(ApiResult result, int? classId)> BookClassAsync(int classId);
        Task<ApiResult> CancelReservationAsync(int classId);
        Task<PhysicalAssessment?> GetLatestPhysicalAssessmentAsync(int idMembro);
        Task<Reservation?> BookPhysicalAssessmentAsync(int idMembro, DateTime dataReserva);
        Task<Reservation?> GetActivePhysicalAssessmentAsync(int memberId);
        Task CancelPhysicalAssessmentAsync(int idAvaliacao);
        Task<TrainingPlanViewModel?> GetCurrentTrainingPlanAsync();
    }
    public class TokenResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public bool NeedsPasswordChange { get; set; }
        public string? Message { get; set; }
    }
}
