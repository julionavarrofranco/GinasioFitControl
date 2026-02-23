using System;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using FitControlAdmin.Models;
using System.Text.Json;
using System.Text.Json.Serialization;
using FitControlAdmin.Helper;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace FitControlAdmin.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private string? _accessToken;
        public string? CurrentAccessToken => _accessToken;

        public ApiService()
        {
            _baseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"] ?? "https://localhost:7267";
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(_baseUrl);
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        public void SetToken(string token)
        {
            _accessToken = token;
            _httpClient.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", token);
        }

            public async Task<TokenResponse?> LoginAsync(string email, string password)
        {
            var loginDto = new LoginDto { Email = email, Password = password };

            try
            {
                var response = await _httpClient.PostAsJsonAsync("/api/Auth/login", loginDto);

                // Em caso de erro da API, ler mensagem
                if (!response.IsSuccessStatusCode)
                {
                    var errorText = await response.Content.ReadAsStringAsync();
                    try
                    {
                        throw new Exception("Erro no login.");
                    }
                    catch
                    {
                        throw new Exception(errorText);
                    }
                }

                // Ler token da resposta
                var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>();
                if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.AccessToken))
                    throw new Exception("A resposta do servidor não contém um token válido.");

                // Extrair claim Tipo
                var tipo = JwtHelper.GetClaim(tokenResponse.AccessToken, "Tipo");

                // Bloquear membros (apenas funcionários)
                if (tipo == "Membro")
                {
                    throw new Exception("Apenas funcionários podem aceder à aplicação de gestão.");
                }

                // Guardar token
                SetToken(tokenResponse.AccessToken);

                return tokenResponse;
            }
            catch (Exception ex)
            {
                // Repassar mensagem para o LoginWindow
                throw new Exception(ex.Message);
            }
        }

        public async Task<List<MemberDto>?> GetUsersAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/Member");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<MemberDto>>();
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<UserDto?> GetUserAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/User/{id}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<UserDto>();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"GetUserAsync failed: {response.StatusCode} - {errorContent}");
                }
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetUserAsync exception: {ex.Message}");
                return null;
            }
        }

        public async Task<UserDto?> GetUserByIdAsync(int idUser, bool includeFuncionario = false, bool includeMembro = false)
        {
            try
            {
                var queryParams = new List<string>();
                if (includeFuncionario) queryParams.Add("includeFuncionario=true");
                if (includeMembro) queryParams.Add("includeMembro=true");

                var queryString = queryParams.Any() ? "?" + string.Join("&", queryParams) : "";
                var response = await _httpClient.GetAsync($"/api/User/{idUser}{queryString}");

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<UserDto>();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"GetUserByIdAsync failed: {response.StatusCode} - {errorContent}");
                }
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetUserByIdAsync exception: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> UpdateUserAsync(int id, UserUpdateDto updateDto)
        {
            try
            {
                var response = await _httpClient.PutAsJsonAsync($"/api/User/{id}", updateDto);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> UpdateMemberAsync(int idMembro, UpdateMemberDto updateDto)
        {
            try
            {
                var response = await _httpClient.PatchAsJsonAsync($"/api/Member/update-member/{idMembro}", updateDto);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return (true, null);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    var message = ExtractMessage(errorContent);
                    return (false, message ?? $"Erro ao atualizar membro ({response.StatusCode}).");
                }
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<List<EmployeeDto>?> GetAllEmployeesAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/Employee");
                System.Diagnostics.Debug.WriteLine($"GetAllEmployeesAsync: Response status: {response.StatusCode}");
                if (response.IsSuccessStatusCode)
                {
                    var employees = await response.Content.ReadFromJsonAsync<List<EmployeeDto>>();
                    System.Diagnostics.Debug.WriteLine($"GetAllEmployeesAsync: Found {employees?.Count ?? 0} employees");
                    return employees;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"GetAllEmployeesAsync: Error response: {errorContent}");
                }
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetAllEmployeesAsync: Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> UpdateEmployeeAsync(int idFuncionario, UpdateEmployeeDto updateDto)
        {
            try
            {
                var response = await _httpClient.PatchAsJsonAsync($"/api/Employee/{idFuncionario}", updateDto);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return (true, null);
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    var message = ExtractMessage(errorContent);
                    return (false, message ?? $"Erro ao atualizar funcionário ({response.StatusCode}).");
                }
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"/api/User/{id}");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> ChangeUserActiveStatusAsync(int idUser, bool isActive)
        {
            try
            {
                var statusDto = new UserStatusDto
                {
                    IdUser = idUser,
                    IsActive = isActive
                };
                var response = await _httpClient.PatchAsJsonAsync("/api/User/change-active-status", statusDto);
                if (response.IsSuccessStatusCode)
                    return (true, null);
                var errorContent = await response.Content.ReadAsStringAsync();
                try
                {
                    var json = System.Text.Json.JsonDocument.Parse(errorContent);
                    if (json.RootElement.TryGetProperty("message", out var msg))
                        return (false, msg.GetString());
                }
                catch { }
                return (false, errorContent ?? $"Erro ({response.StatusCode})");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> RegisterUserAsync(UserRegisterDto registerDto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("/api/Auth/register", registerDto);
                if (response.IsSuccessStatusCode)
                {
                    return (true, null);
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                var message = ExtractMessage(errorContent);
                return (false, message ?? $"Erro ao criar utilizador ({response.StatusCode}).");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<bool> LogoutAsync()
        {
            try
            {
                var response = await _httpClient.PostAsync("/api/Auth/logout", null);
                if (response.IsSuccessStatusCode)
                {
                    _accessToken = null;
                    _httpClient.DefaultRequestHeaders.Authorization = null;
                    return true;
                }
                return false;
            }
            catch
            {
                // Limpar token local mesmo que o logout falhe
                _accessToken = null;
                _httpClient.DefaultRequestHeaders.Authorization = null;
                return false;
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> ResetPasswordAsync(string email)
        {
            try
            {
                var resetDto = new ResetPasswordDto { Email = email };
                var response = await _httpClient.PostAsJsonAsync("/api/User/reset-password", resetDto);
                
                if (response.IsSuccessStatusCode)
                {
                    return (true, null);
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                var message = ExtractMessage(errorContent);
                return (false, message ?? $"Erro ao redefinir palavra-passe ({response.StatusCode}).");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<UserDto?> GetCurrentUserAsync()
        {
            try
            {
                // Usar endpoint /api/User/me
                var response = await _httpClient.GetAsync("/api/User/me");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<UserDto>();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"GetCurrentUserAsync failed: {response.StatusCode} - {errorContent}");
                }
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetCurrentUserAsync error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"GetCurrentUserAsync stack trace: {ex.StackTrace}");
                return null;
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> ChangePasswordAsync(string passwordAtual, string novaPassword)
        {
            try
            {
                var changePasswordDto = new ChangePasswordDto 
                { 
                    PasswordAtual = passwordAtual, 
                    NovaPassword = novaPassword 
                };
                
                System.Diagnostics.Debug.WriteLine($"ChangePasswordAsync: Attempting to change password (new password length: {novaPassword?.Length ?? 0})");
                
                var response = await _httpClient.PatchAsJsonAsync("/api/User/change-password", changePasswordDto);
                
                if (response.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine("ChangePasswordAsync: Password changed successfully");
                    return (true, null);
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"ChangePasswordAsync: Error response ({response.StatusCode}): {errorContent}");
                var message = ExtractMessage(errorContent);
                return (false, message ?? $"Erro ao alterar palavra-passe ({response.StatusCode}).");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ChangePasswordAsync: Exception: {ex.Message}");
                return (false, ex.Message);
            }
        }

        private static string? ExtractMessage(string content)
        {
            try
            {
                using var doc = JsonDocument.Parse(content);
                var root = doc.RootElement;
                
                // Tentar propriedade message
                if (root.TryGetProperty("message", out var messageElement) &&
                    messageElement.ValueKind == JsonValueKind.String)
                {
                    return messageElement.GetString();
                }
                
                // Tentar propriedade error
                if (root.TryGetProperty("error", out var errorElement) &&
                    errorElement.ValueKind == JsonValueKind.String)
                {
                    return errorElement.GetString();
                }
            }
            catch
            {
                // Em caso de falha de parse, devolver conteúdo raw
                if (!string.IsNullOrWhiteSpace(content) && content.Length < 500)
                {
                    return content;
                }
            }

            return null;
        }

        #region Exercise Methods

        public async Task<(bool Success, string? ErrorMessage, ExerciseResponseDto? Exercise)> CreateExerciseAsync(ExerciseDto exerciseDto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("/api/Exercise", exerciseDto);
                if (response.IsSuccessStatusCode)
                {
                    var exercise = await response.Content.ReadFromJsonAsync<ExerciseResponseDto>();
                    return (true, null, exercise);
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                var message = ExtractMessage(errorContent);
                return (false, message ?? $"Erro ao criar exercício ({response.StatusCode}).", null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message, null);
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> UpdateExerciseAsync(int idExercicio, UpdateExerciseDto updateDto)
        {
            try
            {
                var response = await _httpClient.PatchAsJsonAsync($"/api/Exercise/update-exercise/{idExercicio}", updateDto);
                if (response.IsSuccessStatusCode)
                {
                    return (true, null);
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                var message = ExtractMessage(errorContent);
                return (false, message ?? $"Erro ao atualizar exercício ({response.StatusCode}).");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> ChangeExerciseStatusAsync(int idExercicio, bool ativo)
        {
            try
            {
                var response = await _httpClient.PatchAsync($"/api/Exercise/change-active-status/{idExercicio}?ativo={ativo}", null);
                if (response.IsSuccessStatusCode)
                {
                    return (true, null);
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                var message = ExtractMessage(errorContent);
                return (false, message ?? $"Erro ao alterar estado do exercício ({response.StatusCode}).");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<List<ExerciseResponseDto>?> GetExercisesByStateAsync(bool ativo, bool ordenarAsc = true)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/Exercise/by-state?ativo={ativo}&ordenarAsc={ordenarAsc}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<ExerciseResponseDto>>();
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<ExerciseResponseDto>?> GetExercisesByMuscleGroupAsync(GrupoMuscular grupo, bool ordenarAsc = true)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/Exercise/by-muscle-group/{grupo}?ordenarAsc={ordenarAsc}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<ExerciseResponseDto>>();
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<ExerciseResponseDto>?> GetExercisesByNameAsync(string nome, bool ordenarAsc = true)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/Exercise/by-name?nome={Uri.EscapeDataString(nome)}&ordenarAsc={ordenarAsc}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<ExerciseResponseDto>>();
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region Payment Methods

        public async Task<(bool Success, string? ErrorMessage, PaymentResponseDto? Payment)> CreatePaymentAsync(PaymentDto paymentDto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("/api/Payment", paymentDto);
                if (response.IsSuccessStatusCode)
                {
                    var payment = await response.Content.ReadFromJsonAsync<PaymentResponseDto>();
                    return (true, null, payment);
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                var message = ExtractMessage(errorContent);
                
                // Usar conteúdo completo do erro se não houver mensagem extraída
                if (string.IsNullOrEmpty(message))
                {
                    message = errorContent.Length > 500 ? errorContent.Substring(0, 500) + "..." : errorContent;
                }
                
                return (false, message ?? $"Erro ao criar pagamento ({response.StatusCode}).", null);
            }
            catch (Exception ex)
            {
                return (false, $"Exceção: {ex.Message}\n\nStack: {ex.StackTrace}", null);
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> UpdatePaymentAsync(int idPagamento, UpdatePaymentDto updateDto)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"UpdatePaymentAsync: Updating payment {idPagamento}");
                System.Diagnostics.Debug.WriteLine($"UpdatePaymentAsync: MetodoPagamento={updateDto.MetodoPagamento}, EstadoPagamento={updateDto.EstadoPagamento}");

                var response = await _httpClient.PatchAsJsonAsync($"/api/Payment/update-payment/{idPagamento}", updateDto);
                System.Diagnostics.Debug.WriteLine($"UpdatePaymentAsync: Response status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    System.Diagnostics.Debug.WriteLine("UpdatePaymentAsync: Update successful");
                    return (true, null);
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"UpdatePaymentAsync: Error content: {errorContent}");

                var message = ExtractMessage(errorContent);
                return (false, message ?? $"Erro ao atualizar pagamento ({response.StatusCode}).");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpdatePaymentAsync: Exception: {ex.Message}");
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> ChangePaymentStatusAsync(int idPagamento, bool ativo)
        {
            try
            {
                var response = await _httpClient.PatchAsync($"/api/Payment/change-active-status/{idPagamento}?ativo={ativo}", null);
                if (response.IsSuccessStatusCode)
                {
                    return (true, null);
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                var message = ExtractMessage(errorContent);
                return (false, message ?? $"Erro ao alterar estado do pagamento ({response.StatusCode}).");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<List<PaymentResponseDto>?> GetPaymentsByActiveStateAsync(bool ativo)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/Payment/by-state?ativo={ativo}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<PaymentResponseDto>>();
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<PaymentResponseDto>?> GetPaymentsByDateAsync(DateTime inicio, DateTime fim)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/Payment/by-date?inicio={inicio:yyyy-MM-dd}&fim={fim:yyyy-MM-dd}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<PaymentResponseDto>>();
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<PaymentResponseDto>?> GetPaymentsByPaymentStateAsync(EstadoPagamento estado)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/Payment/by-state-payment?estado={estado}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<PaymentResponseDto>>();
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<(bool Success, string? ErrorMessage, decimal? Revenue)> GetMonthlyRevenueAsync(int ano, int mes)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/Payment/monthly-revenue?ano={ano}&mes={mes}");
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<JsonElement>();
                    if (result.TryGetProperty("receita", out var receitaElement))
                    {
                        var receita = receitaElement.GetDecimal();
                        return (true, null, receita);
                    }
                    return (false, "Resposta inválida do servidor.", null);
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                var message = ExtractMessage(errorContent);
                return (false, message ?? $"Erro ao obter receita mensal ({response.StatusCode}).", null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message, null);
            }
        }

        public async Task<DashboardSummaryDto?> GetDashboardSummaryAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/Dashboard/summary");
                if (response.IsSuccessStatusCode)
                    return await response.Content.ReadFromJsonAsync<DashboardSummaryDto>();
                return null;
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region Physical Evaluation Methods

        public async Task<(bool Success, string? ErrorMessage, PhysicalEvaluationResponseDto? Evaluation)> CreatePhysicalEvaluationAsync(PhysicalEvaluationDto evaluationDto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("/api/PhysicalEvaluation", evaluationDto);
                if (response.IsSuccessStatusCode)
                {
                    var evaluation = await response.Content.ReadFromJsonAsync<PhysicalEvaluationResponseDto>();
                    return (true, null, evaluation);
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                var message = ExtractMessage(errorContent);
                return (false, message ?? $"Erro ao criar avaliação física ({response.StatusCode}).", null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message, null);
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> UpdatePhysicalEvaluationAsync(int idAvaliacao, UpdatePhysicalEvaluationDto updateDto)
        {
            try
            {
                var response = await _httpClient.PatchAsJsonAsync($"/api/PhysicalEvaluation/update/{idAvaliacao}", updateDto);
                if (response.IsSuccessStatusCode)
                {
                    return (true, null);
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                var message = ExtractMessage(errorContent);
                return (false, message ?? $"Erro ao atualizar avaliação física ({response.StatusCode}).");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> ChangePhysicalEvaluationStatusAsync(int idAvaliacao, bool ativo)
        {
            try
            {
                var response = await _httpClient.PatchAsync($"/api/PhysicalEvaluation/change-active-status/{idAvaliacao}?ativo={ativo}", null);
                if (response.IsSuccessStatusCode)
                {
                    return (true, null);
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                var message = ExtractMessage(errorContent);
                return (false, message ?? $"Erro ao alterar estado da avaliação física ({response.StatusCode}).");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<List<PhysicalEvaluationResponseDto>?> GetAllEvaluationsForMemberAsync(int idMembro)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/PhysicalEvaluation/member/evaluations/{idMembro}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<PhysicalEvaluationResponseDto>>();
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<PhysicalEvaluationResponseDto?> GetLatestEvaluationForMemberAsync(int idMembro)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/PhysicalEvaluation/member/latest-evaluation/{idMembro}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<PhysicalEvaluationResponseDto>();
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<PhysicalEvaluationHistoryDto>?> GetAllEvaluationsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/PhysicalEvaluation/all");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<PhysicalEvaluationHistoryDto>>();
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        #endregion

 #region Physical Evaluation Reservation Methods

        public async Task<(bool Success, string? ErrorMessage, PhysicalEvaluationReservationResponseDto? Reservation)> CreateReservationAsync(int idMembro, DateTime dataReserva)
        {
            try
            {
                var response = await _httpClient.PostAsync($"/api/PhysicalEvaluationReservation/{idMembro}?dataReserva={dataReserva:yyyy-MM-ddTHH:mm:ss}", null);
                if (response.IsSuccessStatusCode)
                {
                    var rawReservation = await response.Content.ReadFromJsonAsync<MembroAvaliacao>();
                    if (rawReservation != null)
                    {
                        var reservation = new PhysicalEvaluationReservationResponseDto
                        {
                            IdAvaliacao = rawReservation.IdMembroAvaliacao,
                            IdMembro = rawReservation.IdMembro,
                            IdFuncionario = null,
                            DataAvaliacao = rawReservation.DataReserva,
                            Estado = rawReservation.Estado.ToString(),
                            NomeMembro = rawReservation.Membro?.Nome,
                            NomeFuncionario = null
                        };
                        return (true, null, reservation);
                    }
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                var message = ExtractMessage(errorContent);
                return (false, message ?? $"Erro ao criar reserva ({response.StatusCode}).", null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message, null);
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> CancelReservationAsync(int idMembro, int idAvaliacao)
        {
            try
            {
                var response = await _httpClient.PatchAsync($"/api/PhysicalEvaluationReservation/cancel/{idMembro}/{idAvaliacao}", null);
                if (response.IsSuccessStatusCode)
                {
                    return (true, null);
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                var message = ExtractMessage(errorContent);
                return (false, message ?? $"Erro ao cancelar reserva ({response.StatusCode}).");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> ConfirmReservationAsync(int idMembro, int idAvaliacao, int idFuncionario)
        {
            try
            {
                var confirmDto = new { IdFuncionario = idFuncionario };
                var response = await _httpClient.PatchAsJsonAsync($"/api/PhysicalEvaluationReservation/confirm-reservation/{idMembro}/{idAvaliacao}", confirmDto);
                if (response.IsSuccessStatusCode)
                {
                    return (true, null);
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                var message = ExtractMessage(errorContent);
                return (false, message ?? $"Erro ao confirmar reserva ({response.StatusCode}).");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> MarkAttendanceAsync(int idMembro, int idAvaliacao, MarkAttendanceDto markAttendanceDto)
        {
            try
            {
                var response = await _httpClient.PatchAsJsonAsync($"/api/PhysicalEvaluationReservation/attendance/{idMembro}/{idAvaliacao}", markAttendanceDto);
                if (response.IsSuccessStatusCode)
                {
                    return (true, null);
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                var message = ExtractMessage(errorContent);
                return (false, message ?? $"Erro ao marcar presença ({response.StatusCode}).");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<List<MemberEvaluationReservationSummaryDto>?> GetActiveReservationsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/PhysicalEvaluationReservation/active");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<MemberEvaluationReservationSummaryDto>>();
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<MemberEvaluationReservationSummaryDto>?> GetCompletedReservationsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/PhysicalEvaluationReservation/completed");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<MemberEvaluationReservationSummaryDto>>();
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region Member Methods

        public async Task<List<MemberDto>?> GetAllMembersAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/Member");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<MemberDto>>();
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region Subscription Methods

        private static readonly JsonSerializerOptions SubscriptionJsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };

        public async Task<List<SubscriptionResponseDto>?> GetSubscriptionsByStateAsync(bool ativo, bool ordenarNomeAsc = true)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/Subscription/by-state?ativo={ativo}&ordenarNomeAsc={ordenarNomeAsc}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<SubscriptionResponseDto>>(SubscriptionJsonOptions);
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> UpdateSubscriptionAsync(int idSubscricao, UpdateSubscriptionDto updateDto)
        {
            try
            {
                var response = await _httpClient.PatchAsJsonAsync($"/api/Subscription/update-subscription/{idSubscricao}", updateDto);
                if (response.IsSuccessStatusCode)
                {
                    return (true, null);
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                var message = ExtractMessage(errorContent);
                return (false, message ?? $"Erro ao atualizar subscrição ({response.StatusCode}).");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string? ErrorMessage, SubscriptionResponseDto? Subscription)> CreateSubscriptionAsync(CreateSubscriptionDto createDto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("/api/Subscription", createDto, SubscriptionJsonOptions);
                if (response.IsSuccessStatusCode)
                {
                    var subscription = await response.Content.ReadFromJsonAsync<SubscriptionResponseDto>(SubscriptionJsonOptions);
                    return (true, null, subscription);
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                var message = ExtractMessage(errorContent);
                return (false, message ?? $"Erro ao criar subscrição ({response.StatusCode}).", null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message, null);
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> ChangeSubscriptionStatusAsync(int idSubscricao, bool ativo)
        {
            try
            {
                var response = await _httpClient.PatchAsync($"/api/Subscription/change-active-status/{idSubscricao}?ativo={ativo}", null);
                if (response.IsSuccessStatusCode)
                {
                    return (true, null);
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                var message = ExtractMessage(errorContent);
                return (false, message ?? $"Erro ao alterar estado da subscrição ({response.StatusCode}).");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        #endregion

        #region Class Management Methods

        // Obtém aulas do PT (templates para agendar)
        public async Task<List<AulaResponseDto>?> GetClassesByPtAsync(int idFuncionario)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/Class/by-pt/{idFuncionario}");
                if (response.IsSuccessStatusCode)
                {
                    var classes = await response.Content.ReadFromJsonAsync<List<AulaResponseDto>>();
                    return classes ?? new List<AulaResponseDto>();
                }
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetClassesByPtAsync: Exception: {ex.Message}");
                return null;
            }
        }

        // Obtém aulas por estado (ativo ou inativo)
        public async Task<List<AulaResponseDto>?> GetClassesByStateAsync(bool ativo)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/Class/by-state?ativo={ativo}");
                if (response.IsSuccessStatusCode)
                {
                    var classes = await response.Content.ReadFromJsonAsync<List<AulaResponseDto>>();
                    System.Diagnostics.Debug.WriteLine($"GetClassesByStateAsync(ativo={ativo}): Retrieved {classes?.Count ?? 0} classes");
                    return classes;
                }
                var errorContent = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"GetClassesByStateAsync(ativo={ativo}): {response.StatusCode} - {errorContent}");
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetClassesByStateAsync: Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<List<AulaResponseDto>?> GetAllClassesAsync()
        {
            try
            {
                // API expõe by-state; combinar ativas e inativas
                var activeClasses = await GetClassesByStateAsync(ativo: true);
                var inactiveClasses = await GetClassesByStateAsync(ativo: false);
                if (activeClasses != null || inactiveClasses != null)
                {
                    var combined = new List<AulaResponseDto>();
                    if (activeClasses != null) combined.AddRange(activeClasses);
                    if (inactiveClasses != null) combined.AddRange(inactiveClasses);
                    System.Diagnostics.Debug.WriteLine($"GetAllClassesAsync: Total {combined.Count} classes (by-state)");
                    return combined;
                }

                // Fallback para outros endpoints
                var endpoints = new[]
                {
                    "/api/Class",
                    "/api/Class/GetAll",
                    "/api/Class/List",
                    "/api/Aulas",
                    "/api/Aulas/GetAll",
                    "/api/Aulas/List"
                };

                foreach (var endpoint in endpoints)
                {
                    System.Diagnostics.Debug.WriteLine($"GetAllClassesAsync: Trying endpoint {endpoint}");
                    var response = await _httpClient.GetAsync(endpoint);
                    if (response.IsSuccessStatusCode)
                    {
                        var classes = await response.Content.ReadFromJsonAsync<List<AulaResponseDto>>();
                        System.Diagnostics.Debug.WriteLine($"GetAllClassesAsync: Retrieved {classes?.Count ?? 0} classes from {endpoint}");
                        return classes;
                    }
                }

                System.Diagnostics.Debug.WriteLine("GetAllClassesAsync: No working endpoint found for getting classes");
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetAllClassesAsync: Exception: {ex.Message}");
                return null;
            }
        }

        public async Task<(bool Success, string? ErrorMessage, AulaResponseDto? Class)> CreateClassAsync(AulaDto classDto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("/api/Class", classDto);
                if (response.IsSuccessStatusCode)
                {
                    var classResponse = await response.Content.ReadFromJsonAsync<AulaResponseDto>();
                    return (true, null, classResponse);
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                var message = ExtractMessage(errorContent);
                return (false, message ?? $"Erro ao criar aula ({response.StatusCode}).", null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message, null);
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> UpdateClassAsync(int idAula, UpdateClassDto updateDto)
        {
            try
            {
                var response = await _httpClient.PatchAsJsonAsync($"/api/Class/update/{idAula}", updateDto);
                if (response.IsSuccessStatusCode)
                {
                    return (true, null);
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                var message = ExtractMessage(errorContent);
                return (false, message ?? $"Erro ao atualizar aula ({response.StatusCode}).");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        // Desativa uma aula (soft-delete)
        public async Task<(bool Success, string? ErrorMessage)> DeleteClassAsync(int idAula)
        {
            try
            {
                // API usa change-active-status para soft-delete
                var response = await _httpClient.PatchAsync($"/api/Class/change-active-status/{idAula}?ativo=false", null);
                if (response.IsSuccessStatusCode)
                {
                    return (true, null);
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                var message = ExtractMessage(errorContent);
                return (false, message ?? $"Erro ao desativar aula ({response.StatusCode}).");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        // Reativa uma aula desativada
        public async Task<(bool Success, string? ErrorMessage)> ReactivateClassAsync(int idAula)
        {
            try
            {
                var response = await _httpClient.PatchAsync($"/api/Class/change-active-status/{idAula}?ativo=true", null);
                if (response.IsSuccessStatusCode)
                {
                    return (true, null);
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                var message = ExtractMessage(errorContent);
                return (false, message ?? $"Erro ao reativar aula ({response.StatusCode}).");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<List<ClassReservationSummaryDto>?> GetClassReservationsAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/Class/reservations");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<ClassReservationSummaryDto>>();
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<ClassAttendanceDto?> GetClassAttendanceAsync(int idAulaMarcada)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/Class/attendance/{idAulaMarcada}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ClassAttendanceDto>();
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> MarkClassAttendanceAsync(int idAulaMarcada, List<MemberReservationDto> attendance)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"/api/Class/attendance/{idAulaMarcada}", attendance);
                if (response.IsSuccessStatusCode)
                {
                    return (true, null);
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                var message = ExtractMessage(errorContent);
                return (false, message ?? $"Erro ao marcar presença ({response.StatusCode}).");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool Success, string? ErrorMessage)> CreateClassScheduleAsync(int idAula, DateTime startDate, DateTime endDate)
        {
            try
            {
                var scheduleDto = new
                {
                    IdAula = idAula,
                    StartDate = startDate,
                    EndDate = endDate
                };

                var response = await _httpClient.PostAsJsonAsync("/api/Class/schedule", scheduleDto);
                if (response.IsSuccessStatusCode)
                {
                    return (true, null);
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                var message = ExtractMessage(errorContent);
                return (false, message ?? $"Erro ao criar agendamento ({response.StatusCode}).");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        // Cria uma aula agendada individual
        public async Task<(bool Success, string? ErrorMessage, AulaMarcadaDto? Data)> CreateScheduledClassAsync(ScheduleClassDto dto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync("/api/ScheduleClass/create", dto);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<AulaMarcadaDto>();
                    return (true, null, result);
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                var message = ExtractMessage(errorContent);
                return (false, message ?? $"Erro ao criar aula agendada ({response.StatusCode}).", null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message, null);
            }
        }


        // Cancela uma aula agendada
        public async Task<(bool Success, string? ErrorMessage)> CancelScheduledClassAsync(int idAulaMarcada)
        {
            try
            {
                var response = await _httpClient.PatchAsync($"/api/ScheduleClass/cancel/{idAulaMarcada}", null);
                if (response.IsSuccessStatusCode)
                {
                    return (true, null);
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                var message = ExtractMessage(errorContent);
                return (false, message ?? $"Erro ao cancelar aula ({response.StatusCode}).");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        // Obtém aulas agendadas por PT (próximas 2 semanas)
        public async Task<List<AulaMarcadaResponseDto>?> GetScheduledClassesByPTAsync(int idFuncionario)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/MemberClass/by-pt/{idFuncionario}");
                if (response.IsSuccessStatusCode)
                {
                    // Converter resposta da API para AulaMarcadaResponseDto
                    var reservations = await response.Content.ReadFromJsonAsync<List<ClassReservationSummaryDto>>();
                    if (reservations == null) return null;

                    // Converter para formato esperado
                    var result = reservations.Select(r => new AulaMarcadaResponseDto
                    {
                        IdAulaMarcada = r.IdAulaMarcada,
                        NomeAula = r.NomeAula,
                        DataAula = r.DataAula,
                        Sala = r.Sala,
                        HoraInicio = r.HoraInicio,
                        HoraFim = r.HoraFim,
                        Capacidade = r.Capacidade,
                        TotalReservas = r.TotalReservas
                    }).ToList();

                    return result;
                }
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetScheduledClassesByPTAsync: Exception: {ex.Message}");
                return null;
            }
        }

        // Obtém detalhes de uma aula agendada para marcar presenças
        public async Task<ClassAttendanceDto?> GetScheduledClassForAttendanceAsync(int idAulaMarcada)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/MemberClass/attendance/{idAulaMarcada}");
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<ClassAttendanceDto>();
                }
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetScheduledClassForAttendanceAsync: Exception: {ex.Message}");
                return null;
            }
        }

        // Marca presenças numa aula agendada
        public async Task<(bool Success, string? ErrorMessage)> MarkAttendanceAsync(int idAulaMarcada, List<int> idsPresentes)
        {
            try
            {
                var response = await _httpClient.PatchAsJsonAsync($"/api/MemberClass/mark-attendance/{idAulaMarcada}", idsPresentes);
                if (response.IsSuccessStatusCode)
                {
                    return (true, null);
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                var message = ExtractMessage(errorContent);
                return (false, message ?? $"Erro ao marcar presenças ({response.StatusCode}).");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        #endregion

        #region Training Plan Methods

        // Cria um plano de treino
        public async Task<(bool Success, string? ErrorMessage, TrainingPlanSummaryDto? Data)> CreateTrainingPlanAsync(int idFuncionario, TrainingPlanDto dto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"/api/TrainingPlan?idFuncionario={idFuncionario}", dto);
                if (response.IsSuccessStatusCode)
                {
                    // API retorna dados do plano em camelCase
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var plano = await response.Content.ReadFromJsonAsync<TrainingPlanDetailDto>(options);
                    if (plano != null)
                    {
                        var summary = new TrainingPlanSummaryDto
                        {
                            IdPlano = plano.IdPlano,
                            Nome = plano.Nome,
                            DataCriacao = plano.DataCriacao,
                            Ativo = true
                        };
                        return (true, null, summary);
                    }
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                var message = ExtractMessage(errorContent);
                return (false, message ?? $"Erro ao criar plano ({response.StatusCode}).", null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message, null);
            }
        }

        // Atualiza um plano de treino
        public async Task<(bool Success, string? ErrorMessage)> UpdateTrainingPlanAsync(int idPlano, UpdateTrainingPlanDto dto)
        {
            try
            {
                var response = await _httpClient.PatchAsJsonAsync($"/api/TrainingPlan/{idPlano}", dto);
                if (response.IsSuccessStatusCode)
                    return (true, null);

                var errorContent = await response.Content.ReadAsStringAsync();
                var message = ExtractMessage(errorContent);
                return (false, message ?? $"Erro ao atualizar plano ({response.StatusCode}).");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        // Ativa ou desativa um plano de treino
        public async Task<(bool Success, string? ErrorMessage)> ChangeTrainingPlanStateAsync(int idPlano, bool ativo)
        {
            try
            {
                var response = await _httpClient.PatchAsync($"/api/TrainingPlan/change-active-state/{idPlano}?ativo={ativo}", null);
                if (response.IsSuccessStatusCode)
                    return (true, null);

                var errorContent = await response.Content.ReadAsStringAsync();
                var message = ExtractMessage(errorContent);
                return (false, message ?? $"Erro ao alterar estado ({response.StatusCode}).");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        // Lista planos por estado (ativo ou inativo)
        public async Task<List<TrainingPlanSummaryDto>?> GetTrainingPlansByStateAsync(bool ativo)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/TrainingPlan/summary?ativo={ativo}");
                if (response.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    return await response.Content.ReadFromJsonAsync<List<TrainingPlanSummaryDto>>(options);
                }
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetTrainingPlansByStateAsync: {ex.Message}");
                return null;
            }
        }

        // Obtém detalhes de um plano (nome, observações, exercícios)
        public async Task<TrainingPlanDetailDto?> GetTrainingPlanDetailAsync(int idPlano)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/TrainingPlan/{idPlano}");
                if (response.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    options.Converters.Add(new JsonStringEnumConverter());
                    var detail = await response.Content.ReadFromJsonAsync<TrainingPlanDetailDto>(options);
                    return detail;
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        // Atribui plano a um membro
        public async Task<(bool Success, string? ErrorMessage)> AssignTrainingPlanToMemberAsync(int idMembro, int idPlano)
        {
            try
            {
                var response = await _httpClient.PostAsync($"/api/TrainingPlan/assign-to-member?idMembro={idMembro}&idPlano={idPlano}", null);
                if (response.IsSuccessStatusCode)
                    return (true, null);

                var errorContent = await response.Content.ReadAsStringAsync();
                var message = ExtractMessage(errorContent);
                return (false, message ?? $"Erro ao atribuir plano ({response.StatusCode}).");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        // Remove plano do membro
        public async Task<(bool Success, string? ErrorMessage)> RemoveTrainingPlanFromMemberAsync(int idMembro)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"/api/TrainingPlan/remove-from-member/{idMembro}");
                if (response.IsSuccessStatusCode)
                    return (true, null);

                var errorContent = await response.Content.ReadAsStringAsync();
                var message = ExtractMessage(errorContent);
                return (false, message ?? $"Erro ao remover plano ({response.StatusCode}).");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        // Obtém o plano atual de um membro (com exercícios)
        public async Task<MemberTrainingPlanDto?> GetCurrentMemberPlanAsync(int idMembro)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/TrainingPlan/current/{idMembro}");
                if (response.IsSuccessStatusCode)
                {
                    // API pode retornar estrutura diferente de MemberTrainingPlanDto
                    var dto = await response.Content.ReadFromJsonAsync<MemberTrainingPlanDto>();
                    if (dto != null) return dto;
                    var detail = await response.Content.ReadFromJsonAsync<TrainingPlanDetailDto>();
                    if (detail != null)
                    {
                        return new MemberTrainingPlanDto
                        {
                            NomePlano = detail.Nome,
                            Observacoes = detail.Observacoes,
                            DataCriacao = detail.DataCriacao,
                            CriadoPor = detail.NomeFuncionario ?? "",
                            Exercicios = detail.Exercicios ?? new List<TrainingPlanExerciseDto>()
                        };
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetCurrentMemberPlanAsync: {ex.Message}");
                return null;
            }
        }

        // Histórico de planos do membro
        public async Task<List<TrainingPlanSummaryDto>?> GetMemberPlanHistoryAsync(int idMembro)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/TrainingPlan/history/{idMembro}");
                if (response.IsSuccessStatusCode)
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var list = await response.Content.ReadFromJsonAsync<List<TrainingPlanSummaryDto>>(options);
                    return list ?? new List<TrainingPlanSummaryDto>();
                }
                return null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetMemberPlanHistoryAsync: {ex.Message}");
                return null;
            }
        }

        #endregion

        #region Exercise Plan Methods

        // Adiciona exercício ao plano
        public async Task<(bool Success, string? ErrorMessage)> AddExerciseToPlanAsync(int idPlano, ExercisePlanDto dto)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync($"/api/ExercisePlan/{idPlano}", dto);
                if (response.IsSuccessStatusCode)
                    return (true, null);

                var errorContent = await response.Content.ReadAsStringAsync();
                var message = ExtractMessage(errorContent);
                return (false, message ?? $"Erro ao adicionar exercício ({response.StatusCode}).");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        // Atualiza exercício no plano
        public async Task<(bool Success, string? ErrorMessage)> UpdateExerciseInPlanAsync(int idPlano, int idExercicio, UpdateExercisePlanDto dto)
        {
            try
            {
                var response = await _httpClient.PatchAsJsonAsync($"/api/ExercisePlan/{idPlano}/{idExercicio}", dto);
                if (response.IsSuccessStatusCode)
                    return (true, null);

                var errorContent = await response.Content.ReadAsStringAsync();
                var message = ExtractMessage(errorContent);
                return (false, message ?? $"Erro ao atualizar exercício ({response.StatusCode}).");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        // Remove exercício do plano
        public async Task<(bool Success, string? ErrorMessage)> RemoveExerciseFromPlanAsync(int idPlano, int idExercicio)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"/api/ExercisePlan/{idPlano}/{idExercicio}");
                if (response.IsSuccessStatusCode)
                    return (true, null);

                var errorContent = await response.Content.ReadAsStringAsync();
                var message = ExtractMessage(errorContent);
                return (false, message ?? $"Erro ao remover exercício ({response.StatusCode}).");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        #endregion
    }
}
