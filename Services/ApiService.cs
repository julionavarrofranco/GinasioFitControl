using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using TTFWebsite.Models;
using TTFWebsite.Models.DTOs;

namespace TTFWebsite.Services
{
    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ApiService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly JsonSerializerOptions _jsonOptions;

        public ApiService(HttpClient httpClient, ILogger<ApiService> logger, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;

            var baseUrl = configuration["ApiSettings:BaseUrl"] ?? "http://localhost:7267";
            _httpClient.BaseAddress = new Uri(baseUrl);
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        // -------------------------
        // AUTENTICAÇÃO
        // -------------------------
        public async Task<TokenResponse?> LoginAsync(string email, string password)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("/api/Auth/login", new { Email = email, Password = password });
                if (!response.IsSuccessStatusCode) return null;
                return await response.Content.ReadFromJsonAsync<TokenResponse>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login failed");
                return null;
            }
        }

        public async Task<bool> LogoutAsync(string token)
        {
            try
            {
                var req = new HttpRequestMessage(HttpMethod.Post, "/api/Auth/logout");
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var res = await _httpClient.SendAsync(req);
                return res.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Logout failed");
                return false;
            }
        }

        public async Task<TokenResponse?> RefreshTokenAsync(string refreshToken)
        {
            try
            {
                var res = await _httpClient.PostAsJsonAsync("/api/Auth/refresh", new { RefreshToken = refreshToken });
                if (!res.IsSuccessStatusCode) return null;
                return await res.Content.ReadFromJsonAsync<TokenResponse>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Refresh token failed");
                return null;
            }
        }

        // -------------------------
        // PASSWORD
        // -------------------------
        public async Task<ApiResult> ChangePasswordAsync(string currentPassword, string newPassword)
        {
            var token = _httpContextAccessor.HttpContext?.User.FindFirst("jwt")?.Value;
            if (string.IsNullOrEmpty(token))
                return new ApiResult { Success = false, ErrorMessage = "Sessão expirada" };

            var dto = new { PasswordAtual = currentPassword, NovaPassword = newPassword };
            using var req = new HttpRequestMessage(HttpMethod.Patch, "/api/User/change-password") { Content = JsonContent.Create(dto) };
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                var res = await _httpClient.SendAsync(req);
                if (res.IsSuccessStatusCode) return new ApiResult { Success = true };

                var content = await res.Content.ReadAsStringAsync();
                return new ApiResult { Success = false, ErrorMessage = content };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Change password failed");
                return new ApiResult { Success = false, ErrorMessage = ex.Message };
            }
        }

        public async Task<bool> ResetPasswordAsync(string email)
        {
            try
            {
                var res = await _httpClient.PostAsJsonAsync("/api/User/reset-password", new { Email = email });
                return res.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Reset password failed");
                return false;
            }
        }

        // -------------------------
        // USER / CURRENT
        // -------------------------
        public async Task<CurrentUserDto?> GetCurrentUserAsync()
        {
            using var req = new HttpRequestMessage(HttpMethod.Get, "/api/User/me");
            AddAuth(req);
            var res = await _httpClient.SendAsync(req);
            if (!res.IsSuccessStatusCode) return null;
            return await res.Content.ReadFromJsonAsync<CurrentUserDto>(_jsonOptions);
        }

        // -------------------------
        // PROFILE / MEMBER
        // -------------------------
        public async Task<MemberProfileViewModel?> GetMemberProfileAsync(int idMembro)
        {
            using var req = new HttpRequestMessage(HttpMethod.Get, $"/api/Member/{idMembro}/profile");
            AddAuth(req); // método que adiciona Authorization header

            var res = await _httpClient.SendAsync(req);
            if (!res.IsSuccessStatusCode)
                return null; // poderia lançar exceção ou logar

            var dto = await res.Content.ReadFromJsonAsync<MemberProfileDto>(_jsonOptions);
            if (dto == null)
                return null;

            // Mapear para ViewModel
            var viewModel = new MemberProfileViewModel
            {
                IdMembro = dto.IdMembro,
                Name = dto.Nome ?? "",
                Email = dto.Email ?? "",
                Phone = dto.Telemovel ?? "",
                BirthDate = dto.DataNascimento != DateTime.MinValue ? dto.DataNascimento : DateTime.MinValue,
                MembershipStartDate = dto.DataRegisto != DateTime.MinValue ? dto.DataRegisto : DateTime.MinValue,
                Plan = dto.Subscricao ?? "",
                PersonalTrainer = dto.PersonalTrainer ?? ""
            };

            return viewModel;
        }

        public async Task<PhysicalAssessment?> GetLatestPhysicalAssessmentAsync(int idMembro)
        {
            using var req = new HttpRequestMessage(HttpMethod.Get, $"/api/Member/{idMembro}/evaluations");
            AddAuth(req);

            var res = await _httpClient.SendAsync(req);
            if (!res.IsSuccessStatusCode) return null;

            var dto = await res.Content.ReadFromJsonAsync<PhysicalAssessmentDto>(_jsonOptions);
            if (dto == null) return null;

            // Mapear para o model usado na View
            var assessment = new PhysicalAssessment
            {
                AssessmentDate = dto.dataAvaliacao,
                Weight = dto.peso,
                Height = dto.altura,
                BMI = dto.imc,
                MuscleMass = dto.massaMuscular,
                BodyFat = dto.massaGorda,
                Notes = dto.observacoes,
                TrainerName = dto.avaliador
            };

            return assessment;
        }



        public async Task<string?> BookPhysicalAssessmentAsync(int idMembro, DateTime dataReserva)
        {
            var dataIso = Uri.EscapeDataString(
                dataReserva.ToString("yyyy-MM-ddTHH:mm:ss"));

            using var req = new HttpRequestMessage(
                HttpMethod.Post,
                $"/api/PhysicalEvaluationReservation/reservation?dataReserva={dataIso}");

            AddAuth(req); // importante

            var response = await _httpClient.SendAsync(req);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Erro ao agendar avaliação: {Error}", content);

                // tenta extrair message se for JSON
                try
                {
                    var json = System.Text.Json.JsonDocument.Parse(content);
                    if (json.RootElement.TryGetProperty("message", out var msg))
                        return msg.GetString();
                }
                catch
                {
                    // fallback: devolve todo o texto
                    return content;
                }
            }

            return null; // null significa sucesso
        }





        public async Task<bool> CancelPhysicalAssessmentAsync(int idAvaliacao)
        {
            using var req = new HttpRequestMessage(HttpMethod.Patch, $"/api/PhysicalEvaluationReservation/cancel/{idAvaliacao}");
            AddAuth(req);

            var res = await _httpClient.SendAsync(req);
            return res.IsSuccessStatusCode;
        }


        public async Task<Reservation?> GetActivePhysicalAssessmentAsync(int idMembro)
        {
            using var req = new HttpRequestMessage(
                HttpMethod.Get,
                $"/api/PhysicalEvaluationReservation/active/{idMembro}");

            AddAuth(req);

            var res = await _httpClient.SendAsync(req);
            if (!res.IsSuccessStatusCode || res.StatusCode == HttpStatusCode.NoContent)
                return null;

            var dto = await res.Content.ReadFromJsonAsync<PhysicalReservationDto>(_jsonOptions);
            if (dto == null) return null;

            return new Reservation
            {
                Id = dto.IdMembroAvaliacao,           // ou outro Id que faça sentido
                UserId = dto.IdMembro,
                AssessmentId = dto.IdAvaliacaoFisica,
                ReservationDate = dto.DataReserva,    // aqui agora pega corretamente
                IsCancelled = dto.DataCancelamento.HasValue,
                Type = ReservationType.PhysicalAssessment
            };
        }



        public async Task<TrainingPlanViewModel?> GetTrainingPlanAsync(int idMembro)
        {
            using var req = new HttpRequestMessage(HttpMethod.Get, $"/api/Member/{idMembro}/training-plan");
            AddAuth(req);

            var res = await _httpClient.SendAsync(req);
            if (!res.IsSuccessStatusCode) return null;
            return await res.Content.ReadFromJsonAsync<TrainingPlanViewModel>(_jsonOptions);
        }

        // -------------------------
        // CLASSES / RESERVATIONS
        // -------------------------
        public async Task<List<ScheduleClassDto>> GetAvailableClassesAsync()
        {
            using var req = new HttpRequestMessage(HttpMethod.Get, "/api/ScheduleClass/available");
            AddAuth(req);

            var res = await _httpClient.SendAsync(req);
            if (!res.IsSuccessStatusCode) return new List<ScheduleClassDto>();

            var dtoList = await res.Content.ReadFromJsonAsync<List<ScheduleClassDto>>(_jsonOptions);
            return dtoList ?? new List<ScheduleClassDto>();
        }




        public async Task<List<Reservation>> GetUserReservationsAsync(int idMembro)
        {
            using var req = new HttpRequestMessage(HttpMethod.Get, $"/api/MemberClass/member-reservations/{idMembro}");
            AddAuth(req);

            var res = await _httpClient.SendAsync(req);
            if (!res.IsSuccessStatusCode) return new List<Reservation>();
            return await res.Content.ReadFromJsonAsync<List<Reservation>>(_jsonOptions) ?? new List<Reservation>();
        }

        public async Task<bool> BookClassAsync(int idMembro, int classId)
        {
            using var req = new HttpRequestMessage(HttpMethod.Post, "/api/MemberClass/reserve")
            {
                Content = JsonContent.Create(new { IdMembro = idMembro, IdAula = classId })
            };
            AddAuth(req);

            var res = await _httpClient.SendAsync(req);
            return res.IsSuccessStatusCode;
        }

        public async Task<bool> CancelReservationAsync(int reservationId)
        {
            using var req = new HttpRequestMessage(HttpMethod.Patch, "/api/MemberClass/cancel")
            {
                Content = JsonContent.Create(new { IdAulaMarcada = reservationId })
            };
            AddAuth(req);

            var res = await _httpClient.SendAsync(req);
            return res.IsSuccessStatusCode;
        }

        // -------------------------
        // HELPERS
        // -------------------------
        private void AddAuth(HttpRequestMessage req)
        {
            var token = _httpContextAccessor.HttpContext?.User.FindFirst("jwt")?.Value;
            if (!string.IsNullOrEmpty(token))
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }
}
