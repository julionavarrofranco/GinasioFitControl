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

            public async Task<List<SubscriptionDto>> GetActiveSubscriptionsAsync()
            {
                try
                {
                    using var req = new HttpRequestMessage(HttpMethod.Get, "/api/Subscription/by-state?ativo=true");
                    var res = await _httpClient.SendAsync(req);

                    if (!res.IsSuccessStatusCode) return new List<SubscriptionDto>();

                    var subs = await res.Content.ReadFromJsonAsync<List<SubscriptionDto>>(_jsonOptions);
                    _logger.LogInformation("Subscrições recebidas: {Count}", subs?.Count ?? 0);
                    return subs ?? new List<SubscriptionDto>();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao obter subscrições ativas");
                    return new List<SubscriptionDto>();
                }
            }




            public async Task<PhysicalAssessment?> GetLatestPhysicalAssessmentAsync(int idMembro)
            {
                using var req = new HttpRequestMessage(HttpMethod.Get, $"/api/PhysicalEvaluation/member/latest-evaluation/{idMembro}");
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



        public async Task<Reservation?> BookPhysicalAssessmentAsync(int idMembro, DateTime dataReserva)
        {
            // Confere se já existe reserva ativa
            var existing = await GetActivePhysicalAssessmentAsync(idMembro);
            if (existing != null)
                return null; // já possui reserva ativa

            // Cria a reserva via POST
            var dataIso = Uri.EscapeDataString(dataReserva.ToString("yyyy-MM-ddTHH:mm:ss"));
            using var req = new HttpRequestMessage(HttpMethod.Post, $"/api/PhysicalEvaluationReservation/reservation?dataReserva={dataIso}");
            AddAuth(req);

            var res = await _httpClient.SendAsync(req);
            var content = await res.Content.ReadAsStringAsync();

            if (!res.IsSuccessStatusCode)
            {
                try
                {
                    var json = JsonDocument.Parse(content);
                    if (json.RootElement.TryGetProperty("id", out var idProp)) // assume a API retorna { id, ... }
                    {
                        var id = idProp.GetInt32();
                        return new Reservation
                        {
                            Id = id,
                            UserId = idMembro,
                            AssessmentId = json.RootElement.GetProperty("assessmentId").GetInt32(),
                            CreatedAt = dataReserva,
                            IsCancelled = false,
                            Type = ReservationType.PhysicalAssessment
                        };
                    }
                    else if (json.RootElement.TryGetProperty("message", out var msg))
                    {
                        throw new Exception(msg.GetString() ?? "Erro ao agendar.");
                    }
                }
                catch
                {
                    throw new Exception("Erro ao agendar avaliação: " + content);
                }
            }

            // ✅ Se a API retorna o objeto inteiro como JSON, desserializar
            var reservationDto = JsonSerializer.Deserialize<PhysicalReservationDto>(content, _jsonOptions);
            if (reservationDto == null) return null;

            return new Reservation
            {
                Id = reservationDto.IdMembroAvaliacao,
                UserId = reservationDto.IdMembro,
                AssessmentId = reservationDto.IdAvaliacaoFisica,
                CreatedAt = reservationDto.DataReserva,
                IsCancelled = reservationDto.DataCancelamento.HasValue,
                Type = ReservationType.PhysicalAssessment
            };
        }




        public async Task CancelPhysicalAssessmentAsync(int idReserva)
        {
            using var req = new HttpRequestMessage(HttpMethod.Patch, $"/api/PhysicalEvaluationReservation/cancel/{idReserva}");
            AddAuth(req);

            var res = await _httpClient.SendAsync(req);
            if (!res.IsSuccessStatusCode)
            {
                var content = await res.Content.ReadAsStringAsync();
                _logger.LogError("Erro ao cancelar avaliação: {Error}", content);
                throw new Exception("Erro ao cancelar avaliação.");
            }
        }

        public async Task<Reservation?> GetActivePhysicalAssessmentAsync(int idMembro)
        {
            using var req = new HttpRequestMessage(HttpMethod.Get, $"/api/PhysicalEvaluationReservation/active/{idMembro}");
            AddAuth(req);

            var res = await _httpClient.SendAsync(req);
            if (!res.IsSuccessStatusCode || res.StatusCode == HttpStatusCode.NoContent)
                return null;

            var dto = await res.Content.ReadFromJsonAsync<PhysicalReservationDto>(_jsonOptions);
            if (dto == null) return null;

            return new Reservation
            {
                Id = dto.IdMembroAvaliacao,
                UserId = dto.IdMembro,
                AssessmentId = dto.IdAvaliacaoFisica,
                CreatedAt = dto.DataReserva,
                IsCancelled = dto.DataCancelamento.HasValue,
                Type = ReservationType.PhysicalAssessment
            };
        }




        public async Task<TrainingPlanViewModel?> GetCurrentTrainingPlanAsync()
            {
                using var req = new HttpRequestMessage(HttpMethod.Get, "/api/TrainingPlan/current");
                AddAuth(req);

                var res = await _httpClient.SendAsync(req);
                if (!res.IsSuccessStatusCode || res.StatusCode == HttpStatusCode.NoContent)
                    return null;

                var dto = await res.Content.ReadFromJsonAsync<TrainingPlanDetailDto>(_jsonOptions);
                if (dto == null) return null;

                var vm = new TrainingPlanViewModel
                {
                    Name = dto.Nome,
                    CreatedBy = dto.NomeFuncionario ?? string.Empty,  // <--- aqui
                    CreatedDate = dto.DataCriacao,                    // <--- aqui
                    Observations = dto.Observacoes,
                    Exercises = dto.Exercicios.Select(e => new Exercise
                    {
                        Id = e.IdExercicio,
                        Name = e.NomeExercicio,
                        Description = e.Descricao ?? string.Empty,
                        Sets = e.Series,
                        Reps = e.Repeticoes,
                        Notes = string.Empty,
                        Load = e.Carga,
                        PhotoUrl = string.IsNullOrWhiteSpace(e.FotoUrl)
                            ? "/images/exercise-placeholder.png"
                            : e.FotoUrl,
                        MuscleGroup = e.GrupoMuscular ?? string.Empty
                    }).ToList()
                };


                return vm;
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




        public async Task<List<ClassReservationDto>> GetUserReservationsAsync()
        {
            using var req = new HttpRequestMessage(HttpMethod.Get, "/api/MemberClass/member-reservations");
            AddAuth(req);

            var res = await _httpClient.SendAsync(req);
            if (!res.IsSuccessStatusCode) return new List<ClassReservationDto>();

            var dtos = await res.Content.ReadFromJsonAsync<List<ClassReservationDto>>(_jsonOptions);
            return dtos ?? new List<ClassReservationDto>();
        }



        public async Task<(ApiResult result, int? classId)> BookClassAsync(int classId)
        {
            var url = $"/api/MemberClass/reserve?idAulaMarcada={classId}";
            using var req = new HttpRequestMessage(HttpMethod.Post, url);
            AddAuth(req);

            var res = await _httpClient.SendAsync(req);
            var content = await res.Content.ReadAsStringAsync();

            if (!res.IsSuccessStatusCode)
                return (new ApiResult
                {
                    Success = false,
                    ErrorMessage = ExtractApiMessage(content)
                }, null);

            var dto = JsonSerializer.Deserialize<ReserveClassResponseDto>(content, _jsonOptions);

            return (new ApiResult { Success = true }, dto?.IdAulaMarcada);
        }






        public async Task<ApiResult> CancelReservationAsync(int classId)
        {
            using var req = new HttpRequestMessage(HttpMethod.Patch,
                $"/api/MemberClass/cancel?idAulaMarcada={classId}");

            AddAuth(req);

            var res = await _httpClient.SendAsync(req);
            var content = await res.Content.ReadAsStringAsync();

            if (!res.IsSuccessStatusCode)
                return new ApiResult
                {
                    Success = false,
                    ErrorMessage = ExtractApiMessage(content)
                };

            return new ApiResult { Success = true };
        }





        // -------------------------
        // HELPERS
        // -------------------------

        private string ExtractApiMessage(string content)
        {
            try
            {
                var json = JsonDocument.Parse(content);

                if (json.RootElement.TryGetProperty("message", out var msg))
                    return msg.GetString() ?? "Erro inesperado.";

                if (json.RootElement.TryGetProperty("title", out var title))
                    return title.GetString() ?? content;

                return content;
            }
            catch
            {
                return content;
            }
        }


        private void AddAuth(HttpRequestMessage req)
            {
                var token = _httpContextAccessor.HttpContext?.User.FindFirst("jwt")?.Value;
                if (!string.IsNullOrEmpty(token))
                    req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }
    }
